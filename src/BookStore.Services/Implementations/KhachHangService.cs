using BookStore.Data;
using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services.Implementations;

/// <summary>Quan ly khach hang tren ung dung quan tri (D07): xem, tra cuu, khoa / mo khoa.</summary>
public class KhachHangService : IKhachHangService
{
    private readonly BookStoreDbContext _db;
    private readonly ILogger<KhachHangService> _log;

    public KhachHangService(BookStoreDbContext db, ILogger<KhachHangService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<List<KhachHangDto>> TimAsync(string? tuKhoa = null, TrangThaiTaiKhoan? trangThai = null)
    {
        var q = _db.KhachHang.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            q = q.Where(k => EF.Functions.Like(k.HoTen, $"%{tk}%")
                          || EF.Functions.Like(k.TaiKhoan!.Email, $"%{tk}%")
                          || EF.Functions.Like(k.TaiKhoan!.TenDangNhap, $"%{tk}%")
                          || (k.TaiKhoan!.SoDienThoai != null && EF.Functions.Like(k.TaiKhoan.SoDienThoai, $"%{tk}%")));
        }

        if (trangThai.HasValue)
            q = q.Where(k => k.TaiKhoan!.TrangThai == trangThai.Value);

        return await q
            .OrderByDescending(k => k.MaKhachHang)
            .Select(k => new KhachHangDto
            {
                MaKhachHang = k.MaKhachHang,
                MaTaiKhoan = k.MaTaiKhoan,
                HoTen = k.HoTen,
                TenDangNhap = k.TaiKhoan!.TenDangNhap,
                Email = k.TaiKhoan.Email,
                SoDienThoai = k.TaiKhoan.SoDienThoai,
                NgayDangKy = k.NgayDangKy,
                TrangThai = k.TaiKhoan.TrangThai,
                SoDonHang = k.DonHangs.Count,
                TongChiTieu = k.DonHangs.Where(d => d.TrangThai == TrangThaiDonHang.HoanThanh)
                    .Sum(d => (decimal?)d.TongTien) ?? 0m
            })
            .ToListAsync();
    }

    public async Task<KhachHangDto?> LayChiTietAsync(int maKhachHang)
    {
        var kh = (await TimTheoMaAsync(maKhachHang));
        if (kh is null) return null;

        kh.DiaChiMacDinh = await _db.DiaChiGiaoHang.AsNoTracking()
            .Where(d => d.MaKhachHang == maKhachHang)
            .OrderByDescending(d => d.LaMacDinh).ThenByDescending(d => d.MaDiaChi)
            .Select(d => d.DiaChiChiTiet + ", " + d.PhuongXa + ", " + d.QuanHuyen + ", " + d.TinhThanh)
            .FirstOrDefaultAsync();
        return kh;
    }

    /// <summary>Khoa hoac mo khoa tai khoan khach hang. Tai khoan bi khoa khong dang nhap duoc trang Web.</summary>
    public async Task<KetQua> DoiTrangThaiAsync(int maKhachHang, bool khoa, string? lyDo)
    {
        var kh = await _db.KhachHang.Include(k => k.TaiKhoan)
            .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);
        if (kh?.TaiKhoan is null) return KetQua.Loi("Không tìm thấy khách hàng.");

        if (khoa && string.IsNullOrWhiteSpace(lyDo))
            return KetQua.Loi("Vui lòng nhập lý do khóa tài khoản.");

        var moi = khoa ? TrangThaiTaiKhoan.BiKhoa : TrangThaiTaiKhoan.HoatDong;
        if (kh.TaiKhoan.TrangThai == moi)
            return KetQua.Loi(khoa ? "Tài khoản đã bị khóa từ trước." : "Tài khoản đang hoạt động.");

        kh.TaiKhoan.TrangThai = moi;
        if (!khoa)
        {
            kh.TaiKhoan.SoLanDangNhapSai = 0;
            kh.TaiKhoan.ThoiDiemHetKhoa = null;
        }
        await _db.SaveChangesAsync();

        _log.LogInformation("{HanhDong} tài khoản khách hàng {MaKhachHang}. Lý do: {LyDo}",
            khoa ? "Khóa" : "Mở khóa", maKhachHang, lyDo ?? "(không ghi)");
        return KetQua.Dung(khoa ? $"Đã khóa tài khoản của {kh.HoTen}." : $"Đã mở khóa tài khoản của {kh.HoTen}.");
    }

    private async Task<KhachHangDto?> TimTheoMaAsync(int maKhachHang)
        => await _db.KhachHang.AsNoTracking()
            .Where(k => k.MaKhachHang == maKhachHang)
            .Select(k => new KhachHangDto
            {
                MaKhachHang = k.MaKhachHang,
                MaTaiKhoan = k.MaTaiKhoan,
                HoTen = k.HoTen,
                TenDangNhap = k.TaiKhoan!.TenDangNhap,
                Email = k.TaiKhoan.Email,
                SoDienThoai = k.TaiKhoan.SoDienThoai,
                NgayDangKy = k.NgayDangKy,
                TrangThai = k.TaiKhoan.TrangThai,
                SoDonHang = k.DonHangs.Count,
                TongChiTieu = k.DonHangs.Where(d => d.TrangThai == TrangThaiDonHang.HoanThanh)
                    .Sum(d => (decimal?)d.TongTien) ?? 0m
            })
            .FirstOrDefaultAsync();
}
