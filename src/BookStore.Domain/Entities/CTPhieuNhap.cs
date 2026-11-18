namespace BookStore.Domain.Entities;

public class CTPhieuNhap
{
    public int MaPhieuNhap { get; set; }
    public int MaSach { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGiaNhap { get; set; }

    public PhieuNhap? PhieuNhap { get; set; }
    public Sach? Sach { get; set; }

    public decimal ThanhTien => SoLuong * DonGiaNhap;
}
