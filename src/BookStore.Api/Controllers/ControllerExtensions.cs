using System.Security.Claims;
using BookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

/// <summary>Cac ham tien ich doc thong tin nguoi dung tu token JWT.</summary>
public static class ControllerExtensions
{
    public static int? LayMaTaiKhoan(this ControllerBase c)
        => int.TryParse(c.User.FindFirstValue(ClaimTypes.NameIdentifier), out var ma) ? ma : null;

    public static int? LayMaQuanLy(this ControllerBase c)
        => int.TryParse(c.User.FindFirstValue(TokenService.ClaimMaQuanLy), out var ma) ? ma : null;

    public static string LayTenDangNhap(this ControllerBase c)
        => c.User.FindFirstValue(ClaimTypes.Name) ?? "không rõ";
}
