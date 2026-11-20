using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;

namespace BookStore.Tests;

/// <summary>Kiem thu xac thuc, phan quyen va quan ly sach (BR07).</summary>
public class TaiKhoanVaSachTests : CoSoKiemThu
{
    // ---------- Xac thuc ----------

    [Fact]
    public async Task DangNhap_DungMatKhau_TraVeThongTinNguoiDung()
    {
        var kq = await TaiKhoanService.DangNhapAsync(
            new DangNhapDto { TenDangNhap = "khach01", MatKhau = "123456" }, laUngDungQuanTri: false);

        Assert.True(kq.ThanhCong, kq.ThongBao);
        Assert.Equal(TenVaiTro.KhachHang, kq.DuLieu!.VaiTro);
        Assert.Equal(MaKhachHang, kq.DuLieu.MaKhachHang);
    }

    [Fact]
    public async Task DangNhap_SaiMatKhau_TraVeLoi()
    {
        var kq = await TaiKhoanService.DangNhapAsync(
            new DangNhapDto { TenDangNhap = "khach01", MatKhau = "sai-mat-khau" }, laUngDungQuanTri: false);

        Assert.False(kq.ThanhCong);
    }

    [Fact]
    public async Task DangNhap_TaiKhoanKhachHang_KhongVaoDuocUngDungQuanTri()
    {
        var kq = await TaiKhoanService.DangNhapAsync(
            new DangNhapDto { TenDangNhap = "khach01", MatKhau = "123456" }, laUngDungQuanTri: true);

        Assert.False(kq.ThanhCong);
        Assert.Contains("không có quyền", kq.ThongBao);
    }

    [Fact]
    public async Task DangNhap_TaiKhoanQuanLy_KhongVaoDuocTrangWebBanHang()
    {
        var kq = await TaiKhoanService.DangNhapAsync(
            new DangNhapDto { TenDangNhap = "ql01", MatKhau = "123456" }, laUngDungQuanTri: false);

        Assert.False(kq.ThanhCong);
    }

    [Fact]
    public async Task DangNhap_SaiQuaSoLanChoPhep_TamKhoaTaiKhoan()
    {
        var dto = new DangNhapDto { TenDangNhap = "khach01", MatKhau = "sai" };

        for (var i = 0; i < QuyTacNghiepVu.SoLanDangNhapSaiToiDa; i++)
            await TaiKhoanService.DangNhapAsync(dto, laUngDungQuanTri: false);

        // Lan nay nhap dung mat khau nhung tai khoan dang bi tam khoa
        var kq = await TaiKhoanService.DangNhapAsync(
            new DangNhapDto { TenDangNhap = "khach01", MatKhau = "123456" }, laUngDungQuanTri: false);

        Assert.False(kq.ThanhCong);
        Assert.Contains("tạm khóa", kq.ThongBao);
    }

    [Fact]
    public async Task DangKy_TrungTenDangNhap_TraVeLoi()
    {
        var kq = await TaiKhoanService.DangKyAsync(new DangKyDto
        {
            TenDangNhap = "khach01",
            HoTen = "Người Mới",
            Email = "nguoimoi@test.local",
            MatKhau = "123456",
            XacNhanMatKhau = "123456"
        });

        Assert.False(kq.ThanhCong);
        Assert.Contains("đã được sử dụng", kq.ThongBao);
    }

    [Fact]
    public async Task DangKy_MatKhauXacNhanKhongKhop_TraVeLoi()
    {
        var kq = await TaiKhoanService.DangKyAsync(new DangKyDto
        {
            TenDangNhap = "nguoimoi",
            HoTen = "Người Mới",
            Email = "nguoimoi@test.local",
            MatKhau = "123456",
            XacNhanMatKhau = "654321"
        });

        Assert.False(kq.ThanhCong);
        Assert.Contains("không khớp", kq.ThongBao);
    }

    [Fact]
    public async Task DangKy_HopLe_TaoTaiKhoanVaHoSoKhachHang()
    {
        var kq = await TaiKhoanService.DangKyAsync(new DangKyDto
        {
            TenDangNhap = "nguoimoi",
            HoTen = "Người Mới",
            Email = "nguoimoi@test.local",
            SoDienThoai = "0911222333",
            MatKhau = "123456",
            XacNhanMatKhau = "123456"
        });

        Assert.True(kq.ThanhCong, kq.ThongBao);
        Assert.NotNull(kq.DuLieu!.MaKhachHang);
        Assert.Equal(TenVaiTro.KhachHang, kq.DuLieu.VaiTro);

        // Mat khau phai duoc bam, khong luu dang chu thuong
        var tk = Db.TaiKhoan.Single(t => t.TenDangNhap == "nguoimoi");
        Assert.NotEqual("123456", tk.MatKhauHash);
    }

    // ---------- Quan ly sach ----------

    [Fact]
    public async Task ThemSach_ThieuTenSach_TraVeLoi()
    {
        var kq = await SachService.ThemAsync(new SachLuuDto { TenSach = "  ", MaLoaiSach = 1, GiaBan = 1000 });

        Assert.False(kq.ThanhCong);
    }

    [Fact]
    public async Task ThemSach_GiaAm_TraVeLoi()
    {
        var kq = await SachService.ThemAsync(new SachLuuDto
        {
            TenSach = "Sách lỗi",
            MaLoaiSach = Db.LoaiSach.First().MaLoaiSach,
            GiaBan = -1
        });

        Assert.False(kq.ThanhCong);
    }

    [Fact]
    public async Task XoaSach_ChuaPhatSinhGiaoDich_XoaHan()
    {
        var kqThem = await SachService.ThemAsync(new SachLuuDto
        {
            TenSach = "Sách chưa bán",
            MaLoaiSach = Db.LoaiSach.First().MaLoaiSach,
            GiaBan = 50_000,
            SoLuongTon = 3
        });

        var kqXoa = await SachService.XoaAsync(kqThem.DuLieu);

        Assert.True(kqXoa.ThanhCong, kqXoa.ThongBao);
        Assert.Null(await SachService.LayTheoMaAsync(kqThem.DuLieu));
    }

    [Fact]
    public async Task XoaSach_DaCoDonHang_ChiChuyenSangNgungKinhDoanh_Theo_BR07()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 1);
        await DonHangService.DatHangAsync(MaKhachHang, new DatHangDto
        {
            HoTenNguoiNhan = "Người Nhận",
            SoDienThoaiNhan = "0900000000",
            DiaChiGiao = "1 Đường Thử"
        });

        var kq = await SachService.XoaAsync(MaSachA);

        Assert.True(kq.ThanhCong);
        Assert.Contains("Ngừng kinh doanh", kq.ThongBao);

        var sach = await SachService.LayTheoMaAsync(MaSachA);
        Assert.NotNull(sach);
        Assert.Equal(TrangThaiSach.NgungKinhDoanh, sach!.TrangThai);
    }

    [Fact]
    public async Task TimSach_TheoTuKhoa_TraVeDungKetQua()
    {
        var kq = await SachService.TimAsync(new TimSachDto { TuKhoa = "Sách B" });

        Assert.Single(kq.DanhSach);
        Assert.Equal(MaSachB, kq.DanhSach[0].MaSach);
    }

    [Fact]
    public async Task TimSach_LocTheoKhoangGia_TraVeDungKetQua()
    {
        var kq = await SachService.TimAsync(new TimSachDto { GiaTu = 200_000 });

        Assert.Single(kq.DanhSach);
        Assert.Equal("Sách B", kq.DanhSach[0].TenSach);
    }

    [Fact]
    public async Task TimSach_SachNgungKinhDoanh_KhongHienThiTrenTrangWeb()
    {
        var sach = Db.Sach.First(s => s.MaSach == MaSachB);
        sach.TrangThai = TrangThaiSach.NgungKinhDoanh;
        await Db.SaveChangesAsync();

        var trangWeb = await SachService.TimAsync(new TimSachDto { ChiSachDangBan = true });
        var quanTri = await SachService.TimAsync(new TimSachDto { ChiSachDangBan = false });

        Assert.DoesNotContain(trangWeb.DanhSach, s => s.MaSach == MaSachB);
        Assert.Contains(quanTri.DanhSach, s => s.MaSach == MaSachB);
    }
}
