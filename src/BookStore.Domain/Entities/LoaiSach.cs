namespace BookStore.Domain.Entities;

public class LoaiSach
{
    public int MaLoaiSach { get; set; }
    public string TenLoaiSach { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool TrangThai { get; set; } = true;

    public ICollection<Sach> Sachs { get; set; } = new List<Sach>();
}
