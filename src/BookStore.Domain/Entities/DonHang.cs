using BookStore.Domain.Enums;

namespace BookStore.Domain.Entities;

public class DonHang
{
    public int MaDonHang { get; set; }
    public int MaKhachHang { get; set; }
    public int? MaQuanLy { get; set; }
    public DateTime NgayDat { get; set; } = DateTime.Now;
    public TrangThaiDonHang TrangThai { get; set; } = TrangThaiDonHang.ChoXacNhan;

    // Thong tin nguoi nhan duoc sao luu tai thoi diem dat (du lieu lich su)
    public string HoTenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string DiaChiGiao { get; set; } = string.Empty;

    public decimal TamTinh { get; set; }
    public decimal PhiVanChuyen { get; set; }
    public decimal TongTien { get; set; }
    public HinhThucThanhToan HinhThucThanhToan { get; set; } = HinhThucThanhToan.COD;
    public string? GhiChu { get; set; }

    public KhachHang? KhachHang { get; set; }
    public QuanLy? QuanLy { get; set; }
    public ICollection<CTDonHang> CTDonHangs { get; set; } = new List<CTDonHang>();
    public ICollection<LichSuDonHang> LichSuDonHangs { get; set; } = new List<LichSuDonHang>();
    public ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
