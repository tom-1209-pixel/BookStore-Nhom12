namespace BookStore.Domain.Entities;

public class CTDonHang
{
    public int MaDonHang { get; set; }
    public int MaSach { get; set; }

    /// <summary>Ten sach sao luu tai thoi diem dat, phong khi sach bi doi ten.</summary>
    public string TenSach { get; set; } = string.Empty;
    public int SoLuong { get; set; }

    /// <summary>Gia ban tai thoi diem dat hang (BR04).</summary>
    public decimal DonGia { get; set; }

    public DonHang? DonHang { get; set; }
    public Sach? Sach { get; set; }

    public decimal ThanhTien => SoLuong * DonGia;
}
