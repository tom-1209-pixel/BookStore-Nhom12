using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services.Implementations;

public class GioHangService : IGioHangService
{
    private readonly BookStoreDbContext _db;
    private readonly ILogger<GioHangService> _log;

    public GioHangService(BookStoreDbContext db, ILogger<GioHangService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<GioHangDto> LayGioHangAsync(int maKhachHang)
    {
        var gio = await _db.GioHang
            .Include(g => g.CTGioHangs).ThenInclude(ct => ct.Sach)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

        if (gio is null) return new GioHangDto();

        return new GioHangDto
        {
            MaGioHang = gio.MaGioHang,
            Dong = gio.CTGioHangs
                .OrderBy(ct => ct.NgayThem)
                .Select(ct => new DongGioHangDto
                {
                    MaSach = ct.MaSach,
                    TenSach = ct.Sach?.TenSach ?? string.Empty,
                    AnhBia = ct.Sach?.AnhBia,
                    DonGia = ct.Sach?.GiaBan ?? 0m,
                    SoLuong = ct.SoLuong,
                    SoLuongTon = ct.Sach?.SoLuongTon ?? 0
                })
                .ToList()
        };
    }

    public async Task<KetQua> ThemVaoGioAsync(int maKhachHang, int maSach, int soLuong)
    {
        if (soLuong <= 0) return KetQua.Loi("Số lượng phải lớn hơn 0.");

        var sach = await _db.Sach.FirstOrDefaultAsync(s => s.MaSach == maSach);
        if (sach is null) return KetQua.Loi("Không tìm thấy sách.");
        if (sach.TrangThai == TrangThaiSach.NgungKinhDoanh)
            return KetQua.Loi("Sách này đã ngừng kinh doanh.");

        var gio = await LayHoacTaoGioAsync(maKhachHang);

        var dong = await _db.CTGioHang.FirstOrDefaultAsync(ct => ct.MaGioHang == gio.MaGioHang && ct.MaSach == maSach);
        var soLuongMoi = (dong?.SoLuong ?? 0) + soLuong;

        if (soLuongMoi > sach.SoLuongTon)
            return KetQua.Loi($"Số lượng tồn chỉ còn {sach.SoLuongTon} quyển.", "BR02");

        if (dong is null)
            _db.CTGioHang.Add(new CTGioHang { MaGioHang = gio.MaGioHang, MaSach = maSach, SoLuong = soLuong });
        else
            dong.SoLuong = soLuongMoi;

        gio.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        _log.LogInformation("Khách hàng {MaKhachHang} thêm sách {MaSach} x{SoLuong} vào giỏ.", maKhachHang, maSach, soLuong);
        return KetQua.Dung("Đã thêm vào giỏ hàng.");
    }

    public async Task<KetQua> CapNhatSoLuongAsync(int maKhachHang, int maSach, int soLuong)
    {
        var gio = await _db.GioHang.FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);
        if (gio is null) return KetQua.Loi("Giỏ hàng đang trống.");

        var dong = await _db.CTGioHang.FirstOrDefaultAsync(ct => ct.MaGioHang == gio.MaGioHang && ct.MaSach == maSach);
        if (dong is null) return KetQua.Loi("Sách không có trong giỏ hàng.");

        if (soLuong <= 0)
        {
            _db.CTGioHang.Remove(dong);
        }
        else
        {
            var sach = await _db.Sach.FirstOrDefaultAsync(s => s.MaSach == maSach);
            if (sach is null) return KetQua.Loi("Không tìm thấy sách.");
            if (soLuong > sach.SoLuongTon)
                return KetQua.Loi($"Số lượng tồn chỉ còn {sach.SoLuongTon} quyển.", "BR02");
            dong.SoLuong = soLuong;
        }

        gio.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        return KetQua.Dung("Đã cập nhật giỏ hàng.");
    }

    public async Task<KetQua> XoaDongAsync(int maKhachHang, int maSach)
        => await CapNhatSoLuongAsync(maKhachHang, maSach, 0);

    public async Task<KetQua> XoaGioAsync(int maKhachHang)
    {
        var gio = await _db.GioHang
            .Include(g => g.CTGioHangs)
            .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);
        if (gio is null) return KetQua.Dung("Giỏ hàng đã trống.");

        _db.CTGioHang.RemoveRange(gio.CTGioHangs);
        gio.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        return KetQua.Dung("Đã xóa giỏ hàng.");
    }

    private async Task<GioHang> LayHoacTaoGioAsync(int maKhachHang)
    {
        var gio = await _db.GioHang.FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);
        if (gio is not null) return gio;

        gio = new GioHang { MaKhachHang = maKhachHang };
        _db.GioHang.Add(gio);
        await _db.SaveChangesAsync();
        return gio;
    }
}
