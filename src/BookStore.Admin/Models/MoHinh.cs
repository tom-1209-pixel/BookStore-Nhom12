using System.Text.Json.Serialization;

namespace BookStore.Admin.Models;

// Ung dung Desktop khong tham chieu BookStore.Services va khong noi truc tiep
// vao CSDL. No chi trao doi voi BookStore.Api qua HTTP nen tu khai bao lai
// cac lop du lieu tuong ung voi JSON ma API tra ve.

public class NguoiDung
{
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public int? MaQuanLy { get; set; }

    public bool LaQuanLy => VaiTro == "QUẢN LÝ";
}

public class KetQuaDangNhap
{
    public string Token { get; set; } = string.Empty;
    public DateTime HetHan { get; set; }
    public NguoiDung NguoiDung { get; set; } = new();
}

public class Sach
{
    public int MaSach { get; set; }
    public string? ISBN { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int MaLoaiSach { get; set; }
    public string TenLoaiSach { get; set; } = string.Empty;
    public int? MaNXB { get; set; }
    public string? TenNXB { get; set; }
    public string TenTacGia { get; set; } = string.Empty;
    public List<int> MaTacGias { get; set; } = new();
    public int? NamXuatBan { get; set; }
    public int? SoTrang { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public int NguongCanhBao { get; set; } = 5;
    public string? MoTa { get; set; }
    public string? AnhBia { get; set; }
    public byte TrangThai { get; set; }

    /// <summary>Anh bia lay qua Web API (cung thu muc anh voi trang Web).</summary>
    public Microsoft.UI.Xaml.Media.ImageSource? AnhHienThi => BookStore.Admin.Helpers.TienIch.TaoAnh(AnhBia);

    public string GiaBanHienThi => GiaBan.ToString("#,##0") + " đ";
    public string TrangThaiHienThi => TrangThai == 1 ? "Đang kinh doanh" : "Ngừng kinh doanh";
    public bool SapHetHang => TrangThai == 1 && SoLuongTon <= NguongCanhBao;
    public override string ToString() => $"{TenSach} (tồn {SoLuongTon})";
}

public class LoaiSach
{
    public int MaLoaiSach { get; set; }
    public string TenLoaiSach { get; set; } = string.Empty;
    public int SoSach { get; set; }
    public override string ToString() => TenLoaiSach;
}

public class DonHang
{
    public int MaDonHang { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
    public DateTime NgayDat { get; set; }
    public byte TrangThai { get; set; }
    public string TenTrangThai { get; set; } = string.Empty;
    public string HoTenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string DiaChiGiao { get; set; } = string.Empty;
    public decimal TamTinh { get; set; }
    public decimal PhiVanChuyen { get; set; }
    public decimal TongTien { get; set; }
    public string TenHinhThucThanhToan { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public string? TenQuanLyXuLy { get; set; }
    public List<DongDonHang> Dong { get; set; } = new();
    public List<LichSuDonHang> LichSu { get; set; } = new();

    public string NgayDatHienThi => NgayDat.ToString("dd/MM/yyyy HH:mm");
    public string TongTienHienThi => TongTien.ToString("#,##0") + " đ";
}

public class DongDonHang
{
    public int MaSach { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;

    public string DonGiaHienThi => DonGia.ToString("#,##0") + " đ";
    public string ThanhTienHienThi => ThanhTien.ToString("#,##0") + " đ";
}

public class LichSuDonHang
{
    public DateTime ThoiDiem { get; set; }
    public string? TrangThaiCu { get; set; }
    public string TrangThaiMoi { get; set; } = string.Empty;
    public string? NguoiThucHien { get; set; }
    public string? GhiChu { get; set; }

    public string MoTa => $"{ThoiDiem:dd/MM/yyyy HH:mm} — {TrangThaiMoi}" +
                          (string.IsNullOrWhiteSpace(NguoiThucHien) ? "" : $" ({NguoiThucHien})");
}

public class ThongKe
{
    public int SoDonChoXacNhan { get; set; }
    public int SoDonDangGiao { get; set; }
    public int SoDonHoanThanhThangNay { get; set; }
    public decimal DoanhThuThangNay { get; set; }
    public int SoSachSapHetHang { get; set; }
    public List<SachBanChay> TopSachBanChay { get; set; } = new();

    public string DoanhThuHienThi => DoanhThuThangNay.ToString("#,##0") + " đ";
}

public class SachBanChay
{
    public string TenSach { get; set; } = string.Empty;
    public int SoLuongBan { get; set; }
    public decimal DoanhThu { get; set; }
    public string DoanhThuHienThi => DoanhThu.ToString("#,##0") + " đ";
}

public class TrangKetQua<T>
{
    public List<T> DanhSach { get; set; } = new();
    public int TongSoDong { get; set; }
    public int Trang { get; set; }
    public int SoDongMoiTrang { get; set; }
    public int TongSoTrang { get; set; }
}

public class PhanHoi
{
    [JsonPropertyName("thongBao")]
    public string ThongBao { get; set; } = string.Empty;
}
