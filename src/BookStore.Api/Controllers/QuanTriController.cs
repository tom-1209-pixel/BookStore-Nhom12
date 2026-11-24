using BookStore.Domain.Enums;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

/// <summary>Loai sach, tac gia, nha xuat ban, nha cung cap (D04).</summary>
[ApiController]
[Route("api/danhmuc/{loai}")]
[Authorize(Roles = TenVaiTro.QuanLy)]
public class DanhMucController : ControllerBase
{
    private readonly IDanhMucService _danhMuc;

    public DanhMucController(IDanhMucService danhMuc) => _danhMuc = danhMuc;

    /// <summary>loai: loaisach | tacgia | nhaxuatban | nhacungcap</summary>
    [HttpGet]
    public async Task<ActionResult<List<DanhMucDto>>> DanhSach(string loai, [FromQuery] string? tuKhoa)
        => DocLoai(loai) is { } l ? Ok(await _danhMuc.LayDanhSachAsync(l, tuKhoa)) : LoaiSai();

    [HttpPost]
    public async Task<IActionResult> Them(string loai, [FromBody] DanhMucDto dto)
    {
        if (DocLoai(loai) is not { } l) return LoaiSai();
        dto.Ma = 0;
        var kq = await _danhMuc.LuuAsync(l, dto);
        return kq.ThanhCong ? Ok(new { ma = kq.DuLieu, thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpPut("{ma:int}")]
    public async Task<IActionResult> CapNhat(string loai, int ma, [FromBody] DanhMucDto dto)
    {
        if (DocLoai(loai) is not { } l) return LoaiSai();
        dto.Ma = ma;
        var kq = await _danhMuc.LuuAsync(l, dto);
        return kq.ThanhCong ? Ok(new { ma, thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpDelete("{ma:int}")]
    public async Task<IActionResult> Xoa(string loai, int ma)
    {
        if (DocLoai(loai) is not { } l) return LoaiSai();
        var kq = await _danhMuc.XoaAsync(l, ma);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    private static LoaiDanhMuc? DocLoai(string loai) => loai.ToLowerInvariant() switch
    {
        "loaisach" => LoaiDanhMuc.LoaiSach,
        "tacgia" => LoaiDanhMuc.TacGia,
        "nhaxuatban" => LoaiDanhMuc.NhaXuatBan,
        "nhacungcap" => LoaiDanhMuc.NhaCungCap,
        _ => null
    };

    private BadRequestObjectResult LoaiSai()
        => BadRequest(new { thongBao = "Loại danh mục phải là loaisach, tacgia, nhaxuatban hoặc nhacungcap." });
}

/// <summary>Ton kho va phieu nhap (D06). Quy tac BR08 nam o tang Services.</summary>
[ApiController]
[Route("api/kho")]
[Authorize(Roles = TenVaiTro.QuanLy)]
public class KhoController : ControllerBase
{
    private readonly IKhoService _kho;
    private readonly ILogger<KhoController> _log;

    public KhoController(IKhoService kho, ILogger<KhoController> log)
    {
        _kho = kho;
        _log = log;
    }

    [HttpGet("tonkho")]
    public async Task<ActionResult<List<TonKhoDto>>> TonKho([FromQuery] string? tuKhoa, [FromQuery] bool chiSapHetHang = false)
        => Ok(await _kho.LayTonKhoAsync(tuKhoa, chiSapHetHang));

    public record NguongRequest(int Nguong);

    [HttpPut("tonkho/{maSach:int}/nguong")]
    public async Task<IActionResult> CapNhatNguong(int maSach, [FromBody] NguongRequest req)
    {
        var kq = await _kho.CapNhatNguongAsync(maSach, req.Nguong);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpGet("phieunhap")]
    public async Task<ActionResult<List<PhieuNhapDto>>> DanhSachPhieu(
        [FromQuery] TrangThaiPhieuNhap? trangThai, [FromQuery] string? tuKhoa)
        => Ok(await _kho.TimPhieuNhapAsync(trangThai, tuKhoa));

    [HttpGet("phieunhap/{ma:int}")]
    public async Task<ActionResult<PhieuNhapDto>> ChiTietPhieu(int ma)
    {
        var p = await _kho.LayPhieuNhapAsync(ma);
        return p is null ? NotFound(new { thongBao = "Không tìm thấy phiếu nhập." }) : Ok(p);
    }

    [HttpPost("phieunhap")]
    public async Task<IActionResult> LapPhieu([FromBody] PhieuNhapLuuDto dto)
    {
        var maQuanLy = this.LayMaQuanLy();
        if (maQuanLy is null) return BadRequest(new { thongBao = "Tài khoản đăng nhập không có hồ sơ quản lý." });

        var kq = await _kho.TaoPhieuNhapAsync(maQuanLy.Value, dto);
        return kq.ThanhCong ? Ok(new { ma = kq.DuLieu, thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpPut("phieunhap/{ma:int}")]
    public async Task<IActionResult> SuaPhieu(int ma, [FromBody] PhieuNhapLuuDto dto)
    {
        var kq = await _kho.CapNhatPhieuNhapAsync(ma, dto);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpPost("phieunhap/{ma:int}/hoanthanh")]
    public async Task<IActionResult> HoanThanhPhieu(int ma)
    {
        _log.LogInformation("{NguoiThucHien} xác nhận hoàn thành phiếu nhập {Ma}.", this.LayTenDangNhap(), ma);
        var kq = await _kho.HoanThanhPhieuNhapAsync(ma);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    [HttpDelete("phieunhap/{ma:int}")]
    public async Task<IActionResult> XoaPhieu(int ma)
    {
        var kq = await _kho.XoaPhieuNhapAsync(ma);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }
}

/// <summary>Quan ly khach hang (D07).</summary>
[ApiController]
[Route("api/khachhang")]
[Authorize(Roles = TenVaiTro.QuanLy)]
public class KhachHangController : ControllerBase
{
    private readonly IKhachHangService _khachHang;

    public KhachHangController(IKhachHangService khachHang) => _khachHang = khachHang;

    [HttpGet]
    public async Task<ActionResult<List<KhachHangDto>>> Tim([FromQuery] string? tuKhoa, [FromQuery] TrangThaiTaiKhoan? trangThai)
        => Ok(await _khachHang.TimAsync(tuKhoa, trangThai));

    [HttpGet("{ma:int}")]
    public async Task<ActionResult<KhachHangDto>> ChiTiet(int ma)
    {
        var kh = await _khachHang.LayChiTietAsync(ma);
        return kh is null ? NotFound(new { thongBao = "Không tìm thấy khách hàng." }) : Ok(kh);
    }

    public record TrangThaiRequest(bool Khoa, string? LyDo);

    [HttpPut("{ma:int}/trangthai")]
    public async Task<IActionResult> DoiTrangThai(int ma, [FromBody] TrangThaiRequest req)
    {
        var kq = await _khachHang.DoiTrangThaiAsync(ma, req.Khoa, req.LyDo);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }
}
