using BookStore.Domain.Enums;

namespace BookStore.Services.Dtos;

/// <summary>Thong tin sach dung cho danh sach va chi tiet.</summary>
public class SachDto
{
    public int MaSach { get; set; }
    public string? ISBN { get; set; }
    public string TenSach { get; set; } = string.Empty;
    public int MaLoaiSach { get; set; }
    public string TenLoaiSach { get; set; } = string.Empty;
    public int? MaNXB { get; set; }
    public string? TenNXB { get; set; }
    public string TenTacGia { get; set; } = string.Empty;
    public List<int> MaTacGias { get; set; } = new();
    public int? NamXuatBan { get; set; }
    public int? SoTrang { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public int NguongCanhBao { get; set; }
    public string? MoTa { get; set; }
    public string? AnhBia { get; set; }
    public TrangThaiSach TrangThai { get; set; }

    public bool ConHang => SoLuongTon > 0 && TrangThai == TrangThaiSach.DangKinhDoanh;
}

/// <summary>Du lieu tao moi hoac cap nhat mot quyen sach.</summary>
public class SachLuuDto
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
    public int NguongCanhBao { get; set; } = 5;
    public string? MoTa { get; set; }
    public string? AnhBia { get; set; }
    public TrangThaiSach TrangThai { get; set; } = TrangThaiSach.DangKinhDoanh;

    /// <summary>Danh sach ma tac gia gan voi sach (quan he n-n qua bang Sach_TacGia).</summary>
    public List<int>? MaTacGias { get; set; }
}

/// <summary>Tieu chi tim kiem va loc sach o trang Web va ung dung quan tri.</summary>
public class TimSachDto
{
    public string? TuKhoa { get; set; }
    public int? MaLoaiSach { get; set; }
    public decimal? GiaTu { get; set; }
    public decimal? GiaDen { get; set; }
    public string? SapXep { get; set; }
    public int Trang { get; set; } = 1;
    public int SoDongMoiTrang { get; set; } = 12;
    public bool ChiSachDangBan { get; set; } = true;
}

/// <summary>Mot trang ket qua, kem tong so dong de ve thanh phan trang.</summary>
public class TrangKetQua<T>
{
    public List<T> DanhSach { get; set; } = new();
    public int TongSoDong { get; set; }
    public int Trang { get; set; } = 1;
    public int SoDongMoiTrang { get; set; } = 12;
    public int TongSoTrang => SoDongMoiTrang <= 0 ? 1 : (int)Math.Ceiling((double)TongSoDong / SoDongMoiTrang);
}
