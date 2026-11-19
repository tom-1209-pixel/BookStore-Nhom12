using System.Text.Json.Serialization;

namespace BookStore.Admin.Models;

// Lop du lieu cho cac man hinh danh muc, kho, khach hang va bao cao.
// Khai bao rieng tuong ung voi JSON cua BookStore.Api (Desktop khong tham chieu BookStore.Services).

/// <summary>Mot muc cua bon danh muc: loai sach, tac gia, nha xuat ban, nha cung cap.</summary>
public class DanhMuc
{
    public int Ma { get; set; }
    public string Ten { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public int SoLienKet { get; set; }

    [JsonIgnore]
    public string ThongTinPhu => string.Join(" · ", new[] { SoDienThoai, DiaChi, Email, MoTa }
        .Where(x => !string.IsNullOrWhiteSpace(x)));

    public override string ToString() => Ten;
}

public class TonKho
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public string TenLoaiSach { get; set; } = string.Empty;
    public int SoLuongTon { get; set; }
    public int NguongCanhBao { get; set; }
    public byte TrangThai { get; set; }
    public bool SapHetHang { get; set; }
    public string CanhBao => SapHetHang ? "Sắp hết" : (TrangThai == 0 ? "Ngừng KD" : "Đủ hàng");
}

public class DongPhieuNhap
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGiaNhap { get; set; }

    [JsonIgnore] public decimal ThanhTien => SoLuong * DonGiaNhap;
    [JsonIgnore] public string MoTa => $"{TenSach} — {SoLuong} × {DonGiaNhap:#,##0} đ = {ThanhTien:#,##0} đ";
}

public class PhieuNhap
{
    public int MaPhieuNhap { get; set; }
    public int MaNCC { get; set; }
    public string TenNCC { get; set; } = string.Empty;
    public string? TenQuanLy { get; set; }
    public DateTime NgayLap { get; set; }
    public DateTime? NgayHoanThanh { get; set; }
    public decimal TongTien { get; set; }
    public byte TrangThai { get; set; }
    public string TenTrangThai { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public int SoDauSach { get; set; }
    public List<DongPhieuNhap> Dong { get; set; } = new();

    public bool DaHoanThanh => TrangThai == 1;
    public string NgayLapHienThi => NgayLap.ToString("dd/MM/yyyy HH:mm");
    public string TongTienHienThi => TongTien.ToString("#,##0") + " đ";
}

public class KhachHang
{
    public int MaKhachHang { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public DateTime NgayDangKy { get; set; }
    public byte TrangThai { get; set; }
    public string TenTrangThai { get; set; } = string.Empty;
    public int SoDonHang { get; set; }
    public decimal TongChiTieu { get; set; }
    public string? DiaChiMacDinh { get; set; }

    public bool BiKhoa => TrangThai == 0;
    public string NgayDangKyHienThi => NgayDangKy.ToString("dd/MM/yyyy");
    public string TongChiTieuHienThi => TongChiTieu.ToString("#,##0") + " đ";
}

public class DoanhThuNgay
{
    public DateTime Ngay { get; set; }
    public int SoDon { get; set; }
    public decimal DoanhThu { get; set; }
    public string NgayHienThi => Ngay.ToString("dd/MM");
    public string DoanhThuHienThi => DoanhThu.ToString("#,##0") + " đ";

    /// <summary>Do rong thanh bieu do (pixel), tinh theo ngay co doanh thu cao nhat trong ky.</summary>
    [JsonIgnore] public double DoRongThanh { get; set; }
}

public class BaoCaoDoanhThu
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public decimal TongDoanhThu { get; set; }
    public int SoDonHoanThanh { get; set; }
    public int SoSachBanRa { get; set; }
    public decimal GiaTriTrungBinhDon { get; set; }
    public List<DoanhThuNgay> TheoNgay { get; set; } = new();
    public List<SachBanChay> TopSachBanChay { get; set; } = new();

    public string TongDoanhThuHienThi => TongDoanhThu.ToString("#,##0") + " đ";
    public string TrungBinhHienThi => GiaTriTrungBinhDon.ToString("#,##0") + " đ";
}

public class NhapXuatTon
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int TonDauKy { get; set; }
    public int NhapTrongKy { get; set; }
    public int XuatTrongKy { get; set; }
    public int TonCuoiKy { get; set; }
}

public class DonTheoTrangThai
{
    public byte TrangThai { get; set; }
    public string TenTrangThai { get; set; } = string.Empty;
    public int SoDon { get; set; }
    public decimal TongTien { get; set; }
    public string TongTienHienThi => TongTien.ToString("#,##0") + " đ";
}

public class PhanHoiCoMa
{
    [JsonPropertyName("ma")]
    public int Ma { get; set; }

    [JsonPropertyName("thongBao")]
    public string ThongBao { get; set; } = string.Empty;
}
