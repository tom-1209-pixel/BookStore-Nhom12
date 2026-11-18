namespace BookStore.Domain.Entities;

public class NhaCungCap
{
    public int MaNCC { get; set; }
    public string TenNCC { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }

    public ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
}
