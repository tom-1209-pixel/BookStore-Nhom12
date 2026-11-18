using BookStore.Domain.Enums;

namespace BookStore.Domain.Entities;

public class Sach
{
    public int MaSach { get; set; }
    public string? ISBN { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int MaLoaiSach { get; set; }
    public int? MaNXB { get; set; }
    public int? NamXuatBan { get; set; }
    public int? SoTrang { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }

    /// <summary>Ton kho nho hon hoac bang nguong nay thi canh bao sap het hang.</summary>
    public int NguongCanhBao { get; set; } = 5;
    public string? MoTa { get; set; }
    public string? AnhBia { get; set; }
    public TrangThaiSach TrangThai { get; set; } = TrangThaiSach.DangKinhDoanh;
    public DateTime NgayTao { get; set; } = DateTime.Now;

    public LoaiSach? LoaiSach { get; set; }
    public NhaXuatBan? NhaXuatBan { get; set; }
    public ICollection<Sach_TacGia> Sach_TacGias { get; set; } = new List<Sach_TacGia>();
    public ICollection<CTDonHang> CTDonHangs { get; set; } = new List<CTDonHang>();
    public ICollection<CTPhieuNhap> CTPhieuNhaps { get; set; } = new List<CTPhieuNhap>();
}
