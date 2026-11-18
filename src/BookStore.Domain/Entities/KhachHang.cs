namespace BookStore.Domain.Entities;

public class KhachHang
{
    public int MaKhachHang { get; set; }
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    public byte? GioiTinh { get; set; }
    public DateTime NgayDangKy { get; set; } = DateTime.Now;

    public TaiKhoan? TaiKhoan { get; set; }
    public ICollection<DiaChiGiaoHang> DiaChiGiaoHangs { get; set; } = new List<DiaChiGiaoHang>();
    public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    public GioHang? GioHang { get; set; }
}
