using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Services.Implementations;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BookStore.Services.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BookStore.Tests;

/// <summary>
/// Lop nen cho cac lop kiem thu. Moi bo kiem thu chay tren mot CSDL SQLite
/// nam trong bo nho rieng biet nen khong anh huong lan nhau va khong can
/// cai dat SQL Server khi chay kiem thu.
/// </summary>
public abstract class CoSoKiemThu : IDisposable
{
    private readonly SqliteConnection _ketNoi;
    protected readonly BookStoreDbContext Db;

    /// <summary>Email gia: ghi nho thu thay vi gui that, de kiem tra noi dung.</summary>
    protected readonly EmailGhiNho Email = new();

    /// <summary>Cau hinh VNPay rong: he thong dung cong gia lap va khoa ky gia lap.</summary>
    protected readonly CauHinhVnPay CauHinhVnPay = new();

    protected ITaiKhoanService TaiKhoanService => new TaiKhoanService(Db, NullLogger<TaiKhoanService>.Instance, Email);
    protected ISachService SachService => new SachService(Db, NullLogger<SachService>.Instance);
    protected IGioHangService GioHangService => new GioHangService(Db, NullLogger<GioHangService>.Instance);
    protected IDonHangService DonHangService => new DonHangService(Db, NullLogger<DonHangService>.Instance, Email);
    protected IBaoCaoService BaoCaoService => new BaoCaoService(Db);
    protected IDanhMucService DanhMucService => new DanhMucService(Db, NullLogger<DanhMucService>.Instance);
    protected IKhoService KhoService => new KhoService(Db, NullLogger<KhoService>.Instance);
    protected IKhachHangService KhachHangService => new KhachHangService(Db, NullLogger<KhachHangService>.Instance);
    protected IThanhToanService ThanhToanService => new ThanhToanService(Db, DonHangService,
        Options.Create(CauHinhVnPay), NullLogger<ThanhToanService>.Instance);
    protected IXuatExcelService XuatExcelService => new XuatExcelService(BaoCaoService);

    // Du lieu mau duoc tao san cho moi bo kiem thu
    protected int MaKhachHang;
    protected int MaQuanLy;
    protected int MaNhaCungCap;
    protected int MaSachA;   // gia 100.000, ton 10
    protected int MaSachB;   // gia 250.000, ton 5

    protected CoSoKiemThu()
    {
        _ketNoi = new SqliteConnection("DataSource=:memory:");
        _ketNoi.Open();

        var options = new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseSqlite(_ketNoi)
            .Options;

        Db = new BookStoreDbContext(options);
        Db.Database.EnsureCreated();

        TaoDuLieuMau();
    }

    private void TaoDuLieuMau()
    {
        var hasher = new PasswordHasher<TaiKhoan>();

        var vtKhach = new VaiTro { TenVaiTro = TenVaiTro.KhachHang };
        var vtQuanLy = new VaiTro { TenVaiTro = TenVaiTro.QuanLy };
        Db.VaiTro.AddRange(vtKhach, vtQuanLy);
        Db.SaveChanges();

        var tkKhach = new TaiKhoan
        {
            TenDangNhap = "khach01",
            Email = "khach01@test.local",
            MaVaiTro = vtKhach.MaVaiTro
        };
        tkKhach.MatKhauHash = hasher.HashPassword(tkKhach, "123456");

        var tkQuanLy = new TaiKhoan
        {
            TenDangNhap = "ql01",
            Email = "ql01@test.local",
            MaVaiTro = vtQuanLy.MaVaiTro
        };
        tkQuanLy.MatKhauHash = hasher.HashPassword(tkQuanLy, "123456");

        Db.TaiKhoan.AddRange(tkKhach, tkQuanLy);
        Db.SaveChanges();

        var kh = new KhachHang { MaTaiKhoan = tkKhach.MaTaiKhoan, HoTen = "Khách Kiểm Thử" };
        Db.KhachHang.Add(kh);
        var ql = new QuanLy { MaTaiKhoan = tkQuanLy.MaTaiKhoan, HoTen = "Quản Lý Kiểm Thử" };
        Db.QuanLy.Add(ql);
        var ncc = new NhaCungCap { TenNCC = "NCC Kiểm thử", SoDienThoai = "0900000000" };
        Db.NhaCungCap.Add(ncc);
        Db.SaveChanges();
        MaKhachHang = kh.MaKhachHang;
        MaQuanLy = ql.MaQuanLy;
        MaNhaCungCap = ncc.MaNCC;

        var loai = new LoaiSach { TenLoaiSach = "Kiểm thử" };
        Db.LoaiSach.Add(loai);
        Db.SaveChanges();

        var sachA = new Sach { TenSach = "Sách A", MaLoaiSach = loai.MaLoaiSach, GiaBan = 100_000m, SoLuongTon = 10 };
        var sachB = new Sach { TenSach = "Sách B", MaLoaiSach = loai.MaLoaiSach, GiaBan = 250_000m, SoLuongTon = 5 };
        Db.Sach.AddRange(sachA, sachB);
        Db.SaveChanges();

        MaSachA = sachA.MaSach;
        MaSachB = sachB.MaSach;
    }

    /// <summary>Doc lai so luong ton hien tai cua mot quyen sach tu CSDL.</summary>
    protected int LayTonKho(int maSach)
    {
        Db.ChangeTracker.Clear();
        return Db.Sach.AsNoTracking().First(s => s.MaSach == maSach).SoLuongTon;
    }

    /// <summary>Them sach vao gio roi dat hang, tra ve ma don moi tao.</summary>
    protected async Task<int> DatDonAsync(HinhThucThanhToan hinhThuc, params (int MaSach, int SoLuong)[] dong)
    {
        foreach (var (ma, sl) in dong)
            await GioHangService.ThemVaoGioAsync(MaKhachHang, ma, sl);
        var kq = await DonHangService.DatHangAsync(MaKhachHang, new Services.Dtos.DatHangDto
        {
            HoTenNguoiNhan = "Người Nhận", SoDienThoaiNhan = "0901234567",
            DiaChiGiao = "1 Đường A, Quận 5", HinhThucThanhToan = hinhThuc
        });
        Assert.True(kq.ThanhCong, kq.ThongBao);
        return kq.DuLieu!.MaDonHang;
    }

    public void Dispose()
    {
        Db.Dispose();
        _ketNoi.Dispose();
        GC.SuppressFinalize(this);
    }
}
