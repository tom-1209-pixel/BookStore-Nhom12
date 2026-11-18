namespace BookStore.Domain.Entities;

public class NhaXuatBan
{
    public int MaNXB { get; set; }
    public string TenNXB { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }

    public ICollection<Sach> Sachs { get; set; } = new List<Sach>();
}
