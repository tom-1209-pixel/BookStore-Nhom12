namespace BookStore.Domain.Entities;

public class GioHang
{
    public int MaGioHang { get; set; }
    public int MaKhachHang { get; set; }
    public DateTime NgayCapNhat { get; set; } = DateTime.Now;

    public KhachHang? KhachHang { get; set; }
    public ICollection<CTGioHang> CTGioHangs { get; set; } = new List<CTGioHang>();
}
