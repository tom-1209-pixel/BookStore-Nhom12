using System.Net;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Endpoints;

/// <summary>
/// Thanh toan truc tuyen qua VNPay va dat lai mat khau qua email.
/// Nghiep vu ky / kiem tra chu ky nam o tang Services (ThanhToanService).
/// </summary>
public static class ThanhToanEndpoints
{
    public static void MapThanhToanEndpoints(this IEndpointRouteBuilder app)
    {
        // Buoc 1: tao yeu cau thanh toan va chuyen khach sang cong VNPay (hoac cong gia lap)
        app.MapGet("/thanh-toan/vnpay/{maDonHang:int}", async (
            HttpContext ctx, int maDonHang, IThanhToanService thanhToan) =>
        {
            var maKH = TaiKhoanEndpoints.LayMaKhachHang(ctx.User);
            if (maKH is null)
                return Results.Redirect($"/dang-nhap?duongDanTiep={Uri.EscapeDataString($"/ket-qua-dat-hang/{maDonHang}")}");

            var goc = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            var kq = await thanhToan.TaoYeuCauThanhToanAsync(maDonHang, maKH.Value, LayIp(ctx),
                $"{goc}/thanh-toan/vnpay-tra-ve", $"{goc}/vnpay-gia-lap");

            return kq.ThanhCong && kq.DuLieu is not null
                ? Results.Redirect(kq.DuLieu)
                : Results.Redirect($"/ket-qua-dat-hang/{maDonHang}?loi={Uri.EscapeDataString(kq.ThongBao)}");
        });

        // Buoc 2: cong thanh toan chuyen trinh duyet ve day kem ket qua da ky
        app.MapGet("/thanh-toan/vnpay-tra-ve", async (HttpContext ctx, IThanhToanService thanhToan) =>
        {
            var thamSo = ctx.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var kq = await thanhToan.XuLyKetQuaTraVeAsync(thamSo);

            var maDon = kq.DuLieu > 0 ? kq.DuLieu
                : int.TryParse(thamSo.GetValueOrDefault("vnp_TxnRef")?.Split('-')[0], out var m) ? m : 0;
            var tham = kq.ThanhCong ? "thongBao" : "loi";

            return maDon > 0
                ? Results.Redirect($"/ket-qua-dat-hang/{maDon}?{tham}={Uri.EscapeDataString(kq.ThongBao)}")
                : Results.Redirect($"/tai-khoan/don-hang?{tham}={Uri.EscapeDataString(kq.ThongBao)}");
        });

        // ---------------- Cong gia lap ----------------
        // Huy giao dich (moi buoc) hoac xac nhan da thanh toan bang VNPAY-QR
        app.MapPost("/vnpay-gia-lap/xac-nhan", ([FromForm] IFormCollection form, IThanhToanService thanhToan) =>
        {
            var thamSo = LayThamSoVnp(form);
            var thanhCong = form["ketQua"] == "thanhcong" && form["phuongThuc"] == "qr";

            var kq = thanhCong
                ? thanhToan.TaoKetQuaGiaLap(thamSo, true, nganHang: "VNPAYQR", loaiThe: "QRCODE")
                : thanhToan.TaoKetQuaGiaLap(thamSo, false, "24");
            return ChuyenKetQua(kq);
        });

        // Buoc nhap the ATM: kiem tra thong tin the, dung thi chuyen sang buoc OTP
        app.MapPost("/vnpay-gia-lap/the", ([FromForm] IFormCollection form, IThanhToanService thanhToan) =>
        {
            var thamSo = LayThamSoVnp(form);
            if (!thanhToan.KiemTraYeuCauGiaLap(thamSo))
                return Results.Redirect($"/tai-khoan/don-hang?loi={Uri.EscapeDataString("Yêu cầu thanh toán không hợp lệ.")}");

            var nganHang = form["nganHang"].ToString();
            var soThe = new string(form["soThe"].ToString().Where(char.IsDigit).ToArray());
            var tenChuThe = string.Join(' ', form["tenChuThe"].ToString().ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var ngayPhatHanh = form["ngayPhatHanh"].ToString().Trim();
            int.TryParse(form["lanSai"], out var lanSai);

            var the = TheGiaLap.FirstOrDefault(t => t.SoThe == soThe);
            var dungThongTin = the is not null && nganHang == "NCB"
                               && tenChuThe == the.TenChuThe && ngayPhatHanh == the.NgayPhatHanh;

            if (!dungThongTin)
            {
                lanSai++;
                if (lanSai >= 3)
                    return ChuyenKetQua(thanhToan.TaoKetQuaGiaLap(thamSo, false, "10", nganHang));

                return Results.Redirect(UrlCongGiaLap(thamSo, new()
                {
                    ["buoc"] = "the",
                    ["nganHangChon"] = nganHang,
                    ["lanSai"] = lanSai.ToString(),
                    ["loi"] = $"Thông tin thẻ không chính xác. Bạn còn {3 - lanSai} lần thử."
                }));
            }

            // The dung thong tin nhung bi tu choi (het tien, bi khoa...)
            if (the!.MaTuChoi is not null)
                return ChuyenKetQua(thanhToan.TaoKetQuaGiaLap(thamSo, false, the.MaTuChoi, nganHang));

            var bonSo = soThe[^4..];
            return Results.Redirect(UrlCongGiaLap(thamSo, new()
            {
                ["buoc"] = "otp",
                ["the"] = bonSo,
                ["xt"] = thanhToan.KyBuocGiaLap($"{thamSo.GetValueOrDefault("vnp_TxnRef")}|{bonSo}")
            }));
        });

        // Buoc OTP: chi chap nhan khi buoc nhap the da qua (chu ky xt hop le)
        app.MapPost("/vnpay-gia-lap/otp", ([FromForm] IFormCollection form, IThanhToanService thanhToan) =>
        {
            var thamSo = LayThamSoVnp(form);
            var bonSo = form["the"].ToString();
            var xt = form["xt"].ToString();
            if (!thanhToan.KiemTraYeuCauGiaLap(thamSo)
                || xt != thanhToan.KyBuocGiaLap($"{thamSo.GetValueOrDefault("vnp_TxnRef")}|{bonSo}"))
                return Results.Redirect($"/tai-khoan/don-hang?loi={Uri.EscapeDataString("Yêu cầu thanh toán không hợp lệ.")}");

            if (form["otp"].ToString().Trim() == MaOtpGiaLap)
                return ChuyenKetQua(thanhToan.TaoKetQuaGiaLap(thamSo, true));

            int.TryParse(form["lanSai"], out var lanSai);
            lanSai++;
            if (lanSai >= 3)
                return ChuyenKetQua(thanhToan.TaoKetQuaGiaLap(thamSo, false, "13"));

            return Results.Redirect(UrlCongGiaLap(thamSo, new()
            {
                ["buoc"] = "otp",
                ["the"] = bonSo,
                ["xt"] = xt,
                ["lanSai"] = lanSai.ToString(),
                ["loi"] = $"Mã OTP không đúng. Bạn còn {3 - lanSai} lần thử."
            }));
        });

        // ---------------- Quen mat khau ----------------
        app.MapPost("/tai-khoan/quen-mat-khau", async (
            HttpContext ctx, [FromForm] string email, ITaiKhoanService taiKhoan) =>
        {
            var goc = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            var kq = await taiKhoan.YeuCauDatLaiMatKhauAsync(email ?? "",
                token => $"{goc}/dat-lai-mat-khau?token={Uri.EscapeDataString(token)}");
            var tham = kq.ThanhCong ? "thongBao" : "loi";
            return Results.Redirect($"/quen-mat-khau?{tham}={Uri.EscapeDataString(kq.ThongBao)}");
        });

        app.MapPost("/tai-khoan/dat-lai-mat-khau", async (
            [FromForm] string token, [FromForm] string matKhauMoi, [FromForm] string xacNhanMatKhau,
            ITaiKhoanService taiKhoan) =>
        {
            var kq = await taiKhoan.DatLaiMatKhauAsync(token ?? "", matKhauMoi ?? "", xacNhanMatKhau ?? "");
            return kq.ThanhCong
                ? Results.Redirect($"/dang-nhap?thongBao={Uri.EscapeDataString(kq.ThongBao)}")
                : Results.Redirect($"/dat-lai-mat-khau?token={Uri.EscapeDataString(token ?? "")}&loi={Uri.EscapeDataString(kq.ThongBao)}");
        });
    }

    // The thu nghiem cua cong gia lap, theo bo the thu cua VNPay sandbox (ngan hang NCB)
    private sealed record TheThu(string SoThe, string TenChuThe, string NgayPhatHanh, string? MaTuChoi);

    private static readonly TheThu[] TheGiaLap =
    [
        new("9704198526191432198", "NGUYEN VAN A", "07/15", null),
        new("9704195798459170488", "NGUYEN VAN A", "07/15", "51"),   // khong du so du
        new("9704192181368742", "NGUYEN VAN A", "07/15", "12"),      // the bi khoa
        new("9704193370791314", "NGUYEN VAN A", "07/15", "09")       // chua dang ky Internet Banking
    ];

    private const string MaOtpGiaLap = "123456";

    private static Dictionary<string, string> LayThamSoVnp(IFormCollection form)
        => form.Where(f => f.Key.StartsWith("vnp_", StringComparison.Ordinal))
               .ToDictionary(f => f.Key, f => f.Value.ToString());

    private static string UrlCongGiaLap(Dictionary<string, string> thamSo, Dictionary<string, string?> them)
    {
        var tatCa = thamSo.ToDictionary(p => p.Key, p => (string?)p.Value);
        foreach (var p in them) tatCa[p.Key] = p.Value;
        return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("/vnpay-gia-lap", tatCa);
    }

    private static IResult ChuyenKetQua(BookStore.Services.Common.KetQua<string> kq)
        => kq.ThanhCong && kq.DuLieu is not null
            ? Results.Redirect(kq.DuLieu)
            : Results.Redirect($"/tai-khoan/don-hang?loi={Uri.EscapeDataString(kq.ThongBao)}");

    private static string LayIp(HttpContext ctx)
    {
        var ip = ctx.Connection.RemoteIpAddress;
        if (ip is null || IPAddress.IsLoopback(ip)) return "127.0.0.1";
        return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString();
    }
}
