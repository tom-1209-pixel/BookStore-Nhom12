namespace BookStore.Domain.Entities;

public class TacGia
{
    public int MaTacGia { get; set; }
    public string TenTacGia { get; set; } = string.Empty;
    public string? TieuSu { get; set; }

    public ICollection<Sach_TacGia> Sach_TacGias { get; set; } = new List<Sach_TacGia>();
}
