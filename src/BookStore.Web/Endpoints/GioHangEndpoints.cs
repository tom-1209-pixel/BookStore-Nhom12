using BookStore.Domain.Enums;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Endpoints;

/// <summary>
/// Endpoint cho cac thao tac gio hang va dat hang. Moi thao tac deu kiem tra
/// khach hang da dang nhap (BR01: phai dang nhap moi them duoc vao gio).
/// </summary>
public static class GioHangEndpoints
{
    public static void MapGioHangEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/gio-hang/them", async (
            HttpContext ctx,
            [FromForm] int maSach,
            [FromForm] int soLuong,
            [FromForm] string? duongDanTiep,
            IGioHangService gioHang) =>
        {
            var maKH = TaiKhoanEndpoints.LayMaKhachHang(ctx.User);
            if (maKH is null)
                return Results.Redirect($"/dang-nhap?duongDanTiep={Uri.EscapeDataString(duongDanTiep ?? "/")}" +
                                        "&loi=" + Uri.EscapeDataString("Vui lòng đăng nhập để thêm sách vào giỏ hàng."));

            var kq = await gioHang.ThemVaoGioAsync(maKH.Value, maSach, soLuong <= 0 ? 1 : soLuong);
            var tham = kq.ThanhCong ? "thongBao" : "loi";
            var ve = string.IsNullOrWhiteSpace(duongDanTiep) ? "/gio-hang" : duongDanTiep;
            var noi = ve.Contains('?') ? "&" : "?";
            return Results.Redirect($"{ve}{noi}{tham}={Uri.EscapeDataString(kq.ThongBao)}");
        });

        app.MapPost("/dat-hang", async (
            HttpContext ctx,
            [FromForm] string hoTenNguoiNhan,
            [FromForm] string soDienThoaiNhan,
            [FromForm] string diaChiGiao,
            [FromForm] int hinhThucThanhToan,
            [FromForm] string? ghiChu,
            IDonHangService donHang,
            ILogger<TaiKhoanEndpoints.Nhan> log) =>
        {
            var maKH = TaiKhoanEndpoints.LayMaKhachHang(ctx.User);
            if (maKH is null) return Results.Redirect("/dang-nhap?duongDanTiep=/thanh-toan");

            var dto = new DatHangDto
            {
                HoTenNguoiNhan = hoTenNguoiNhan ?? "",
                SoDienThoaiNhan = soDienThoaiNhan ?? "",
                DiaChiGiao = diaChiGiao ?? "",
                HinhThucThanhToan = (HinhThucThanhToan)hinhThucThanhToan,
                GhiChu = ghiChu
            };

            var kq = await donHang.DatHangAsync(maKH.Value, dto);

            if (!kq.ThanhCong || kq.DuLieu is null)
                return Results.Redirect("/thanh-toan?loi=" + Uri.EscapeDataString(kq.ThongBao));

            log.LogInformation("Khách hàng {MaKhachHang} đặt đơn {MaDonHang} thành công.",
                maKH.Value, kq.DuLieu.MaDonHang);

            // Don VNPay chuyen thang sang buoc thanh toan truc tuyen
            return kq.DuLieu.HinhThucThanhToan == HinhThucThanhToan.VNPay
                ? Results.Redirect($"/thanh-toan/vnpay/{kq.DuLieu.MaDonHang}")
                : Results.Redirect($"/ket-qua-dat-hang/{kq.DuLieu.MaDonHang}");
        });

        app.MapPost("/don-hang/huy", async (
            HttpContext ctx,
            [FromForm] int maDonHang,
            [FromForm] string? lyDo,
            IDonHangService donHang) =>
        {
            var maKH = TaiKhoanEndpoints.LayMaKhachHang(ctx.User);
            if (maKH is null) return Results.Redirect("/dang-nhap");

            var kq = await donHang.KhachHuyDonAsync(maDonHang, maKH.Value, lyDo);
            var tham = kq.ThanhCong ? "thongBao" : "loi";
            return Results.Redirect($"/tai-khoan/don-hang?{tham}={Uri.EscapeDataString(kq.ThongBao)}");
        });
    }
}
