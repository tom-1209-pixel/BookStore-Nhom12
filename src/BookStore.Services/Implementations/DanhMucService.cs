using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Services.Common;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services.Implementations;

/// <summary>
/// Quan ly bon danh muc: loai sach, tac gia, nha xuat ban, nha cung cap (D04).
/// Muc danh muc dang duoc sach hoac phieu nhap tham chieu thi khong duoc xoa,
/// tranh lam mat toan ven du lieu lich su.
/// </summary>
public class DanhMucService : IDanhMucService
{
    private readonly BookStoreDbContext _db;
    private readonly ILogger<DanhMucService> _log;

    public DanhMucService(BookStoreDbContext db, ILogger<DanhMucService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<List<DanhMucDto>> LayDanhSachAsync(LoaiDanhMuc loai, string? tuKhoa = null)
    {
        var tk = tuKhoa?.Trim();
        var coTuKhoa = !string.IsNullOrEmpty(tk);

        return loai switch
        {
            LoaiDanhMuc.LoaiSach => await _db.LoaiSach.AsNoTracking()
                .Where(x => !coTuKhoa || EF.Functions.Like(x.TenLoaiSach, $"%{tk}%"))
                .OrderBy(x => x.TenLoaiSach)
                .Select(x => new DanhMucDto
                {
                    Ma = x.MaLoaiSach, Ten = x.TenLoaiSach, MoTa = x.MoTa, SoLienKet = x.Sachs.Count
                }).ToListAsync(),

            LoaiDanhMuc.TacGia => await _db.TacGia.AsNoTracking()
                .Where(x => !coTuKhoa || EF.Functions.Like(x.TenTacGia, $"%{tk}%"))
                .OrderBy(x => x.TenTacGia)
                .Select(x => new DanhMucDto
                {
                    Ma = x.MaTacGia, Ten = x.TenTacGia, MoTa = x.TieuSu, SoLienKet = x.Sach_TacGias.Count
                }).ToListAsync(),

            LoaiDanhMuc.NhaXuatBan => await _db.NhaXuatBan.AsNoTracking()
                .Where(x => !coTuKhoa || EF.Functions.Like(x.TenNXB, $"%{tk}%"))
                .OrderBy(x => x.TenNXB)
                .Select(x => new DanhMucDto
                {
                    Ma = x.MaNXB, Ten = x.TenNXB, DiaChi = x.DiaChi, SoDienThoai = x.SoDienThoai,
                    SoLienKet = x.Sachs.Count
                }).ToListAsync(),

            LoaiDanhMuc.NhaCungCap => await _db.NhaCungCap.AsNoTracking()
                .Where(x => !coTuKhoa || EF.Functions.Like(x.TenNCC, $"%{tk}%")
                                      || (x.SoDienThoai != null && EF.Functions.Like(x.SoDienThoai, $"%{tk}%")))
                .OrderBy(x => x.TenNCC)
                .Select(x => new DanhMucDto
                {
                    Ma = x.MaNCC, Ten = x.TenNCC, DiaChi = x.DiaChi, SoDienThoai = x.SoDienThoai,
                    Email = x.Email, SoLienKet = x.PhieuNhaps.Count
                }).ToListAsync(),

            _ => new List<DanhMucDto>()
        };
    }

    public async Task<KetQua<int>> LuuAsync(LoaiDanhMuc loai, DanhMucDto dto)
    {
        var ten = dto.Ten?.Trim() ?? string.Empty;
        if (ten.Length == 0) return KetQua<int>.Loi("Vui lòng nhập tên.");
        if (ten.Length > 150) return KetQua<int>.Loi("Tên không được vượt quá 150 ký tự.");
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains('@'))
            return KetQua<int>.Loi("Email không hợp lệ.");

        var laThem = dto.Ma <= 0;
        int ma;

        switch (loai)
        {
            case LoaiDanhMuc.LoaiSach:
            {
                if (await _db.LoaiSach.AnyAsync(x => x.TenLoaiSach == ten && x.MaLoaiSach != dto.Ma))
                    return KetQua<int>.Loi("Tên loại sách đã tồn tại.");
                var e = laThem ? new LoaiSach() : await _db.LoaiSach.FindAsync(dto.Ma);
                if (e is null) return KetQua<int>.Loi("Không tìm thấy loại sách.");
                e.TenLoaiSach = ten;
                e.MoTa = Rong(dto.MoTa);
                if (laThem) _db.LoaiSach.Add(e);
                await _db.SaveChangesAsync();
                ma = e.MaLoaiSach;
                break;
            }
            case LoaiDanhMuc.TacGia:
            {
                var e = laThem ? new TacGia() : await _db.TacGia.FindAsync(dto.Ma);
                if (e is null) return KetQua<int>.Loi("Không tìm thấy tác giả.");
                e.TenTacGia = ten;
                e.TieuSu = Rong(dto.MoTa);
                if (laThem) _db.TacGia.Add(e);
                await _db.SaveChangesAsync();
                ma = e.MaTacGia;
                break;
            }
            case LoaiDanhMuc.NhaXuatBan:
            {
                if (await _db.NhaXuatBan.AnyAsync(x => x.TenNXB == ten && x.MaNXB != dto.Ma))
                    return KetQua<int>.Loi("Tên nhà xuất bản đã tồn tại.");
                var e = laThem ? new NhaXuatBan() : await _db.NhaXuatBan.FindAsync(dto.Ma);
                if (e is null) return KetQua<int>.Loi("Không tìm thấy nhà xuất bản.");
                e.TenNXB = ten;
                e.DiaChi = Rong(dto.DiaChi);
                e.SoDienThoai = Rong(dto.SoDienThoai);
                if (laThem) _db.NhaXuatBan.Add(e);
                await _db.SaveChangesAsync();
                ma = e.MaNXB;
                break;
            }
            case LoaiDanhMuc.NhaCungCap:
            {
                if (await _db.NhaCungCap.AnyAsync(x => x.TenNCC == ten && x.MaNCC != dto.Ma))
                    return KetQua<int>.Loi("Tên nhà cung cấp đã tồn tại.");
                var e = laThem ? new NhaCungCap() : await _db.NhaCungCap.FindAsync(dto.Ma);
                if (e is null) return KetQua<int>.Loi("Không tìm thấy nhà cung cấp.");
                e.TenNCC = ten;
                e.DiaChi = Rong(dto.DiaChi);
                e.SoDienThoai = Rong(dto.SoDienThoai);
                e.Email = Rong(dto.Email);
                if (laThem) _db.NhaCungCap.Add(e);
                await _db.SaveChangesAsync();
                ma = e.MaNCC;
                break;
            }
            default:
                return KetQua<int>.Loi("Loại danh mục không hợp lệ.");
        }

        _log.LogInformation("{HanhDong} {Loai} {Ma} - {Ten}.", laThem ? "Đã thêm" : "Đã cập nhật", loai, ma, ten);
        return KetQua<int>.Dung(ma, laThem ? $"Đã thêm {TenLoai(loai)}." : $"Đã cập nhật {TenLoai(loai)}.");
    }

    public async Task<KetQua> XoaAsync(LoaiDanhMuc loai, int ma)
    {
        switch (loai)
        {
            case LoaiDanhMuc.LoaiSach:
            {
                var e = await _db.LoaiSach.FindAsync(ma);
                if (e is null) return KetQua.Loi("Không tìm thấy loại sách.");
                if (await _db.Sach.AnyAsync(s => s.MaLoaiSach == ma))
                    return KetQua.Loi("Loại sách đang có sách nên không thể xóa.");
                _db.LoaiSach.Remove(e);
                break;
            }
            case LoaiDanhMuc.TacGia:
            {
                var e = await _db.TacGia.FindAsync(ma);
                if (e is null) return KetQua.Loi("Không tìm thấy tác giả.");
                if (await _db.Sach_TacGia.AnyAsync(s => s.MaTacGia == ma))
                    return KetQua.Loi("Tác giả đang được gán cho sách nên không thể xóa.");
                _db.TacGia.Remove(e);
                break;
            }
            case LoaiDanhMuc.NhaXuatBan:
            {
                var e = await _db.NhaXuatBan.FindAsync(ma);
                if (e is null) return KetQua.Loi("Không tìm thấy nhà xuất bản.");
                if (await _db.Sach.AnyAsync(s => s.MaNXB == ma))
                    return KetQua.Loi("Nhà xuất bản đang có sách nên không thể xóa.");
                _db.NhaXuatBan.Remove(e);
                break;
            }
            case LoaiDanhMuc.NhaCungCap:
            {
                var e = await _db.NhaCungCap.FindAsync(ma);
                if (e is null) return KetQua.Loi("Không tìm thấy nhà cung cấp.");
                if (await _db.PhieuNhap.AnyAsync(p => p.MaNCC == ma))
                    return KetQua.Loi("Nhà cung cấp đã có phiếu nhập nên không thể xóa.");
                _db.NhaCungCap.Remove(e);
                break;
            }
            default:
                return KetQua.Loi("Loại danh mục không hợp lệ.");
        }

        await _db.SaveChangesAsync();
        _log.LogInformation("Đã xóa {Loai} {Ma}.", loai, ma);
        return KetQua.Dung($"Đã xóa {TenLoai(loai)}.");
    }

    public static string TenLoai(LoaiDanhMuc loai) => loai switch
    {
        LoaiDanhMuc.LoaiSach => "loại sách",
        LoaiDanhMuc.TacGia => "tác giả",
        LoaiDanhMuc.NhaXuatBan => "nhà xuất bản",
        LoaiDanhMuc.NhaCungCap => "nhà cung cấp",
        _ => "danh mục"
    };

    private static string? Rong(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
