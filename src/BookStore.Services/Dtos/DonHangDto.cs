using BookStore.Domain.Enums;

namespace BookStore.Services.Dtos;

public class GioHangDto
{
    public int MaGioHang { get; set; }
    public List<DongGioHangDto> Dong { get; set; } = new();

    public decimal TamTinh => Dong.Sum(d => d.ThanhTien);
    public int TongSoLuong => Dong.Sum(d => d.SoLuong);
}

public class DongGioHangDto
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public string? AnhBia { get; set; }
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; }
    public int SoLuongTon { get; set; }
    public decimal ThanhTien => DonGia * SoLuong;
}

/// <summary>Du lieu khach hang nhap o trang thanh toan (W05).</summary>
public class DatHangDto
{
    public string HoTenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string DiaChiGiao { get; set; } = string.Empty;
    public HinhThucThanhToan HinhThucThanhToan { get; set; } = HinhThucThanhToan.COD;
    public string? GhiChu { get; set; }
}

public class DonHangDto
{
    public int MaDonHang { get; set; }
    public int MaKhachHang { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
    public DateTime NgayDat { get; set; }
    public TrangThaiDonHang TrangThai { get; set; }
    public string TenTrangThai => MoTaTrangThai(TrangThai);
    public string HoTenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string DiaChiGiao { get; set; } = string.Empty;
    public decimal TamTinh { get; set; }
    public decimal PhiVanChuyen { get; set; }
    public decimal TongTien { get; set; }
    public HinhThucThanhToan HinhThucThanhToan { get; set; }
    public string TenHinhThucThanhToan => HinhThucThanhToan == HinhThucThanhToan.COD
        ? "Thanh toán khi nhận hàng (COD)" : "Thanh toán qua VNPay";
    public string? GhiChu { get; set; }
    public string? TenQuanLyXuLy { get; set; }
    public List<DongDonHangDto> Dong { get; set; } = new();
    public List<LichSuDonHangDto> LichSu { get; set; } = new();

    public static string MoTaTrangThai(TrangThaiDonHang tt) => tt switch
    {
        TrangThaiDonHang.ChoThanhToan => "Chờ thanh toán",
        TrangThaiDonHang.ChoXacNhan => "Chờ xác nhận",
        TrangThaiDonHang.DaXacNhan => "Đã xác nhận",
        TrangThaiDonHang.DangGiao => "Đang giao",
        TrangThaiDonHang.HoanThanh => "Hoàn thành",
        TrangThaiDonHang.DaHuy => "Đã hủy",
        _ => tt.ToString()
    };
}

public class DongDonHangDto
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public string? AnhBia { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
}

public class LichSuDonHangDto
{
    public DateTime ThoiDiem { get; set; }
    public string? TrangThaiCu { get; set; }
    public string TrangThaiMoi { get; set; } = string.Empty;
    public string? NguoiThucHien { get; set; }
    public string? GhiChu { get; set; }
}

/// <summary>Tieu chi loc danh sach don hang o ung dung quan tri (D03).</summary>
public class TimDonHangDto
{
    public string? TuKhoa { get; set; }
    public TrangThaiDonHang? TrangThai { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public int Trang { get; set; } = 1;
    public int SoDongMoiTrang { get; set; } = 20;
}

/// <summary>So lieu tong hop cho dashboard (D01).</summary>
public class ThongKeDto
{
    public int SoDonChoXacNhan { get; set; }
    public int SoDonDangGiao { get; set; }
    public int SoDonHoanThanhThangNay { get; set; }
    public decimal DoanhThuThangNay { get; set; }
    public int SoSachSapHetHang { get; set; }
    public List<SachBanChayDto> TopSachBanChay { get; set; } = new();
}

public class SachBanChayDto
{
    public string TenSach { get; set; } = string.Empty;
    public int SoLuongBan { get; set; }
    public decimal DoanhThu { get; set; }
}
