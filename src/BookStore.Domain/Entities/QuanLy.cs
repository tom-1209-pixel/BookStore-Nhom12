namespace BookStore.Domain.Entities;

public class QuanLy
{
    public int MaQuanLy { get; set; }
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public DateTime NgayVaoLam { get; set; } = DateTime.Now;
    public string? ChucVu { get; set; }

    public TaiKhoan? TaiKhoan { get; set; }
    public ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
    public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
