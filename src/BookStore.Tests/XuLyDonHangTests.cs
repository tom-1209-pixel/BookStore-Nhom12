using Microsoft.EntityFrameworkCore;
using BookStore.Domain.Enums;
using BookStore.Services.Dtos;

namespace BookStore.Tests;

/// <summary>
/// Kiem thu phia ung dung quan tri: so do chuyen trang thai don hang,
/// BR03 (hoan ton kho khi huy), BR05 (huy don qua han), BR06 (khach tu huy),
/// BR10 (doanh thu chi tinh don hoan thanh).
/// </summary>
public class XuLyDonHangTests : CoSoKiemThu
{
    private async Task<int> TaoDonAsync(int soLuong = 2, HinhThucThanhToan ht = HinhThucThanhToan.COD)
    {
        await GioHangService.ThemVaoGioAsync(MaKhachHang, MaSachA, soLuong);
        var kq = await DonHangService.DatHangAsync(MaKhachHang, new DatHangDto
        {
            HoTenNguoiNhan = "Người Nhận",
            SoDienThoaiNhan = "0900000000",
            DiaChiGiao = "1 Đường Thử, Quận 1, TP.HCM",
            HinhThucThanhToan = ht
        });
        Assert.True(kq.ThanhCong, kq.ThongBao);
        return kq.DuLieu!.MaDonHang;
    }

    [Fact]
    public async Task ChuyenTrangThai_TheoDungSoDo_ThanhCong()
    {
        var ma = await TaoDonAsync();

        Assert.True((await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DaXacNhan, "ql01", null)).ThanhCong);
        Assert.True((await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DangGiao, "ql01", null)).ThanhCong);
        Assert.True((await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.HoanThanh, "ql01", null)).ThanhCong);

        var don = await DonHangService.LayChiTietAsync(ma);
        Assert.Equal(TrangThaiDonHang.HoanThanh, don!.TrangThai);
        Assert.Equal(4, don.LichSu.Count);   // 1 ban ghi luc tao + 3 lan chuyen
    }

    [Theory]
    [InlineData(TrangThaiDonHang.DangGiao)]    // bo qua buoc Da xac nhan
    [InlineData(TrangThaiDonHang.HoanThanh)]   // bo qua hai buoc
    public async Task ChuyenTrangThai_BoQuaBuoc_BiTuChoi(TrangThaiDonHang trangThaiMoi)
    {
        var ma = await TaoDonAsync();

        var kq = await DonHangService.ChuyenTrangThaiAsync(ma, trangThaiMoi, "ql01", null);

        Assert.False(kq.ThanhCong);
        Assert.Contains("Không thể chuyển", kq.ThongBao);
    }

    [Fact]
    public async Task ChuyenTrangThai_DonDaHoanThanh_KhongDoiDuocNua()
    {
        var ma = await TaoDonAsync();
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DaXacNhan, "ql01", null);
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DangGiao, "ql01", null);
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.HoanThanh, "ql01", null);

        var kq = await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DaHuy, "ql01", null);

        Assert.False(kq.ThanhCong);
    }

    [Fact]
    public async Task HuyDon_HoanLaiTonKho_Theo_BR03()
    {
        var tonBanDau = LayTonKho(MaSachA);
        var ma = await TaoDonAsync(soLuong: 3);
        Assert.Equal(tonBanDau - 3, LayTonKho(MaSachA));

        var kq = await DonHangService.ChuyenTrangThaiAsync(
            ma, TrangThaiDonHang.DaHuy, "ql01", null, "Khách báo không nhận hàng");

        Assert.True(kq.ThanhCong, kq.ThongBao);
        Assert.Equal(tonBanDau, LayTonKho(MaSachA));   // ton kho tro ve nhu cu
    }

    [Fact]
    public async Task GiaoThatBai_DuocGhiNhanBangTrangThaiDaHuy_KemLyDo()
    {
        var ma = await TaoDonAsync();
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DaXacNhan, "ql01", null);
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DangGiao, "ql01", null);

        var kq = await DonHangService.ChuyenTrangThaiAsync(
            ma, TrangThaiDonHang.DaHuy, "ql01", null, "Giao 3 lần không gặp khách");

        Assert.True(kq.ThanhCong, kq.ThongBao);
        var don = await DonHangService.LayChiTietAsync(ma);
        Assert.Equal(TrangThaiDonHang.DaHuy, don!.TrangThai);
        Assert.Contains("Giao 3 lần không gặp khách", don.LichSu.Last().GhiChu);
    }

    [Fact]
    public async Task KhachHuyDon_OTrangThaiChoXacNhan_ThanhCong()
    {
        var ma = await TaoDonAsync();

        var kq = await DonHangService.KhachHuyDonAsync(ma, MaKhachHang, "Đặt nhầm sách");

        Assert.True(kq.ThanhCong, kq.ThongBao);
    }

    [Fact]
    public async Task KhachHuyDon_SauKhiDaXacNhan_BiTuChoi_Theo_BR06()
    {
        var ma = await TaoDonAsync();
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DaXacNhan, "ql01", null);

        var kq = await DonHangService.KhachHuyDonAsync(ma, MaKhachHang, null);

        Assert.False(kq.ThanhCong);
        Assert.Equal("BR06", kq.MaLoi);
    }

    [Fact]
    public async Task KhachHuyDon_CuaNguoiKhac_KhongTimThay()
    {
        var ma = await TaoDonAsync();

        var kq = await DonHangService.KhachHuyDonAsync(ma, MaKhachHang + 999, null);

        Assert.False(kq.ThanhCong);
    }

    [Fact]
    public async Task HuyDonQuaHan_ChiHuyDonVNPayDaQuaBaMuoiPhut_Theo_BR05()
    {
        var maQuaHan = await TaoDonAsync(1, HinhThucThanhToan.VNPay);
        var maMoiDat = await TaoDonAsync(1, HinhThucThanhToan.VNPay);

        // Day ngay dat cua don thu nhat lui lai 45 phut
        var don = Db.DonHang.First(d => d.MaDonHang == maQuaHan);
        don.NgayDat = DateTime.Now.AddMinutes(-45);
        await Db.SaveChangesAsync();

        var soDonHuy = await DonHangService.HuyDonQuaHanAsync();

        Assert.Equal(1, soDonHuy);
        Assert.Equal(TrangThaiDonHang.DaHuy, (await DonHangService.LayChiTietAsync(maQuaHan))!.TrangThai);
        Assert.Equal(TrangThaiDonHang.ChoThanhToan, (await DonHangService.LayChiTietAsync(maMoiDat))!.TrangThai);
    }

    [Fact]
    public async Task DoanhThu_ChiTinhDonHoanThanh_Theo_BR10()
    {
        var maHoanThanh = await TaoDonAsync(1);   // 100.000 + 25.000 = 125.000
        await DonHangService.ChuyenTrangThaiAsync(maHoanThanh, TrangThaiDonHang.DaXacNhan, "ql01", null);
        await DonHangService.ChuyenTrangThaiAsync(maHoanThanh, TrangThaiDonHang.DangGiao, "ql01", null);
        await DonHangService.ChuyenTrangThaiAsync(maHoanThanh, TrangThaiDonHang.HoanThanh, "ql01", null);

        await TaoDonAsync(1);   // don nay van dang Cho xac nhan, khong tinh vao doanh thu

        var doanhThu = await BaoCaoService.TinhDoanhThuAsync(DateTime.Today, DateTime.Today);

        Assert.Equal(125_000m, doanhThu);
    }

    [Fact]
    public async Task DonHoanThanh_GhiNhanThanhToanThanhCong()
    {
        var ma = await TaoDonAsync(1);
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DaXacNhan, "ql01", null);
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DangGiao, "ql01", null);
        await DonHangService.ChuyenTrangThaiAsync(ma, TrangThaiDonHang.HoanThanh, "ql01", null);

        Db.ChangeTracker.Clear();
        var tt = Db.ThanhToan.AsNoTracking().Single(t => t.MaDonHang == ma);

        Assert.Equal(TrangThaiThanhToan.ThanhCong, tt.TrangThai);
    }
}
