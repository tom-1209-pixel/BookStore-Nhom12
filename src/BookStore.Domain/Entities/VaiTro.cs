namespace BookStore.Domain.Entities;

public class VaiTro
{
    public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;
    public string? MoTa { get; set; }

    public ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
