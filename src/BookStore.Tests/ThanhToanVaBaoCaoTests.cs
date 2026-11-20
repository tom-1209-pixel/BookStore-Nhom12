using BookStore.Domain.Enums;
using BookStore.Services.Implementations;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Tests;

/// <summary>Kiem thu thanh toan VNPay (chu ky, ket qua tra ve), email, quen mat khau va bao cao.</summary>
public class ThanhToanVaBaoCaoTests : CoSoKiemThu
{
    private static Dictionary<string, string> DocTruyVan(string url)
    {
        var q = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
        return q.AllKeys.Where(k => k is not null).ToDictionary(k => k!, k => q[k]!);
    }

    private async Task<Dictionary<string, string>> ThanhToanGiaLapAsync(int maDon, bool thanhCong)
    {
        var yc = await ThanhToanService.TaoYeuCauThanhToanAsync(maDon, MaKhachHang, "127.0.0.1",
            "http://localhost/tra-ve", "http://localhost/gia-lap");
        Assert.True(yc.ThanhCong, yc.ThongBao);
        var kq = ThanhToanService.TaoKetQuaGiaLap(DocTruyVan(yc.DuLieu!), thanhCong);
        Assert.True(kq.ThanhCong, kq.ThongBao);
        return DocTruyVan(kq.DuLieu!);
    }

    [Fact]
    public void ChuKyVnPay_SapXepVaMaHoaDungDacTa()
    {
        var thamSo = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = "5-101010", ["vnp_Amount"] = "12500000", ["vnp_OrderInfo"] = "Thanh toan don hang 5",
            ["vnp_SecureHash"] = "bo-qua", ["khac"] = "bo-qua"
        };

        // Chi lay tham so vnp_*, bo vnp_SecureHash, sap xep theo ten va ma hoa URL (dau cach thanh +)
        Assert.Equal("vnp_Amount=12500000&vnp_OrderInfo=Thanh+toan+don+hang+5&vnp_TxnRef=5-101010",
            VnPayChuKy.TaoChuoiTruyVan(thamSo));

        var chuKy = VnPayChuKy.Ky(thamSo, "khoa");
        Assert.Equal(128, chuKy.Length);   // HMAC-SHA512 o dang hex
        thamSo["vnp_SecureHash"] = chuKy;
        Assert.True(VnPayChuKy.KiemTra(thamSo, "khoa"));
        Assert.False(VnPayChuKy.KiemTra(thamSo, "khoa-khac"));

        thamSo["vnp_Amount"] = "1";   // bi sua so tien sau khi ky
        Assert.False(VnPayChuKy.KiemTra(thamSo, "khoa"));
    }

    [Fact]
    public async Task ThanhToanVnPayThanhCong_DonSangChoXacNhan_VaGhiNhanGiaoDich()
    {
        var maDon = await DatDonAsync(HinhThucThanhToan.VNPay, (MaSachA, 2));
        var traVe = await ThanhToanGiaLapAsync(maDon, thanhCong: true);

        var kq = await ThanhToanService.XuLyKetQuaTraVeAsync(traVe);

        Assert.True(kq.ThanhCong, kq.ThongBao);
        Assert.Equal(maDon, kq.DuLieu);
        Db.ChangeTracker.Clear();
        var don = await Db.DonHang.Include(d => d.ThanhToans).FirstAsync(d => d.MaDonHang == maDon);
        Assert.Equal(TrangThaiDonHang.ChoXacNhan, don.TrangThai);
        var tt = Assert.Single(don.ThanhToans, t => t.TrangThai == TrangThaiThanhToan.ThanhCong);
        Assert.False(string.IsNullOrEmpty(tt.MaGiaoDich));

        // Gui lai cung ket qua (tai lai trang) khong ghi nhan lan hai
        Assert.True((await ThanhToanService.XuLyKetQuaTraVeAsync(traVe)).ThanhCong);
        Db.ChangeTracker.Clear();
        Assert.Equal(1, await Db.ThanhToan.CountAsync(t => t.MaDonHang == maDon && t.TrangThai == TrangThaiThanhToan.ThanhCong));
    }

    [Fact]
    public async Task ThanhToanVnPayBiHuy_DonVanChoThanhToan_VaThanhToanLaiDuoc()
    {
        var maDon = await DatDonAsync(HinhThucThanhToan.VNPay, (MaSachA, 1));

        var kq = await ThanhToanService.XuLyKetQuaTraVeAsync(await ThanhToanGiaLapAsync(maDon, thanhCong: false));
        Assert.False(kq.ThanhCong);
        Assert.Contains("hủy giao dịch", kq.ThongBao);

        Db.ChangeTracker.Clear();
        Assert.Equal(TrangThaiDonHang.ChoThanhToan, (await Db.DonHang.FindAsync(maDon))!.TrangThai);

        // Lan thu hai thanh cong
        var kq2 = await ThanhToanService.XuLyKetQuaTraVeAsync(await ThanhToanGiaLapAsync(maDon, thanhCong: true));
        Assert.True(kq2.ThanhCong, kq2.ThongBao);
    }

    [Fact]
    public async Task KetQuaTraVeBiSua_KhongDuocGhiNhan()
    {
        var maDon = await DatDonAsync(HinhThucThanhToan.VNPay, (MaSachA, 1));
        var traVe = await ThanhToanGiaLapAsync(maDon, thanhCong: false);
        traVe["vnp_ResponseCode"] = "00";            // ke gian sua thanh "thanh cong"
        traVe["vnp_TransactionStatus"] = "00";

        var kq = await ThanhToanService.XuLyKetQuaTraVeAsync(traVe);

        Assert.False(kq.ThanhCong);
        Assert.Contains("Chữ ký", kq.ThongBao);
        Db.ChangeTracker.Clear();
        Assert.Equal(TrangThaiDonHang.ChoThanhToan, (await Db.DonHang.FindAsync(maDon))!.TrangThai);
    }

    [Fact]
    public async Task DonCod_KhongTaoDuocYeuCauThanhToanTrucTuyen()
    {
        var maDon = await DatDonAsync(HinhThucThanhToan.COD, (MaSachA, 1));
        var yc = await ThanhToanService.TaoYeuCauThanhToanAsync(maDon, MaKhachHang, "127.0.0.1", "http://a", "http://b");
        Assert.False(yc.ThanhCong);
    }

    [Fact]
    public async Task DatHang_VaChuyenTrangThai_DeuGuiEmailChoKhach()
    {
        var maDon = await DatDonAsync(HinhThucThanhToan.COD, (MaSachA, 1));
        Assert.Contains(Email.Thu, t => t.NguoiNhan == "khach01@test.local" && t.TieuDe.Contains($"#{maDon}"));

        await DonHangService.ChuyenTrangThaiAsync(maDon, TrangThaiDonHang.DaXacNhan, "ql01", MaQuanLy);
        Assert.Contains(Email.Thu, t => t.TieuDe.Contains("Đã xác nhận"));
    }

    [Fact]
    public async Task QuenMatKhau_TokenDungMotLan_VaMatKhauMoiDangNhapDuoc()
    {
        string? lienKet = null;
        var kq = await TaiKhoanService.YeuCauDatLaiMatKhauAsync("khach01@test.local", t => lienKet = $"http://x/dat-lai?token={t}");
        Assert.True(kq.ThanhCong);
        Assert.Single(Email.Thu);
        var token = lienKet!.Split("token=")[1];

        // CSDL chi luu ban bam cua token
        Db.ChangeTracker.Clear();
        Assert.NotEqual(token, (await Db.TaiKhoan.FirstAsync(t => t.TenDangNhap == "khach01")).TokenDatLaiMatKhau);

        Assert.True((await TaiKhoanService.DatLaiMatKhauAsync(token, "matkhaumoi", "matkhaumoi")).ThanhCong);
        Assert.False((await TaiKhoanService.DatLaiMatKhauAsync(token, "khac123", "khac123")).ThanhCong);   // dung lai

        var dn = await TaiKhoanService.DangNhapAsync(new() { TenDangNhap = "khach01", MatKhau = "matkhaumoi" }, false);
        Assert.True(dn.ThanhCong);
    }

    [Fact]
    public async Task QuenMatKhau_EmailKhongTonTai_KhongGuiThuNhungThongBaoGiongNhau()
    {
        var kq = await TaiKhoanService.YeuCauDatLaiMatKhauAsync("khong-co@test.local", t => t);
        Assert.True(kq.ThanhCong);
        Assert.Empty(Email.Thu);
    }

    [Fact]
    public async Task BaoCaoNhapXuatTon_KhopVoiPhieuNhapVaDonHang()
    {
        var hom = DateTime.Today;
        var maPhieu = (await KhoService.TaoPhieuNhapAsync(MaQuanLy, new()
        {
            MaNCC = MaNhaCungCap,
            Dong = new() { new() { MaSach = MaSachA, SoLuong = 7, DonGiaNhap = 50_000m } }
        })).DuLieu;
        await KhoService.HoanThanhPhieuNhapAsync(maPhieu);
        await DatDonAsync(HinhThucThanhToan.COD, (MaSachA, 3));
        var maHuy = await DatDonAsync(HinhThucThanhToan.COD, (MaSachA, 2));
        await DonHangService.KhachHuyDonAsync(maHuy, MaKhachHang, null);

        var dong = (await BaoCaoService.LayNhapXuatTonAsync(hom, hom)).Single(x => x.MaSach == MaSachA);

        Assert.Equal(10, dong.TonDauKy);
        Assert.Equal(7, dong.NhapTrongKy);
        Assert.Equal(3, dong.XuatTrongKy);   // don bi huy khong tinh
        Assert.Equal(14, dong.TonCuoiKy);
        Assert.Equal(LayTonKho(MaSachA), dong.TonCuoiKy);
    }

    [Fact]
    public async Task BaoCaoDoanhThu_ChiTinhDonHoanThanh_VaXuatExcelDocDuoc()
    {
        var ma = await DatDonAsync(HinhThucThanhToan.COD, (MaSachB, 2));   // 500.000, mien phi van chuyen
        foreach (var tt in new[] { TrangThaiDonHang.DaXacNhan, TrangThaiDonHang.DangGiao, TrangThaiDonHang.HoanThanh })
            await DonHangService.ChuyenTrangThaiAsync(ma, tt, "ql01", MaQuanLy);
        await DatDonAsync(HinhThucThanhToan.COD, (MaSachA, 1));   // chua hoan thanh

        var bc = await BaoCaoService.LayBaoCaoDoanhThuAsync(DateTime.Today.AddDays(-2), DateTime.Today);
        Assert.Equal(500_000m, bc.TongDoanhThu);
        Assert.Equal(1, bc.SoDonHoanThanh);
        Assert.Equal(3, bc.TheoNgay.Count);   // moi ngay trong ky mot dong

        var dem = await BaoCaoService.DemDonTheoTrangThaiAsync(DateTime.Today, DateTime.Today);
        Assert.Equal(1, dem.Single(x => x.TrangThai == TrangThaiDonHang.HoanThanh).SoDon);
        Assert.Equal(1, dem.Single(x => x.TrangThai == TrangThaiDonHang.ChoXacNhan).SoDon);

        var tep = await XuatExcelService.XuatDoanhThuAsync(DateTime.Today, DateTime.Today);
        using var wb = new XLWorkbook(new MemoryStream(tep));
        Assert.Equal(500_000m, wb.Worksheet("Doanh thu").Cell(4, 2).GetValue<decimal>());
        Assert.Equal("Sách B", wb.Worksheet("Sách bán chạy").Cell(5, 2).GetString());
    }
}
