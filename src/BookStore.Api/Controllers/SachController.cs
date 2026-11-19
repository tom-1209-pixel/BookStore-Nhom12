using BookStore.Domain.Enums;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

[ApiController]
[Route("api/sach")]
[Authorize(Roles = TenVaiTro.QuanLy)]
public class SachController : ControllerBase
{
    private readonly ISachService _sach;

    public SachController(ISachService sach) => _sach = sach;

    /// <summary>Tim kiem, loc va phan trang danh sach sach cho man hinh D03.</summary>
    [HttpGet]
    public async Task<ActionResult<TrangKetQua<SachDto>>> Tim([FromQuery] TimSachDto dto)
    {
        dto.ChiSachDangBan = false;   // ung dung quan tri xem ca sach ngung kinh doanh
        return Ok(await _sach.TimAsync(dto));
    }

    [HttpGet("{maSach:int}")]
    public async Task<ActionResult<SachDto>> ChiTiet(int maSach)
    {
        var s = await _sach.LayTheoMaAsync(maSach);
        return s is null ? NotFound(new { thongBao = "Không tìm thấy sách." }) : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> Them([FromBody] SachLuuDto dto)
    {
        var kq = await _sach.ThemAsync(dto);
        return kq.ThanhCong
            ? CreatedAtAction(nameof(ChiTiet), new { maSach = kq.DuLieu }, new { maSach = kq.DuLieu, thongBao = kq.ThongBao })
            : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpPut("{maSach:int}")]
    public async Task<IActionResult> CapNhat(int maSach, [FromBody] SachLuuDto dto)
    {
        dto.MaSach = maSach;
        var kq = await _sach.CapNhatAsync(dto);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    /// <summary>Xoa sach. Theo BR07, sach da phat sinh giao dich chi bi ngung kinh doanh.</summary>
    [HttpDelete("{maSach:int}")]
    [Authorize(Roles = TenVaiTro.QuanLy)]
    public async Task<IActionResult> Xoa(int maSach)
    {
        var kq = await _sach.XoaAsync(maSach);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpGet("danhmuc")]
    public async Task<ActionResult<List<LoaiSachDto>>> DanhMuc() => Ok(await _sach.LayDanhMucAsync());
}
