using BookStore.Data;
using BookStore.Domain.Enums;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services.Implementations;

/// <summary>
/// Bao cao thong ke (D08). BR10: doanh thu chi tinh cac don o trang thai Hoan thanh.
/// </summary>
public class BaoCaoService : IBaoCaoService
{
    private readonly BookStoreDbContext _db;

    public BaoCaoService(BookStoreDbContext db) => _db = db;

    public async Task<ThongKeDto> LayThongKeAsync()
    {
        var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var dauThangSau = dauThang.AddMonths(1);

        var donThangNay = _db.DonHang
            .Where(d => d.TrangThai == TrangThaiDonHang.HoanThanh
                     && d.NgayDat >= dauThang && d.NgayDat < dauThangSau);

        return new ThongKeDto
        {
            SoDonChoXacNhan = await _db.DonHang.CountAsync(d => d.TrangThai == TrangThaiDonHang.ChoXacNhan),
            SoDonDangGiao = await _db.DonHang.CountAsync(d => d.TrangThai == TrangThaiDonHang.DangGiao),
            SoDonHoanThanhThangNay = await donThangNay.CountAsync(),
            DoanhThuThangNay = await donThangNay.SumAsync(d => (decimal?)d.TongTien) ?? 0m,
            // Moi sach co nguong canh bao rieng (cot NguongCanhBao)
            SoSachSapHetHang = await _db.Sach.CountAsync(
                s => s.TrangThai == TrangThaiSach.DangKinhDoanh && s.SoLuongTon <= s.NguongCanhBao),
            TopSachBanChay = await TopSachBanChayAsync(dauThang, dauThangSau.AddDays(-1), 5)
        };
    }

    public async Task<decimal> TinhDoanhThuAsync(DateTime tuNgay, DateTime denNgay)
        => await _db.DonHang
            .Where(d => d.TrangThai == TrangThaiDonHang.HoanThanh
                     && d.NgayDat >= tuNgay.Date
                     && d.NgayDat < denNgay.Date.AddDays(1))
            .SumAsync(d => (decimal?)d.TongTien) ?? 0m;

    public async Task<List<SachBanChayDto>> TopSachBanChayAsync(DateTime tuNgay, DateTime denNgay, int soDong = 10)
        => await _db.CTDonHang
            .Where(ct => ct.DonHang!.TrangThai == TrangThaiDonHang.HoanThanh
                      && ct.DonHang.NgayDat >= tuNgay.Date
                      && ct.DonHang.NgayDat < denNgay.Date.AddDays(1))
            .GroupBy(ct => ct.TenSach)
            .Select(g => new SachBanChayDto
            {
                TenSach = g.Key,
                SoLuongBan = g.Sum(x => x.SoLuong),
                DoanhThu = g.Sum(x => x.SoLuong * x.DonGia)
            })
            .OrderByDescending(x => x.SoLuongBan)
            .Take(soDong)
            .ToListAsync();

    public async Task<BaoCaoDoanhThuDto> LayBaoCaoDoanhThuAsync(DateTime tuNgay, DateTime denNgay)
    {
        var tu = tuNgay.Date;
        var den = denNgay.Date;

        var dsDon = await _db.DonHang.AsNoTracking()
            .Where(d => d.TrangThai == TrangThaiDonHang.HoanThanh && d.NgayDat >= tu && d.NgayDat < den.AddDays(1))
            .Select(d => new { d.NgayDat, d.TongTien, SoSach = d.CTDonHangs.Sum(ct => ct.SoLuong) })
            .ToListAsync();

        // Gom theo ngay trong bo nho de moi ngay trong ky deu co mot dong, ke ca ngay khong co don
        var theoNgay = new List<DoanhThuNgayDto>();
        for (var ngay = tu; ngay <= den && theoNgay.Count < 366; ngay = ngay.AddDays(1))
        {
            var don = dsDon.Where(d => d.NgayDat.Date == ngay).ToList();
            theoNgay.Add(new DoanhThuNgayDto { Ngay = ngay, SoDon = don.Count, DoanhThu = don.Sum(d => d.TongTien) });
        }

        return new BaoCaoDoanhThuDto
        {
            TuNgay = tu,
            DenNgay = den,
            TongDoanhThu = dsDon.Sum(d => d.TongTien),
            SoDonHoanThanh = dsDon.Count,
            SoSachBanRa = dsDon.Sum(d => d.SoSach),
            TheoNgay = theoNgay,
            TopSachBanChay = await TopSachBanChayAsync(tu, den, 10)
        };
    }

    /// <summary>
    /// Bao cao nhap - xuat - ton trong ky.
    /// Nhap = so luong cua cac phieu nhap Hoan thanh trong ky (BR08).
    /// Xuat = so luong cua cac don tao trong ky va khong bi huy (ton kho bi tru ngay khi tao don - BR03,
    /// don huy da duoc hoan lai - BR03).
    /// Ton cuoi ky duoc suy nguoc tu ton hien tai, ton dau ky = ton cuoi - nhap + xuat.
    /// </summary>
    public async Task<List<NhapXuatTonDto>> LayNhapXuatTonAsync(DateTime tuNgay, DateTime denNgay)
    {
        var tu = tuNgay.Date;
        var sauKy = denNgay.Date.AddDays(1);

        var sachs = await _db.Sach.AsNoTracking()
            .Select(s => new { s.MaSach, s.TenSach, s.SoLuongTon })
            .ToListAsync();

        var nhap = await _db.CTPhieuNhap.AsNoTracking()
            .Where(ct => ct.PhieuNhap!.TrangThai == TrangThaiPhieuNhap.HoanThanh && ct.PhieuNhap.NgayHoanThanh >= tu)
            .Select(ct => new { ct.MaSach, ct.SoLuong, Ngay = ct.PhieuNhap!.NgayHoanThanh!.Value })
            .ToListAsync();

        var xuat = await _db.CTDonHang.AsNoTracking()
            .Where(ct => ct.DonHang!.TrangThai != TrangThaiDonHang.DaHuy && ct.DonHang.NgayDat >= tu)
            .Select(ct => new { ct.MaSach, ct.SoLuong, Ngay = ct.DonHang!.NgayDat })
            .ToListAsync();

        var kq = new List<NhapXuatTonDto>();
        foreach (var s in sachs)
        {
            var nhapTrongKy = nhap.Where(x => x.MaSach == s.MaSach && x.Ngay < sauKy).Sum(x => x.SoLuong);
            var nhapSauKy = nhap.Where(x => x.MaSach == s.MaSach && x.Ngay >= sauKy).Sum(x => x.SoLuong);
            var xuatTrongKy = xuat.Where(x => x.MaSach == s.MaSach && x.Ngay < sauKy).Sum(x => x.SoLuong);
            var xuatSauKy = xuat.Where(x => x.MaSach == s.MaSach && x.Ngay >= sauKy).Sum(x => x.SoLuong);

            var tonCuoi = s.SoLuongTon - nhapSauKy + xuatSauKy;
            kq.Add(new NhapXuatTonDto
            {
                MaSach = s.MaSach,
                TenSach = s.TenSach,
                TonCuoiKy = tonCuoi,
                NhapTrongKy = nhapTrongKy,
                XuatTrongKy = xuatTrongKy,
                TonDauKy = tonCuoi - nhapTrongKy + xuatTrongKy
            });
        }

        return kq.OrderBy(x => x.TenSach).ToList();
    }

    public async Task<List<DonTheoTrangThaiDto>> DemDonTheoTrangThaiAsync(DateTime tuNgay, DateTime denNgay)
    {
        var tu = tuNgay.Date;
        var sauKy = denNgay.Date.AddDays(1);

        var ds = await _db.DonHang.AsNoTracking()
            .Where(d => d.NgayDat >= tu && d.NgayDat < sauKy)
            .Select(d => new { d.TrangThai, d.TongTien })
            .ToListAsync();

        return Enum.GetValues<TrangThaiDonHang>()
            .Select(tt => new DonTheoTrangThaiDto
            {
                TrangThai = tt,
                SoDon = ds.Count(d => d.TrangThai == tt),
                TongTien = ds.Where(d => d.TrangThai == tt).Sum(d => d.TongTien)
            })
            .ToList();
    }
}
