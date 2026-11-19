using BookStore.Api.Services;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

[ApiController]
[Route("api/xacthuc")]
public class XacThucController : ControllerBase
{
    private readonly ITaiKhoanService _taiKhoan;
    private readonly ITokenService _token;

    public XacThucController(ITaiKhoanService taiKhoan, ITokenService token)
    {
        _taiKhoan = taiKhoan;
        _token = token;
    }

    /// <summary>Dang nhap ung dung quan tri. Tai khoan KHACH HANG bi tu choi.</summary>
    [HttpPost("dangnhap")]
    [AllowAnonymous]
    public async Task<ActionResult<KetQuaDangNhapApiDto>> DangNhap([FromBody] DangNhapDto dto)
    {
        var kq = await _taiKhoan.DangNhapAsync(dto, laUngDungQuanTri: true);
        if (!kq.ThanhCong || kq.DuLieu is null)
            return Unauthorized(new { thongBao = kq.ThongBao });

        var (token, hetHan) = _token.TaoToken(kq.DuLieu);
        return Ok(new KetQuaDangNhapApiDto { Token = token, HetHan = hetHan, NguoiDung = kq.DuLieu });
    }

    /// <summary>Lay thong tin nguoi dang dang nhap, dung de kiem tra token con hieu luc.</summary>
    [HttpGet("toi")]
    [Authorize]
    public async Task<ActionResult<NguoiDungDto>> Toi()
    {
        var ma = this.LayMaTaiKhoan();
        var nd = ma is null ? null : await _taiKhoan.LayNguoiDungAsync(ma.Value);
        return nd is null ? NotFound() : Ok(nd);
    }

    [HttpPost("doimatkhau")]
    [Authorize]
    public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauDto dto)
    {
        var ma = this.LayMaTaiKhoan();
        if (ma is null) return Unauthorized();

        var kq = await _taiKhoan.DoiMatKhauAsync(ma.Value, dto);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }
}
