using BookStore.Domain.Enums;

namespace BookStore.Domain.Entities;

public class TaiKhoan
{
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhauHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public int MaVaiTro { get; set; }
    public TrangThaiTaiKhoan TrangThai { get; set; } = TrangThaiTaiKhoan.HoatDong;
    public DateTime NgayTao { get; set; } = DateTime.Now;

    // Bo sung 2026-10-30: phuc vu khoa tai khoan va dat lai mat khau
    public int SoLanDangNhapSai { get; set; }
    public DateTime? ThoiDiemHetKhoa { get; set; }
    public string? TokenDatLaiMatKhau { get; set; }
    public DateTime? HanToken { get; set; }

    public VaiTro? VaiTro { get; set; }
    public KhachHang? KhachHang { get; set; }
    public QuanLy? QuanLy { get; set; }
}
