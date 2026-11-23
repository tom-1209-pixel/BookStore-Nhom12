using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services.Implementations;

/// <summary>
/// Quan ly kho va nhap hang (D06). Noi cai dat quy tac BR08: phieu nhap chi cong
/// vao ton kho khi duoc xac nhan Hoan thanh, sau do khong duoc sua hay xoa.
/// </summary>
public class KhoService : IKhoService
{
    private readonly BookStoreDbContext _db;
    private readonly ILogger<KhoService> _log;

    public KhoService(BookStoreDbContext db, ILogger<KhoService> log)
    {
        _db = db;
        _log = log;
    }

    // ------------------------------------------------------------ Ton kho

    public async Task<List<TonKhoDto>> LayTonKhoAsync(string? tuKhoa = null, bool chiSapHetHang = false)
    {
        var q = _db.Sach.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            q = q.Where(s => EF.Functions.Like(s.TenSach, $"%{tk}%")
                          || (s.ISBN != null && EF.Functions.Like(s.ISBN, $"%{tk}%")));
        }

        if (chiSapHetHang)
            q = q.Where(s => s.TrangThai == TrangThaiSach.DangKinhDoanh && s.SoLuongTon <= s.NguongCanhBao);

        return await q
            .OrderBy(s => s.SoLuongTon - s.NguongCanhBao)
            .ThenBy(s => s.TenSach)
            .Select(s => new TonKhoDto
            {
                MaSach = s.MaSach,
                TenSach = s.TenSach,
                TenLoaiSach = s.LoaiSach!.TenLoaiSach,
                SoLuongTon = s.SoLuongTon,
                NguongCanhBao = s.NguongCanhBao,
                TrangThai = s.TrangThai
            })
            .ToListAsync();
    }

    public async Task<KetQua> CapNhatNguongAsync(int maSach, int nguong)
    {
        if (nguong < 0) return KetQua.Loi("Ngưỡng cảnh báo không được là số âm.");

        var s = await _db.Sach.FindAsync(maSach);
        if (s is null) return KetQua.Loi("Không tìm thấy sách.");

        s.NguongCanhBao = nguong;
        await _db.SaveChangesAsync();
        _log.LogInformation("Đặt ngưỡng cảnh báo của sách {MaSach} là {Nguong}.", maSach, nguong);
        return KetQua.Dung($"Đã đặt ngưỡng cảnh báo của \"{s.TenSach}\" là {nguong}.");
    }

    // ------------------------------------------------------------ Phieu nhap

    public async Task<List<PhieuNhapDto>> TimPhieuNhapAsync(TrangThaiPhieuNhap? trangThai = null, string? tuKhoa = null)
    {
        var q = _db.PhieuNhap
            .Include(p => p.NhaCungCap)
            .Include(p => p.QuanLy)
            .Include(p => p.CTPhieuNhaps)
            .AsNoTracking()
            .AsQueryable();

        if (trangThai.HasValue) q = q.Where(p => p.TrangThai == trangThai.Value);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            q = q.Where(p => EF.Functions.Like(p.NhaCungCap!.TenNCC, $"%{tk}%") || p.MaPhieuNhap.ToString() == tk);
        }

        var ds = await q.OrderByDescending(p => p.MaPhieuNhap).ToListAsync();
        return ds.Select(p => ChuyenDoi(p, kemChiTiet: false)).ToList();
    }

    public async Task<PhieuNhapDto?> LayPhieuNhapAsync(int maPhieuNhap)
    {
        var p = await _db.PhieuNhap
            .Include(x => x.NhaCungCap)
            .Include(x => x.QuanLy)
            .Include(x => x.CTPhieuNhaps).ThenInclude(ct => ct.Sach)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaPhieuNhap == maPhieuNhap);
        return p is null ? null : ChuyenDoi(p, kemChiTiet: true);
    }

    public async Task<KetQua<int>> TaoPhieuNhapAsync(int maQuanLy, PhieuNhapLuuDto dto)
    {
        var loi = await KiemTraAsync(dto);
        if (loi is not null) return KetQua<int>.Loi(loi);

        if (!await _db.QuanLy.AnyAsync(q => q.MaQuanLy == maQuanLy))
            return KetQua<int>.Loi("Không xác định được quản lý lập phiếu.");

        var p = new PhieuNhap
        {
            MaNCC = dto.MaNCC,
            MaQuanLy = maQuanLy,
            NgayLap = DateTime.Now,
            GhiChu = string.IsNullOrWhiteSpace(dto.GhiChu) ? null : dto.GhiChu.Trim(),
            TrangThai = TrangThaiPhieuNhap.Nhap
        };
        GanDong(p, dto);

        _db.PhieuNhap.Add(p);
        await _db.SaveChangesAsync();
        _log.LogInformation("Đã lập phiếu nhập {MaPhieuNhap}, tổng tiền {TongTien}.", p.MaPhieuNhap, p.TongTien);
        return KetQua<int>.Dung(p.MaPhieuNhap, $"Đã lập phiếu nhập #{p.MaPhieuNhap} ở trạng thái Chờ nhập.");
    }

    public async Task<KetQua> CapNhatPhieuNhapAsync(int maPhieuNhap, PhieuNhapLuuDto dto)
    {
        var p = await _db.PhieuNhap.Include(x => x.CTPhieuNhaps)
            .FirstOrDefaultAsync(x => x.MaPhieuNhap == maPhieuNhap);
        if (p is null) return KetQua.Loi("Không tìm thấy phiếu nhập.");
        if (p.TrangThai != TrangThaiPhieuNhap.Nhap)
            return KetQua.Loi("Phiếu nhập đã hoàn thành nên không thể sửa.", "BR08");

        var loi = await KiemTraAsync(dto);
        if (loi is not null) return KetQua.Loi(loi);

        p.MaNCC = dto.MaNCC;
        p.GhiChu = string.IsNullOrWhiteSpace(dto.GhiChu) ? null : dto.GhiChu.Trim();
        _db.CTPhieuNhap.RemoveRange(p.CTPhieuNhaps);
        p.CTPhieuNhaps.Clear();
        GanDong(p, dto);

        await _db.SaveChangesAsync();
        _log.LogInformation("Đã sửa phiếu nhập {MaPhieuNhap}.", maPhieuNhap);
        return KetQua.Dung($"Đã cập nhật phiếu nhập #{maPhieuNhap}.");
    }

    /// <summary>
    /// BR08: cong so luong cua tung dong vao ton kho trong cung mot transaction voi
    /// viec doi trang thai phieu, de ton kho khong bao gio lech voi phieu da hoan thanh.
    /// </summary>
    public async Task<KetQua> HoanThanhPhieuNhapAsync(int maPhieuNhap)
    {
        var p = await _db.PhieuNhap.Include(x => x.CTPhieuNhaps)
            .FirstOrDefaultAsync(x => x.MaPhieuNhap == maPhieuNhap);
        if (p is null) return KetQua.Loi("Không tìm thấy phiếu nhập.");
        if (p.TrangThai == TrangThaiPhieuNhap.HoanThanh)
            return KetQua.Loi("Phiếu nhập đã được hoàn thành trước đó.", "BR08");
        if (p.CTPhieuNhaps.Count == 0)
            return KetQua.Loi("Phiếu nhập chưa có sách nào.");

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await _db.Database.BeginTransactionAsync();
            try
            {
                var maSachs = p.CTPhieuNhaps.Select(ct => ct.MaSach).ToList();
                var sachs = await _db.Sach.Where(s => maSachs.Contains(s.MaSach)).ToListAsync();

                foreach (var ct in p.CTPhieuNhaps)
                {
                    var s = sachs.First(x => x.MaSach == ct.MaSach);
                    s.SoLuongTon += ct.SoLuong;
                }

                p.TrangThai = TrangThaiPhieuNhap.HoanThanh;
                p.NgayHoanThanh = DateTime.Now;

                await _db.SaveChangesAsync();
                await tran.CommitAsync();

                _log.LogInformation("Phiếu nhập {MaPhieuNhap} hoàn thành, cộng {SoLuong} quyển vào tồn kho (BR08).",
                    maPhieuNhap, p.CTPhieuNhaps.Sum(x => x.SoLuong));
                return KetQua.Dung($"Phiếu nhập #{maPhieuNhap} đã hoàn thành, tồn kho đã được cộng thêm.");
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                _log.LogError(ex, "Lỗi khi hoàn thành phiếu nhập {MaPhieuNhap}.", maPhieuNhap);
                return KetQua.Loi("Không thể hoàn thành phiếu nhập do lỗi hệ thống.");
            }
        });
    }

    public async Task<KetQua> XoaPhieuNhapAsync(int maPhieuNhap)
    {
        var p = await _db.PhieuNhap.FindAsync(maPhieuNhap);
        if (p is null) return KetQua.Loi("Không tìm thấy phiếu nhập.");
        if (p.TrangThai == TrangThaiPhieuNhap.HoanThanh)
            return KetQua.Loi("Phiếu nhập đã hoàn thành nên không thể xóa.", "BR08");

        _db.PhieuNhap.Remove(p);   // chi tiet phieu xoa theo (cascade)
        await _db.SaveChangesAsync();
        _log.LogInformation("Đã xóa phiếu nhập {MaPhieuNhap}.", maPhieuNhap);
        return KetQua.Dung($"Đã xóa phiếu nhập #{maPhieuNhap}.");
    }

    // ------------------------------------------------------------ Ham phu

    private async Task<string?> KiemTraAsync(PhieuNhapLuuDto dto)
    {
        if (dto.MaNCC <= 0) return "Vui lòng chọn nhà cung cấp.";
        if (!await _db.NhaCungCap.AnyAsync(n => n.MaNCC == dto.MaNCC)) return "Nhà cung cấp không tồn tại.";
        if (dto.Dong is null || dto.Dong.Count == 0) return "Phiếu nhập phải có ít nhất một sách.";
        if (dto.Dong.Any(d => d.SoLuong <= 0)) return "Số lượng nhập phải lớn hơn 0.";
        if (dto.Dong.Any(d => d.DonGiaNhap < 0)) return "Giá nhập không được là số âm.";
        if (dto.Dong.GroupBy(d => d.MaSach).Any(g => g.Count() > 1))
            return "Một sách chỉ được xuất hiện một lần trong phiếu nhập.";

        var maSachs = dto.Dong.Select(d => d.MaSach).ToList();
        var soTonTai = await _db.Sach.CountAsync(s => maSachs.Contains(s.MaSach));
        if (soTonTai != maSachs.Count) return "Phiếu nhập có sách không tồn tại.";
        return null;
    }

    private static void GanDong(PhieuNhap p, PhieuNhapLuuDto dto)
    {
        foreach (var d in dto.Dong)
            p.CTPhieuNhaps.Add(new CTPhieuNhap { MaSach = d.MaSach, SoLuong = d.SoLuong, DonGiaNhap = d.DonGiaNhap });
        p.TongTien = dto.Dong.Sum(d => d.SoLuong * d.DonGiaNhap);
    }

    private static PhieuNhapDto ChuyenDoi(PhieuNhap p, bool kemChiTiet) => new()
    {
        MaPhieuNhap = p.MaPhieuNhap,
        MaNCC = p.MaNCC,
        TenNCC = p.NhaCungCap?.TenNCC ?? string.Empty,
        TenQuanLy = p.QuanLy?.HoTen,
        NgayLap = p.NgayLap,
        NgayHoanThanh = p.NgayHoanThanh,
        TongTien = p.TongTien,
        TrangThai = p.TrangThai,
        GhiChu = p.GhiChu,
        SoDauSach = p.CTPhieuNhaps.Count,
        Dong = !kemChiTiet ? new() : p.CTPhieuNhaps.Select(ct => new DongPhieuNhapDto
        {
            MaSach = ct.MaSach,
            TenSach = ct.Sach?.TenSach ?? string.Empty,
            SoLuong = ct.SoLuong,
            DonGiaNhap = ct.DonGiaNhap
        }).ToList()
    };
}
