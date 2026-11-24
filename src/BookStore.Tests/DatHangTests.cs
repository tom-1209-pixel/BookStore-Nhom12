using BookStore.Domain.Enums;
using BookStore.Services.Dtos;

namespace BookStore.Tests;

/// <summary>
/// Kiem thu quy trinh dat hang tren trang Web: BR03 (tru ton kho),
/// BR04 (luu don gia), BR07 (phi van chuyen).
/// </summary>
public class DatHangTests : CoSoKiemThu
{
    private static DatHangDto ThongTinGiao(HinhThucThanhToan ht = HinhThucThanhToan.COD) => new()
    {
        HoTenNguoiNhan = "Người Nhận",
        SoDienThoaiNhan = "0900000000",
        DiaChiGiao = "1 Đường Thử, Quận 1, TP.HCM",
        HinhThucThanhToan = ht
    };

    [Fact]
    public async Task DatHang_TruTonKhoVaTinhDungTongTien()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 2);   // 2 x 100.000 = 200.000

        var kq = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao());

        Assert.True(kq.ThanhCong, kq.ThongBao);
        var don = kq.DuLieu!;

        Assert.Equal(200_000m, don.TamTinh);
        Assert.Equal(25_000m, don.PhiVanChuyen);      // duoi 300.000 nen van thu phi
        Assert.Equal(225_000m, don.TongTien);
        Assert.Equal(8, LayTonKho(MaSachA));          // 10 - 2
    }

    [Fact]
    public async Task DatHang_TuMucBaTramNghin_DuocMienPhiVanChuyen()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachB, 2);   // 2 x 250.000 = 500.000

        var kq = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao());

        Assert.True(kq.ThanhCong, kq.ThongBao);
        Assert.Equal(0m, kq.DuLieu!.PhiVanChuyen);
        Assert.Equal(500_000m, kq.DuLieu.TongTien);
    }

    [Fact]
    public async Task DatHang_VuotTonKho_TraVeLoiVaKhongTruTon()
    {
        // Them 5 quyen (dung bang ton), sau do giam ton xuong 3 de mo phong
        // truong hop sach da duoc nguoi khac mua mat trong luc khach con dang xem gio.
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachB, 5);

        var sach = Db.Sach.First(s => s.MaSach == MaSachB);
        sach.SoLuongTon = 3;
        await Db.SaveChangesAsync();

        var kq = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao());

        Assert.False(kq.ThanhCong);
        Assert.Equal("BR02", kq.MaLoi);
        Assert.Equal(3, LayTonKho(MaSachB));           // ton kho giu nguyen
        Assert.Empty(Db.DonHang);                      // khong tao don nao
    }

    [Fact]
    public async Task DatHang_GioTrong_TraVeLoi()
    {
        var kq = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao());

        Assert.False(kq.ThanhCong);
        Assert.Contains("trống", kq.ThongBao);
    }

    [Fact]
    public async Task DatHang_ThieuDiaChiGiao_TraVeLoi()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 1);

        var dto = ThongTinGiao();
        dto.DiaChiGiao = "   ";

        var kq = await DonHangService.DatHangAsync(MaKhachHang, dto);

        Assert.False(kq.ThanhCong);
        Assert.Contains("địa chỉ", kq.ThongBao, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DatHang_COD_VaoTrangThaiChoXacNhan_ConVNPay_VaoChoThanhToan()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 1);
        var donCod = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao(HinhThucThanhToan.COD));

        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 1);
        var donVnpay = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao(HinhThucThanhToan.VNPay));

        Assert.Equal(TrangThaiDonHang.ChoXacNhan, donCod.DuLieu!.TrangThai);
        Assert.Equal(TrangThaiDonHang.ChoThanhToan, donVnpay.DuLieu!.TrangThai);
    }

    [Fact]
    public async Task DatHang_LuuDonGiaTaiThoiDiemDat_DuSauDoSachDoiGia()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 1);
        var kq = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao());
        var maDon = kq.DuLieu!.MaDonHang;

        // Nha sach tang gia sach sau khi khach da dat
        var sach = Db.Sach.First(s => s.MaSach == MaSachA);
        sach.GiaBan = 150_000m;
        await Db.SaveChangesAsync();

        var don = await DonHangService.LayChiTietAsync(maDon);

        Assert.Equal(100_000m, don!.Dong.Single().DonGia);   // BR04: gia cu duoc giu nguyen
        Assert.Equal(125_000m, don.TongTien);
    }

    [Fact]
    public async Task DatHang_XoaSachTrongGioSauKhiDatThanhCong()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 1);
        await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao());

        var gio = await GioHangService.LayGioHangAsync(MaKhachHang);

        Assert.Empty(gio.Dong);
    }

    [Fact]
    public async Task DatHang_PhatSinhBanGhiThanhToanVaLichSu()
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, 1);
        var kq = await DonHangService.DatHangAsync(MaKhachHang, ThongTinGiao());

        var thanhToan = Db.ThanhToan.Single(t => t.MaDonHang == kq.DuLieu!.MaDonHang);
        Assert.Equal(TrangThaiThanhToan.Cho, thanhToan.TrangThai);
        Assert.Equal(kq.DuLieu!.TongTien, thanhToan.SoTien);

        Assert.Single(kq.DuLieu.LichSu);
    }

    [Fact]
    public async Task ThemVaoGio_VuotTonKho_BiTuChoi()
    {
        var kq = await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachB, 99);

        Assert.False(kq.ThanhCong);
        Assert.Equal("BR02", kq.MaLoi);
    }
}
