using BookStore.Domain.Entities;
using BookStore.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Tests;

/// <summary>Kiem thu subsystem Tai khoan khach hang: thong tin ca nhan va so dia chi giao hang.</summary>
public class TaiKhoanCuaToiTests : CoSoKiemThu
{
    private static DiaChiGiaoHangDto DiaChi(string ten, bool macDinh = false, int ma = 0) => new()
    {
        MaDiaChi = ma,
        HoTenNguoiNhan = ten,
        SoDienThoai = "0901234567",
        DiaChiChiTiet = "1 Nguyen Trai",
        PhuongXa = "Phuong 1",
        QuanHuyen = "Quan 5",
        TinhThanh = "TP Ho Chi Minh",
        LaMacDinh = macDinh
    };

    [Fact]
    public async Task CapNhatHoSo_HopLe_LuuHoTenSoDienThoaiNgaySinh()
    {
        var kq = await TaiKhoanService.CapNhatHoSoAsync(MaKhachHang, new HoSoKhachHangDto
        {
            HoTen = "  Nguyen Van Moi ", SoDienThoai = "0912345678", NgaySinh = new DateTime(2004, 5, 1), GioiTinh = 1
        });

        Assert.True(kq.ThanhCong, kq.ThongBao);
        var hoSo = await TaiKhoanService.LayHoSoAsync(MaKhachHang);
        Assert.Equal("Nguyen Van Moi", hoSo!.HoTen);
        Assert.Equal("0912345678", hoSo.SoDienThoai);
        Assert.Equal(new DateTime(2004, 5, 1), hoSo.NgaySinh);
        Assert.Equal((byte)1, hoSo.GioiTinh);
    }

    [Theory]
    [InlineData("", "0912345678")]          // thieu ho ten
    [InlineData("Nguyen Van A", "12345")]   // so dien thoai sai
    public async Task CapNhatHoSo_KhongHopLe_BiTuChoi(string hoTen, string sdt)
    {
        var kq = await TaiKhoanService.CapNhatHoSoAsync(MaKhachHang, new HoSoKhachHangDto { HoTen = hoTen, SoDienThoai = sdt });
        Assert.False(kq.ThanhCong);
    }

    [Fact]
    public async Task ThemDiaChi_ChonMacDinh_ChiConMotDiaChiMacDinh()
    {
        Assert.True((await TaiKhoanService.LuuDiaChiAsync(MaKhachHang, DiaChi("Nguoi A"))).ThanhCong);
        Assert.True((await TaiKhoanService.LuuDiaChiAsync(MaKhachHang, DiaChi("Nguoi B", macDinh: true))).ThanhCong);

        var ds = await TaiKhoanService.LayDanhSachDiaChiAsync(MaKhachHang);
        Assert.Single(ds, d => d.LaMacDinh);
        Assert.Equal("Nguoi B", ds.Single(d => d.LaMacDinh).HoTenNguoiNhan);
        Assert.Equal("Nguoi B", (await TaiKhoanService.LayDiaChiMacDinhAsync(MaKhachHang))!.HoTenNguoiNhan);
    }

    [Fact]
    public async Task XoaDiaChiMacDinh_DiaChiConLaiTroThanhMacDinh()
    {
        await TaiKhoanService.LuuDiaChiAsync(MaKhachHang, DiaChi("Nguoi A"));
        await TaiKhoanService.LuuDiaChiAsync(MaKhachHang, DiaChi("Nguoi B", macDinh: true));
        var macDinh = (await TaiKhoanService.LayDanhSachDiaChiAsync(MaKhachHang)).Single(d => d.LaMacDinh);

        var kq = await TaiKhoanService.XoaDiaChiAsync(MaKhachHang, macDinh.MaDiaChi);

        Assert.True(kq.ThanhCong, kq.ThongBao);
        var conLai = await TaiKhoanService.LayDanhSachDiaChiAsync(MaKhachHang);
        Assert.Single(conLai, d => d.LaMacDinh);
        Assert.DoesNotContain(conLai, d => d.MaDiaChi == macDinh.MaDiaChi);
    }

    [Fact]
    public async Task SuaHoacXoaDiaChiCuaNguoiKhac_BiTuChoi()
    {
        // Dia chi thuoc mot khach hang khac
        var tk = new TaiKhoan { TenDangNhap = "khach99", Email = "k99@test.vn", MatKhauHash = "x", MaVaiTro = Db.VaiTro.First().MaVaiTro };
        Db.TaiKhoan.Add(tk);
        await Db.SaveChangesAsync();
        var khac = new KhachHang { MaTaiKhoan = tk.MaTaiKhoan, HoTen = "Khach khac", NgayDangKy = DateTime.Now };
        Db.KhachHang.Add(khac);
        await Db.SaveChangesAsync();
        await TaiKhoanService.LuuDiaChiAsync(khac.MaKhachHang, DiaChi("Cua nguoi khac"));
        var maDiaChiKhac = await Db.DiaChiGiaoHang.Where(d => d.MaKhachHang == khac.MaKhachHang).Select(d => d.MaDiaChi).FirstAsync();

        Assert.False((await TaiKhoanService.LuuDiaChiAsync(MaKhachHang, DiaChi("Sua trom", ma: maDiaChiKhac))).ThanhCong);
        Assert.False((await TaiKhoanService.XoaDiaChiAsync(MaKhachHang, maDiaChiKhac)).ThanhCong);
        Assert.Equal("Cua nguoi khac", (await Db.DiaChiGiaoHang.AsNoTracking().FirstAsync(d => d.MaDiaChi == maDiaChiKhac)).HoTenNguoiNhan);
    }
}
