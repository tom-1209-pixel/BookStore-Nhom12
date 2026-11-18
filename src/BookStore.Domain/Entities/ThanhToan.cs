using BookStore.Domain.Enums;

namespace BookStore.Domain.Entities;

public class ThanhToan
{
    public int MaThanhToan { get; set; }
    public int MaDonHang { get; set; }
    public HinhThucThanhToan PhuongThuc { get; set; }
    public decimal SoTien { get; set; }
    public TrangThaiThanhToan TrangThai { get; set; } = TrangThaiThanhToan.Cho;
    public string? MaGiaoDich { get; set; }
    public DateTime ThoiDiem { get; set; } = DateTime.Now;
    public string? NoiDung { get; set; }

    public DonHang? DonHang { get; set; }
}
