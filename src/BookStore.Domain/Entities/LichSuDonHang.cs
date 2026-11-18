using BookStore.Domain.Enums;

namespace BookStore.Domain.Entities;

public class LichSuDonHang
{
    public int MaLichSu { get; set; }
    public int MaDonHang { get; set; }
    public TrangThaiDonHang? TrangThaiCu { get; set; }
    public TrangThaiDonHang TrangThaiMoi { get; set; }
    public DateTime ThoiDiem { get; set; } = DateTime.Now;
    public string? NguoiThucHien { get; set; }
    public string? GhiChu { get; set; }

    public DonHang? DonHang { get; set; }
}
