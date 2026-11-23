using BookStore.Domain.Enums;

namespace BookStore.Domain.Entities;

public class PhieuNhap
{
    public int MaPhieuNhap { get; set; }
    public int MaNCC { get; set; }
    public int MaQuanLy { get; set; }
    public DateTime NgayLap { get; set; } = DateTime.Now;

    /// <summary>Thoi diem xac nhan Hoan thanh; chi luc nay ton kho moi duoc cong (BR08).</summary>
    public DateTime? NgayHoanThanh { get; set; }
    public decimal TongTien { get; set; }
    public TrangThaiPhieuNhap TrangThai { get; set; } = TrangThaiPhieuNhap.Nhap;
    public string? GhiChu { get; set; }

    public NhaCungCap? NhaCungCap { get; set; }
    public QuanLy? QuanLy { get; set; }
    public ICollection<CTPhieuNhap> CTPhieuNhaps { get; set; } = new List<CTPhieuNhap>();
}
