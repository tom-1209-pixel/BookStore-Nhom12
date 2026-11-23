using BookStore.Domain.Enums;
using BookStore.Services.Common;
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

    /// <summary>Xoa sach. Theo BR09, sach da phat sinh giao dich chi bi ngung kinh doanh.</summary>
    [HttpDelete("{maSach:int}")]
    [Authorize(Roles = TenVaiTro.QuanLy)]
    public async Task<IActionResult> Xoa(int maSach)
    {
        var kq = await _sach.XoaAsync(maSach);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpGet("danhmuc")]
    public async Task<ActionResult<List<LoaiSachDto>>> DanhMuc() => Ok(await _sach.LayDanhMucAsync());

    /// <summary>
    /// Tai anh bia len (JPEG, PNG, WebP, toi da 5 MB). Tra ve duong dan de gan vao truong anhBia
    /// khi them / sua sach. Anh duoc luu vao thu muc dung chung ma trang Web cung doc.
    /// </summary>
    [HttpPost("anhbia")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AnhBia.DungLuongToiDa + 64 * 1024)]
    public async Task<IActionResult> TaiAnhBia(IFormFile tep,
        [FromServices] IConfiguration cauHinh, [FromServices] IWebHostEnvironment moiTruong)
    {
        if (tep is null || tep.Length == 0)
            return BadRequest(new { thongBao = "Chưa chọn tệp ảnh." });
        if (tep.Length > AnhBia.DungLuongToiDa)
            return BadRequest(new { thongBao = "Ảnh quá lớn, tối đa 5 MB." });

        var duoi = Path.GetExtension(tep.FileName).ToLowerInvariant();
        if (!AnhBia.DuoiChoPhep.Contains(duoi))
            return BadRequest(new { thongBao = "Chỉ nhận ảnh JPG, PNG hoặc WebP." });

        await using var luong = new MemoryStream();
        await tep.CopyToAsync(luong);
        var duLieu = luong.ToArray();
        if (!AnhBia.LaAnhHopLe(duLieu, duoi))
            return BadRequest(new { thongBao = "Tệp không phải ảnh hợp lệ." });

        // Ten tep do he thong dat, khong dung ten goc cua nguoi dung
        var tenTep = $"{Guid.NewGuid():N}{(duoi == ".jpeg" ? ".jpg" : duoi)}";
        var thuMuc = AnhBia.LayThuMuc(cauHinh, moiTruong.ContentRootPath);
        await System.IO.File.WriteAllBytesAsync(Path.Combine(thuMuc, tenTep), duLieu);

        return Ok(new { duongDan = $"{AnhBia.DuongDanUrl}/{tenTep}", thongBao = "Đã tải ảnh lên." });
    }
}
