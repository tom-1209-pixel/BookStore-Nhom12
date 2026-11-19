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
    }

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
