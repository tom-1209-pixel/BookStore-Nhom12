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

        // Cong gia lap: nguoi dung chon thanh cong / huy, he thong ky ket qua roi chuyen ve buoc 2
        app.MapPost("/vnpay-gia-lap/xac-nhan", ([FromForm] IFormCollection form, IThanhToanService thanhToan) =>
        {
            var thamSo = form.Where(f => f.Key.StartsWith("vnp_", StringComparison.Ordinal))
                .ToDictionary(f => f.Key, f => f.Value.ToString());
            var thanhCong = form["ketQua"] == "thanhcong";

            var kq = thanhToan.TaoKetQuaGiaLap(thamSo, thanhCong);
            return kq.ThanhCong && kq.DuLieu is not null
                ? Results.Redirect(kq.DuLieu)
                : Results.Redirect($"/tai-khoan/don-hang?loi={Uri.EscapeDataString(kq.ThongBao)}");
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

    private static string LayIp(HttpContext ctx)
    {
        var ip = ctx.Connection.RemoteIpAddress;
        if (ip is null || IPAddress.IsLoopback(ip)) return "127.0.0.1";
        return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString();
    }
}
