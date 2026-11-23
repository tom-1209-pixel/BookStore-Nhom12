namespace BookStore.Services.Dtos;

public class DangNhapDto
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
}

public class DangKyDto
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string XacNhanMatKhau { get; set; } = string.Empty;
}

/// <summary>Thong tin nguoi dung sau khi xac thuc thanh cong.</summary>
public class NguoiDungDto
{
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;

    /// <summary>Chi co gia tri khi tai khoan thuoc vai tro KHACH HANG.</summary>
    public int? MaKhachHang { get; set; }

    /// <summary>Chi co gia tri khi tai khoan thuoc vai tro NHAN VIEN hoac QUAN LY.</summary>
    public int? MaQuanLy { get; set; }
}

/// <summary>Ket qua dang nhap tu Web API, kem token JWT cho ung dung Desktop.</summary>
public class KetQuaDangNhapApiDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime HetHan { get; set; }
    public NguoiDungDto NguoiDung { get; set; } = new();
}

public class DoiMatKhauDto
{
    public string MatKhauCu { get; set; } = string.Empty;
    public string MatKhauMoi { get; set; } = string.Empty;
    public string XacNhanMatKhauMoi { get; set; } = string.Empty;
}

public class LoaiSachDto
{
    public int MaLoaiSach { get; set; }
    public string TenLoaiSach { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool TrangThai { get; set; } = true;
    public int SoSach { get; set; }
}

public class DiaChiGiaoHangDto
{
    public int MaDiaChi { get; set; }
    public string HoTenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string DiaChiChiTiet { get; set; } = string.Empty;
    public string PhuongXa { get; set; } = string.Empty;
    public string QuanHuyen { get; set; } = string.Empty;
    public string TinhThanh { get; set; } = string.Empty;
    public string DiaChiDayDu { get; set; } = string.Empty;
    public bool LaMacDinh { get; set; }
}

/// <summary>Thong tin ca nhan cua khach hang (man hinh W07 - Tai khoan cua toi).</summary>
public class HoSoKhachHangDto
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public DateTime? NgaySinh { get; set; }

    /// <summary>0: nu, 1: nam, null: khong cho biet.</summary>
    public byte? GioiTinh { get; set; }
    public DateTime NgayDangKy { get; set; }
}
