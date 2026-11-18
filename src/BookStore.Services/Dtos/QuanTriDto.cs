using BookStore.Domain.Enums;

namespace BookStore.Services.Dtos;

// ---------------------------------------------------------------- Danh muc

/// <summary>Bon loai danh muc dung chung mot man hinh quan ly (D04).</summary>
public enum LoaiDanhMuc
{
    LoaiSach,
    TacGia,
    NhaXuatBan,
    NhaCungCap
}

/// <summary>
/// Mot muc danh muc. Bon bang LoaiSach, TacGia, NhaXuatBan, NhaCungCap co cau truc
/// gan giong nhau nen dung chung mot DTO; truong nao khong ap dung thi de trong.
/// </summary>
public class DanhMucDto
{
    public int Ma { get; set; }
    public string Ten { get; set; } = string.Empty;

    /// <summary>Mo ta loai sach hoac tieu su tac gia.</summary>
    public string? MoTa { get; set; }
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }

    /// <summary>So sach (hoac so phieu nhap doi voi nha cung cap) dang lien ket.</summary>
    public int SoLienKet { get; set; }
}

// ---------------------------------------------------------------- Kho

public class TonKhoDto
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public string TenLoaiSach { get; set; } = string.Empty;
    public int SoLuongTon { get; set; }
    public int NguongCanhBao { get; set; }
    public TrangThaiSach TrangThai { get; set; }
    public bool SapHetHang => TrangThai == TrangThaiSach.DangKinhDoanh && SoLuongTon <= NguongCanhBao;
}

public class DongPhieuNhapDto
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien => SoLuong * DonGiaNhap;
}

public class PhieuNhapDto
{
    public int MaPhieuNhap { get; set; }
    public int MaNCC { get; set; }
    public string TenNCC { get; set; } = string.Empty;
    public string? TenQuanLy { get; set; }
    public DateTime NgayLap { get; set; }
    public DateTime? NgayHoanThanh { get; set; }
    public decimal TongTien { get; set; }
    public TrangThaiPhieuNhap TrangThai { get; set; }
    public string TenTrangThai => TrangThai == TrangThaiPhieuNhap.HoanThanh ? "Đã hoàn thành" : "Chờ nhập";
    public string? GhiChu { get; set; }
    public int SoDauSach { get; set; }
    public List<DongPhieuNhapDto> Dong { get; set; } = new();
}

/// <summary>Du lieu lap moi hoac sua phieu nhap.</summary>
public class PhieuNhapLuuDto
{
    public int MaNCC { get; set; }
    public string? GhiChu { get; set; }
    public List<DongPhieuNhapDto> Dong { get; set; } = new();
}

// ---------------------------------------------------------------- Khach hang

public class KhachHangDto
{
    public int MaKhachHang { get; set; }
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public DateTime NgayDangKy { get; set; }
    public TrangThaiTaiKhoan TrangThai { get; set; }
    public string TenTrangThai => TrangThai == TrangThaiTaiKhoan.HoatDong ? "Hoạt động" : "Bị khóa";
    public int SoDonHang { get; set; }

    /// <summary>Tong tien cac don Hoan thanh cua khach (BR09).</summary>
    public decimal TongChiTieu { get; set; }
    public string? DiaChiMacDinh { get; set; }
}

// ---------------------------------------------------------------- Bao cao

public class DoanhThuNgayDto
{
    public DateTime Ngay { get; set; }
    public int SoDon { get; set; }
    public decimal DoanhThu { get; set; }
}

public class BaoCaoDoanhThuDto
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public decimal TongDoanhThu { get; set; }
    public int SoDonHoanThanh { get; set; }
    public int SoSachBanRa { get; set; }
    public decimal GiaTriTrungBinhDon => SoDonHoanThanh == 0 ? 0 : Math.Round(TongDoanhThu / SoDonHoanThanh, 0);
    public List<DoanhThuNgayDto> TheoNgay { get; set; } = new();
    public List<SachBanChayDto> TopSachBanChay { get; set; } = new();
}

/// <summary>Mot dong bao cao nhap - xuat - ton cua mot quyen sach trong ky.</summary>
public class NhapXuatTonDto
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int TonDauKy { get; set; }
    public int NhapTrongKy { get; set; }
    public int XuatTrongKy { get; set; }
    public int TonCuoiKy { get; set; }
}

public class DonTheoTrangThaiDto
{
    public TrangThaiDonHang TrangThai { get; set; }
    public string TenTrangThai => DonHangDto.MoTaTrangThai(TrangThai);
    public int SoDon { get; set; }
    public decimal TongTien { get; set; }
}
