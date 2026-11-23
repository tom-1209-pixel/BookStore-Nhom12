using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services.Implementations;

public class SachService : ISachService
{
    private readonly BookStoreDbContext _db;
    private readonly ILogger<SachService> _log;

    public SachService(BookStoreDbContext db, ILogger<SachService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<TrangKetQua<SachDto>> TimAsync(TimSachDto dto)
    {
        var q = _db.Sach
            .Include(s => s.LoaiSach)
            .Include(s => s.NhaXuatBan)
            .Include(s => s.Sach_TacGias).ThenInclude(st => st.TacGia)
            .AsNoTracking()
            .AsQueryable();

        if (dto.ChiSachDangBan)
            q = q.Where(s => s.TrangThai == TrangThaiSach.DangKinhDoanh);

        if (!string.IsNullOrWhiteSpace(dto.TuKhoa))
        {
            var tk = dto.TuKhoa.Trim();
            q = q.Where(s => EF.Functions.Like(s.TenSach, $"%{tk}%")
                          || (s.ISBN != null && EF.Functions.Like(s.ISBN, $"%{tk}%"))
                          || s.Sach_TacGias.Any(st => st.TacGia != null && EF.Functions.Like(st.TacGia.TenTacGia, $"%{tk}%")));
        }

        if (dto.MaLoaiSach.HasValue && dto.MaLoaiSach > 0)
            q = q.Where(s => s.MaLoaiSach == dto.MaLoaiSach);
        if (dto.GiaTu.HasValue)
            q = q.Where(s => s.GiaBan >= dto.GiaTu);
        if (dto.GiaDen.HasValue)
            q = q.Where(s => s.GiaBan <= dto.GiaDen);

        q = dto.SapXep switch
        {
            "gia-tang" => q.OrderBy(s => s.GiaBan),
            "gia-giam" => q.OrderByDescending(s => s.GiaBan),
            "ten" => q.OrderBy(s => s.TenSach),
            _ => q.OrderByDescending(s => s.MaSach)
        };

        var tong = await q.CountAsync();
        var trang = Math.Max(1, dto.Trang);
        var soDong = dto.SoDongMoiTrang <= 0 ? 12 : dto.SoDongMoiTrang;

        var ds = await q.Skip((trang - 1) * soDong).Take(soDong).ToListAsync();

        return new TrangKetQua<SachDto>
        {
            DanhSach = ds.Select(ChuyenDoi).ToList(),
            TongSoDong = tong,
            Trang = trang,
            SoDongMoiTrang = soDong
        };
    }

    public async Task<SachDto?> LayTheoMaAsync(int maSach)
    {
        var s = await _db.Sach
            .Include(x => x.LoaiSach)
            .Include(x => x.NhaXuatBan)
            .Include(x => x.Sach_TacGias).ThenInclude(st => st.TacGia)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaSach == maSach);
        return s is null ? null : ChuyenDoi(s);
    }

    public async Task<List<SachDto>> LaySachMoiNhatAsync(int soLuong = 8)
    {
        var ds = await _db.Sach
            .Include(x => x.LoaiSach)
            .Include(x => x.NhaXuatBan)
            .Include(x => x.Sach_TacGias).ThenInclude(st => st.TacGia)
            .Where(x => x.TrangThai == TrangThaiSach.DangKinhDoanh)
            .OrderByDescending(x => x.MaSach)
            .Take(soLuong)
            .AsNoTracking()
            .ToListAsync();
        return ds.Select(ChuyenDoi).ToList();
    }

    public async Task<KetQua<int>> ThemAsync(SachLuuDto dto)
    {
        var loi = KiemTra(dto);
        if (loi is not null) return KetQua<int>.Loi(loi);

        if (!await _db.LoaiSach.AnyAsync(l => l.MaLoaiSach == dto.MaLoaiSach))
            return KetQua<int>.Loi("Loại sách không tồn tại.");

        if (dto.MaNXB.HasValue && !await _db.NhaXuatBan.AnyAsync(n => n.MaNXB == dto.MaNXB))
            return KetQua<int>.Loi("Nhà xuất bản không tồn tại.");

        if (!string.IsNullOrWhiteSpace(dto.ISBN) &&
            await _db.Sach.AnyAsync(s => s.ISBN == dto.ISBN))
            return KetQua<int>.Loi("ISBN này đã có trong hệ thống.");

        var s = new Sach
        {
            ISBN = dto.ISBN?.Trim(),
            TenSach = dto.TenSach.Trim(),
            MaLoaiSach = dto.MaLoaiSach,
            MaNXB = dto.MaNXB,
            NamXuatBan = dto.NamXuatBan,
            SoTrang = dto.SoTrang,
            GiaBan = dto.GiaBan,
            // Tồn kho chỉ thay đổi qua phiếu nhập hoàn thành, đơn hàng và hủy đơn (BR03, BR08),
            // nên sách mới luôn bắt đầu từ 0; báo cáo nhập – xuất – tồn dựa vào điều này.
            SoLuongTon = 0,
            NguongCanhBao = dto.NguongCanhBao,
            MoTa = dto.MoTa,
            AnhBia = dto.AnhBia,
            TrangThai = dto.TrangThai
        };
        var loiTacGia = await GanTacGiaAsync(s, dto.MaTacGias);
        if (loiTacGia is not null) return KetQua<int>.Loi(loiTacGia);
        _db.Sach.Add(s);
        await _db.SaveChangesAsync();
        _log.LogInformation("Đã thêm sách {MaSach} - {TenSach}.", s.MaSach, s.TenSach);
        return KetQua<int>.Dung(s.MaSach, "Đã thêm sách.");
    }

    public async Task<KetQua> CapNhatAsync(SachLuuDto dto)
    {
        var loi = KiemTra(dto);
        if (loi is not null) return KetQua.Loi(loi);

        var s = await _db.Sach
            .Include(x => x.Sach_TacGias)
            .FirstOrDefaultAsync(x => x.MaSach == dto.MaSach);
        if (s is null) return KetQua.Loi("Không tìm thấy sách.");

        if (!await _db.LoaiSach.AnyAsync(l => l.MaLoaiSach == dto.MaLoaiSach))
            return KetQua.Loi("Loại sách không tồn tại.");

        if (dto.MaNXB.HasValue && !await _db.NhaXuatBan.AnyAsync(n => n.MaNXB == dto.MaNXB))
            return KetQua.Loi("Nhà xuất bản không tồn tại.");

        if (!string.IsNullOrWhiteSpace(dto.ISBN) &&
            await _db.Sach.AnyAsync(x => x.ISBN == dto.ISBN && x.MaSach != dto.MaSach))
            return KetQua.Loi("ISBN này đã có trong hệ thống.");

        s.ISBN = dto.ISBN?.Trim();
        s.TenSach = dto.TenSach.Trim();
        s.MaLoaiSach = dto.MaLoaiSach;
        s.MaNXB = dto.MaNXB;
        s.NamXuatBan = dto.NamXuatBan;
        s.SoTrang = dto.SoTrang;
        s.GiaBan = dto.GiaBan;
        // Không ghi SoLuongTon: tồn kho không sửa tay được, chỉ đổi qua phiếu nhập và đơn hàng.
        s.NguongCanhBao = dto.NguongCanhBao;
        s.MoTa = dto.MoTa;
        s.AnhBia = dto.AnhBia;
        s.TrangThai = dto.TrangThai;

        var loiTacGia = await GanTacGiaAsync(s, dto.MaTacGias);
        if (loiTacGia is not null) return KetQua.Loi(loiTacGia);

        await _db.SaveChangesAsync();
        _log.LogInformation("Đã cập nhật sách {MaSach}.", s.MaSach);
        return KetQua.Dung("Đã cập nhật sách.");
    }

    /// <summary>
    /// BR09: sach da phat sinh giao dich (don hang hoac phieu nhap) khong duoc xoa,
    /// chi duoc chuyen sang trang thai Ngung kinh doanh.
    /// </summary>
    public async Task<KetQua> XoaAsync(int maSach)
    {
        var s = await _db.Sach.FirstOrDefaultAsync(x => x.MaSach == maSach);
        if (s is null) return KetQua.Loi("Không tìm thấy sách.");

        var daBan = await _db.CTDonHang.AnyAsync(x => x.MaSach == maSach);
        var daNhap = await _db.CTPhieuNhap.AnyAsync(x => x.MaSach == maSach);

        if (daBan || daNhap)
        {
            s.TrangThai = TrangThaiSach.NgungKinhDoanh;
            await _db.SaveChangesAsync();
            _log.LogInformation("Sách {MaSach} đã phát sinh giao dịch, chuyển sang Ngừng kinh doanh (BR09).", maSach);
            return KetQua.Dung("Sách đã phát sinh giao dịch nên được chuyển sang trạng thái Ngừng kinh doanh thay vì xóa.");
        }

        _db.Sach.Remove(s);
        await _db.SaveChangesAsync();
        _log.LogInformation("Đã xóa sách {MaSach}.", maSach);
        return KetQua.Dung("Đã xóa sách.");
    }

    public async Task<List<LoaiSachDto>> LayDanhMucAsync()
        => await _db.LoaiSach
            .AsNoTracking()
            .OrderBy(l => l.TenLoaiSach)
            .Select(l => new LoaiSachDto
            {
                MaLoaiSach = l.MaLoaiSach,
                TenLoaiSach = l.TenLoaiSach,
                MoTa = l.MoTa,
                TrangThai = l.TrangThai,
                SoSach = l.Sachs.Count(s => s.TrangThai == TrangThaiSach.DangKinhDoanh)
            })
            .ToListAsync();

    private static string? KiemTra(SachLuuDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TenSach)) return "Vui lòng nhập tên sách.";
        if (dto.TenSach.Trim().Length > 255) return "Tên sách không được vượt quá 255 ký tự.";
        if (dto.MaLoaiSach <= 0) return "Vui lòng chọn loại sách.";
        if (dto.GiaBan < 0) return "Giá bán không được là số âm.";
        if (dto.SoLuongTon < 0) return "Số lượng tồn không được là số âm.";
        if (dto.NguongCanhBao < 0) return "Ngưỡng cảnh báo không được là số âm.";
        if (dto.NamXuatBan.HasValue && (dto.NamXuatBan < 1900 || dto.NamXuatBan > DateTime.Now.Year + 1))
            return "Năm xuất bản không hợp lệ.";
        return null;
    }

    /// <summary>
    /// Gan lai danh sach tac gia cho sach. MaTacGias = null nghia la khong doi
    /// (giu nguyen tac gia cu), danh sach rong nghia la go het tac gia.
    /// </summary>
    private async Task<string?> GanTacGiaAsync(Sach s, List<int>? maTacGias)
    {
        if (maTacGias is null) return null;

        var ma = maTacGias.Distinct().ToList();
        var soTonTai = await _db.TacGia.CountAsync(t => ma.Contains(t.MaTacGia));
        if (soTonTai != ma.Count) return "Có tác giả không tồn tại.";

        foreach (var cu in s.Sach_TacGias.Where(st => !ma.Contains(st.MaTacGia)).ToList())
            s.Sach_TacGias.Remove(cu);
        foreach (var m in ma.Where(m => s.Sach_TacGias.All(st => st.MaTacGia != m)))
            s.Sach_TacGias.Add(new Sach_TacGia { MaTacGia = m });
        return null;
    }

    private static SachDto ChuyenDoi(Sach s) => new()
    {
        MaSach = s.MaSach,
        ISBN = s.ISBN,
        TenSach = s.TenSach,
        MaLoaiSach = s.MaLoaiSach,
        TenLoaiSach = s.LoaiSach?.TenLoaiSach ?? string.Empty,
        MaNXB = s.MaNXB,
        TenNXB = s.NhaXuatBan?.TenNXB,
        TenTacGia = string.Join(", ", s.Sach_TacGias.Where(st => st.TacGia != null).Select(st => st.TacGia!.TenTacGia)),
        MaTacGias = s.Sach_TacGias.Select(st => st.MaTacGia).ToList(),
        NamXuatBan = s.NamXuatBan,
        SoTrang = s.SoTrang,
        GiaBan = s.GiaBan,
        SoLuongTon = s.SoLuongTon,
        NguongCanhBao = s.NguongCanhBao,
        MoTa = s.MoTa,
        AnhBia = s.AnhBia,
        TrangThai = s.TrangThai
    };
}
