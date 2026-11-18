namespace BookStore.Domain.Entities;

/// <summary>Bang trung gian cho quan he n-n giua Sach va TacGia.</summary>
public class Sach_TacGia
{
    public int MaSach { get; set; }
    public int MaTacGia { get; set; }
    public string? VaiTroTacGia { get; set; }

    public Sach? Sach { get; set; }
    public TacGia? TacGia { get; set; }
}
