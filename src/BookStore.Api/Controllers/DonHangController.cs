using BookStore.Domain.Enums;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

[ApiController]
[Route("api/donhang")]
[Authorize(Roles = TenVaiTro.QuanLy)]
public class DonHangController : ControllerBase
{
    private readonly IDonHangService _donHang;
    private readonly ILogger<DonHangController> _log;

    public DonHangController(IDonHangService donHang, ILogger<DonHangController> log)
    {
        _donHang = donHang;
        _log = log;
    }

    /// <summary>Danh sach don hang co loc theo trang thai, khoang ngay va tu khoa (D05).</summary>
    [HttpGet]
    public async Task<ActionResult<TrangKetQua<DonHangDto>>> Tim([FromQuery] TimDonHangDto dto)
        => Ok(await _donHang.TimAsync(dto));

    [HttpGet("{maDonHang:int}")]
    public async Task<ActionResult<DonHangDto>> ChiTiet(int maDonHang)
    {
        var don = await _donHang.LayChiTietAsync(maDonHang);
        return don is null ? NotFound(new { thongBao = "Không tìm thấy đơn hàng." }) : Ok(don);
    }

    public record ChuyenTrangThaiRequest(TrangThaiDonHang TrangThaiMoi, string? GhiChu);

    /// <summary>
    /// Nhan vien chuyen trang thai don hang. Tang Services kiem tra buoc chuyen
    /// co hop le theo so do trang thai hay khong va hoan ton kho khi huy don (BR03).
    /// </summary>
    [HttpPut("{maDonHang:int}/trangthai")]
    public async Task<IActionResult> ChuyenTrangThai(int maDonHang, [FromBody] ChuyenTrangThaiRequest req)
    {
        var nguoiThucHien = this.LayTenDangNhap();
        var maQuanLy = this.LayMaQuanLy();

        _log.LogInformation("{NguoiThucHien} yêu cầu chuyển đơn {MaDonHang} sang {TrangThai}.",
            nguoiThucHien, maDonHang, req.TrangThaiMoi);

        var kq = await _donHang.ChuyenTrangThaiAsync(maDonHang, req.TrangThaiMoi, nguoiThucHien, maQuanLy, req.GhiChu);
        return kq.ThanhCong ? Ok(new { thongBao = kq.ThongBao }) : BadRequest(new { thongBao = kq.ThongBao });
    }

    /// <summary>BR05: huy cac don VNPay qua han thanh toan.</summary>
    [HttpPost("huydonquahan")]
    [Authorize(Roles = TenVaiTro.QuanLy)]
    public async Task<IActionResult> HuyDonQuaHan()
    {
        var soDon = await _donHang.HuyDonQuaHanAsync();
        return Ok(new { soDon, thongBao = $"Đã hủy {soDon} đơn quá hạn thanh toán." });
    }
}

[ApiController]
[Route("api/baocao")]
[Authorize(Roles = TenVaiTro.QuanLy)]
public class BaoCaoController : ControllerBase
{
    private readonly IBaoCaoService _baoCao;
    private readonly IXuatExcelService _excel;

    public BaoCaoController(IBaoCaoService baoCao, IXuatExcelService excel)
    {
        _baoCao = baoCao;
        _excel = excel;
    }

    private const string KieuExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>So lieu cho dashboard D02.</summary>
    [HttpGet("thongke")]
    public async Task<ActionResult<ThongKeDto>> ThongKe() => Ok(await _baoCao.LayThongKeAsync());

    [HttpGet("doanhthu")]
    [Authorize(Roles = TenVaiTro.QuanLy)]
    public async Task<ActionResult<object>> DoanhThu([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
    {
        var doanhThu = await _baoCao.TinhDoanhThuAsync(tuNgay, denNgay);
        var top = await _baoCao.TopSachBanChayAsync(tuNgay, denNgay);
        return Ok(new { tuNgay, denNgay, doanhThu, topSachBanChay = top });
    }

    /// <summary>Bao cao doanh thu chi tiet theo ngay kem top sach ban chay (BR10).</summary>
    [HttpGet("doanhthu/chitiet")]
    public async Task<ActionResult<BaoCaoDoanhThuDto>> DoanhThuChiTiet([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        => KyHopLe(tuNgay, denNgay) ?? (ActionResult<BaoCaoDoanhThuDto>)Ok(await _baoCao.LayBaoCaoDoanhThuAsync(tuNgay, denNgay));

    [HttpGet("nhapxuatton")]
    public async Task<ActionResult<List<NhapXuatTonDto>>> NhapXuatTon([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        => KyHopLe(tuNgay, denNgay) ?? (ActionResult<List<NhapXuatTonDto>>)Ok(await _baoCao.LayNhapXuatTonAsync(tuNgay, denNgay));

    [HttpGet("dontheotrangthai")]
    public async Task<ActionResult<List<DonTheoTrangThaiDto>>> DonTheoTrangThai([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        => KyHopLe(tuNgay, denNgay) ?? (ActionResult<List<DonTheoTrangThaiDto>>)Ok(await _baoCao.DemDonTheoTrangThaiAsync(tuNgay, denNgay));

    [HttpGet("xuatexcel/doanhthu")]
    public async Task<IActionResult> XuatExcelDoanhThu([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
    {
        if (KyHopLe(tuNgay, denNgay) is { } loi) return loi;
        var tep = await _excel.XuatDoanhThuAsync(tuNgay, denNgay);
        return File(tep, KieuExcel, $"BaoCaoDoanhThu_{tuNgay:yyyyMMdd}_{denNgay:yyyyMMdd}.xlsx");
    }

    [HttpGet("xuatexcel/nhapxuatton")]
    public async Task<IActionResult> XuatExcelNhapXuatTon([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
    {
        if (KyHopLe(tuNgay, denNgay) is { } loi) return loi;
        var tep = await _excel.XuatNhapXuatTonAsync(tuNgay, denNgay);
        return File(tep, KieuExcel, $"BaoCaoNhapXuatTon_{tuNgay:yyyyMMdd}_{denNgay:yyyyMMdd}.xlsx");
    }

    private BadRequestObjectResult? KyHopLe(DateTime tuNgay, DateTime denNgay)
    {
        if (tuNgay == default || denNgay == default)
            return BadRequest(new { thongBao = "Vui lòng chọn khoảng thời gian." });
        if (tuNgay.Date > denNgay.Date)
            return BadRequest(new { thongBao = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc." });
        if ((denNgay.Date - tuNgay.Date).TotalDays > 366)
            return BadRequest(new { thongBao = "Khoảng thời gian báo cáo tối đa một năm." });
        return null;
    }
}
