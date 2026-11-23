using System.Security.Claims;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Endpoints;

/// <summary>
/// Cac endpoint nhan form dang nhap, dang ky, dang xuat.
/// Dat o endpoint thay vi xu ly trong component vi thao tac ghi cookie
/// xac thuc phai thuc hien trong mot yeu cau HTTP thong thuong.
/// </summary>
public static class TaiKhoanEndpoints
{
    public const string ClaimMaKhachHang = "ma_khach_hang";

    public static void MapTaiKhoanEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tai-khoan/dang-nhap", async (
            HttpContext ctx,
            [FromForm] string tenDangNhap,
            [FromForm] string matKhau,
            [FromForm] string? duongDanTiep,
            ITaiKhoanService taiKhoan,
            ILogger<Nhan> log) =>
        {
            var kq = await taiKhoan.DangNhapAsync(
                new DangNhapDto { TenDangNhap = tenDangNhap ?? "", MatKhau = matKhau ?? "" },
                laUngDungQuanTri: false);

            if (!kq.ThanhCong || kq.DuLieu is null)
                return Results.Redirect($"/dang-nhap?loi={Uri.EscapeDataString(kq.ThongBao)}");

            await GhiCookieAsync(ctx, kq.DuLieu);
            log.LogInformation("Khách hàng {TenDangNhap} đăng nhập trang Web.", kq.DuLieu.TenDangNhap);

            var tiep = string.IsNullOrWhiteSpace(duongDanTiep) ? "/" : duongDanTiep;
            return Results.Redirect(tiep);
        });

        app.MapPost("/tai-khoan/dang-ky", async (
            HttpContext ctx,
            [FromForm] string tenDangNhap,
            [FromForm] string hoTen,
            [FromForm] string email,
            [FromForm] string? soDienThoai,
            [FromForm] string matKhau,
            [FromForm] string xacNhanMatKhau,
            ITaiKhoanService taiKhoan) =>
        {
            var dto = new DangKyDto
            {
                TenDangNhap = tenDangNhap ?? "",
                HoTen = hoTen ?? "",
                Email = email ?? "",
                SoDienThoai = soDienThoai,
                MatKhau = matKhau ?? "",
                XacNhanMatKhau = xacNhanMatKhau ?? ""
            };

            var kq = await taiKhoan.DangKyAsync(dto);
            if (!kq.ThanhCong || kq.DuLieu is null)
                return Results.Redirect($"/dang-ky?loi={Uri.EscapeDataString(kq.ThongBao)}");

            await GhiCookieAsync(ctx, kq.DuLieu);
            return Results.Redirect("/?thongBao=" + Uri.EscapeDataString("Đăng ký thành công. Chào mừng bạn!"));
        });

        app.MapPost("/tai-khoan/dang-xuat", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });

        app.MapPost("/tai-khoan/doi-mat-khau", async (
            HttpContext ctx,
            [FromForm] string matKhauCu,
            [FromForm] string matKhauMoi,
            [FromForm] string xacNhanMatKhauMoi,
            ITaiKhoanService taiKhoan) =>
        {
            var ma = LayMaTaiKhoan(ctx.User);
            if (ma is null) return Results.Redirect("/dang-nhap");

            var kq = await taiKhoan.DoiMatKhauAsync(ma.Value, new DoiMatKhauDto
            {
                MatKhauCu = matKhauCu ?? "",
                MatKhauMoi = matKhauMoi ?? "",
                XacNhanMatKhauMoi = xacNhanMatKhauMoi ?? ""
            });

            var tham = kq.ThanhCong ? "thongBao" : "loi";
            return Results.Redirect($"/tai-khoan/doi-mat-khau?{tham}={Uri.EscapeDataString(kq.ThongBao)}");
        });

        // ---------------- Tai khoan cua toi: thong tin ca nhan va so dia chi ----------------
        app.MapPost("/tai-khoan/thong-tin", async (
            HttpContext ctx,
            [FromForm] string? hoTen,
            [FromForm] string? soDienThoai,
            [FromForm] string? ngaySinh,
            [FromForm] string? gioiTinh,
            ITaiKhoanService taiKhoan) =>
        {
            var maKH = LayMaKhachHang(ctx.User);
            if (maKH is null) return Results.Redirect("/dang-nhap?duongDanTiep=/tai-khoan");

            var kq = await taiKhoan.CapNhatHoSoAsync(maKH.Value, new HoSoKhachHangDto
            {
                HoTen = hoTen ?? "",
                SoDienThoai = soDienThoai,
                NgaySinh = DateTime.TryParse(ngaySinh, out var ns) ? ns : null,
                GioiTinh = byte.TryParse(gioiTinh, out var gt) ? gt : null
            });

            // Doi ho ten thi ghi lai cookie de thanh dieu huong hien ten moi
            if (kq.ThanhCong && LayMaTaiKhoan(ctx.User) is { } maTK && await taiKhoan.LayNguoiDungAsync(maTK) is { } nd)
                await GhiCookieAsync(ctx, nd);
            return VeTrangTaiKhoan(kq);
        });

        app.MapPost("/tai-khoan/dia-chi/luu", async (
            HttpContext ctx,
            [FromForm] int maDiaChi,
            [FromForm] string? hoTenNguoiNhan,
            [FromForm] string? soDienThoai,
            [FromForm] string? diaChiChiTiet,
            [FromForm] string? phuongXa,
            [FromForm] string? quanHuyen,
            [FromForm] string? tinhThanh,
            [FromForm] string? laMacDinh,
            ITaiKhoanService taiKhoan) =>
        {
            var maKH = LayMaKhachHang(ctx.User);
            if (maKH is null) return Results.Redirect("/dang-nhap?duongDanTiep=/tai-khoan");

            var kq = await taiKhoan.LuuDiaChiAsync(maKH.Value, new DiaChiGiaoHangDto
            {
                MaDiaChi = maDiaChi,
                HoTenNguoiNhan = hoTenNguoiNhan ?? "",
                SoDienThoai = soDienThoai ?? "",
                DiaChiChiTiet = diaChiChiTiet ?? "",
                PhuongXa = phuongXa ?? "",
                QuanHuyen = quanHuyen ?? "",
                TinhThanh = tinhThanh ?? "",
                LaMacDinh = laMacDinh == "true"
            });
            return kq.ThanhCong || maDiaChi == 0
                ? VeTrangTaiKhoan(kq)
                : Results.Redirect($"/tai-khoan?sua={maDiaChi}&loi={Uri.EscapeDataString(kq.ThongBao)}");
        });

        app.MapPost("/tai-khoan/dia-chi/xoa", async (HttpContext ctx, [FromForm] int maDiaChi, ITaiKhoanService taiKhoan) =>
        {
            var maKH = LayMaKhachHang(ctx.User);
            if (maKH is null) return Results.Redirect("/dang-nhap?duongDanTiep=/tai-khoan");
            return VeTrangTaiKhoan(await taiKhoan.XoaDiaChiAsync(maKH.Value, maDiaChi));
        });

        app.MapPost("/tai-khoan/dia-chi/mac-dinh", async (HttpContext ctx, [FromForm] int maDiaChi, ITaiKhoanService taiKhoan) =>
        {
            var maKH = LayMaKhachHang(ctx.User);
            if (maKH is null) return Results.Redirect("/dang-nhap?duongDanTiep=/tai-khoan");
            return VeTrangTaiKhoan(await taiKhoan.DatDiaChiMacDinhAsync(maKH.Value, maDiaChi));
        });
    }

    private static IResult VeTrangTaiKhoan(BookStore.Services.Common.KetQua kq)
        => Results.Redirect($"/tai-khoan?{(kq.ThanhCong ? "thongBao" : "loi")}={Uri.EscapeDataString(kq.ThongBao)}");

    private static async Task GhiCookieAsync(HttpContext ctx, NguoiDungDto nd)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, nd.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name, nd.TenDangNhap),
            new(ClaimTypes.Email, nd.Email),
            new(ClaimTypes.Role, nd.VaiTro),
            new("ho_ten", nd.HoTen)
        };
        if (nd.MaKhachHang.HasValue)
            claims.Add(new Claim(ClaimMaKhachHang, nd.MaKhachHang.Value.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    public static int? LayMaKhachHang(ClaimsPrincipal user)
        => int.TryParse(user.FindFirst(ClaimMaKhachHang)?.Value, out var ma) ? ma : null;

    public static int? LayMaTaiKhoan(ClaimsPrincipal user)
        => int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var ma) ? ma : null;

    public static string LayHoTen(ClaimsPrincipal user)
        => user.FindFirst("ho_ten")?.Value ?? user.Identity?.Name ?? string.Empty;

    /// <summary>Lop trong chi dung lam tham so kieu cho ILogger.</summary>
    public sealed class Nhan { }
}
