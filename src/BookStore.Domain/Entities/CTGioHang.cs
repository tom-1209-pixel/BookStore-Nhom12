namespace BookStore.Domain.Entities;

public class CTGioHang
{
    public int MaGioHang { get; set; }
    public int MaSach { get; set; }
    public int SoLuong { get; set; }
    public DateTime NgayThem { get; set; } = DateTime.Now;

    public GioHang? GioHang { get; set; }
    public Sach? Sach { get; set; }
}
