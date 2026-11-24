using Microsoft.Playwright;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace BookStore.UiTests;

/// <summary>Gio hang va dat hang: BR01 (dang nhap de them vao gio), tinh tien, tru ton kho (BR03), VNPay.</summary>
public class DatHangTests : TestCoSo
{
    public DatHangTests(WebFixture web) : base(web) { }

    [Fact]
    public async Task ThemVaoGio_KhiChuaDangNhap_ChuyenSangDangNhap_BR01()
    {
        await MoChiTietSachAsync("Nha gia kim");
        await Trang.ClickAsync("button:has-text('Thêm vào giỏ hàng')");

        await Expect(Trang).ToHaveURLAsync(new Regex("/dang-nhap"));
        await Expect(Trang.Locator(".alert-danger")).ToContainTextAsync("đăng nhập");
    }

    [Fact]
    public async Task GioHang_DoiSoLuong_TongTienCapNhatDung()
    {
        await DangKyMoiAsync();
        await ThemVaoGioTuChiTietAsync("Nha gia kim"); // 89.000 d

        await Trang.GotoAsync("/gio-hang");
        // 89.000 + phi van chuyen 25.000 (duoi 300.000 d)
        await Expect(Trang.Locator(".gia")).ToContainTextAsync(Tien("114.000"));

        await TangSoLuongLenHaiAsync();

        // 2 x 89.000 + 25.000
        await Expect(Trang.Locator(".gia")).ToContainTextAsync(Tien("203.000"));
    }

    [Fact]
    public async Task GioHang_TrenNguongMienPhi_MienPhiVanChuyen()
    {
        await DangKyMoiAsync();
        await ThemVaoGioTuChiTietAsync("Clean Code"); // 245.000
        await Trang.GotoAsync("/gio-hang");
        await TangSoLuongLenHaiAsync(); // 490.000 >= 300.000

        await Expect(Trang.GetByText("Miễn phí").First).ToBeVisibleAsync();
        await Expect(Trang.Locator(".gia")).ToContainTextAsync(Tien("490.000"));
    }

    [Fact]
    public async Task DatHang_COD_ThanhCongVaTruTonKho_BR03()
    {
        await DangKyMoiAsync();
        var tonTruoc = await TonKhoTrenChiTietAsync("Doraemon");
        await Trang.ClickAsync("button:has-text('Thêm vào giỏ hàng')");
        await Trang.WaitForURLAsync(u => u.Contains("thongBao="));

        await Trang.GotoAsync("/thanh-toan");
        await DienThongTinNhanHangAsync();
        await Trang.ClickAsync("button:has-text('Đặt hàng')");

        await Expect(Trang).ToHaveURLAsync(new Regex(@"/ket-qua-dat-hang/\d+"));
        await Expect(Trang.Locator("h1")).ToContainTextAsync("Đặt hàng thành công");
        await Expect(Trang.Locator(".badge.bg-primary")).ToContainTextAsync("Chờ xác nhận");

        var tonSau = await TonKhoTrenChiTietAsync("Doraemon");
        Assert.Equal(tonTruoc - 1, tonSau);
    }

    [Fact]
    public async Task DatHang_VNPayGiaLap_TheATMVaOtp_DonChuyenSangChoXacNhan()
    {
        await DangKyMoiAsync();
        await ThemVaoGioTuChiTietAsync("Dac nhan tam"); // 108.000 + 25.000

        await Trang.GotoAsync("/thanh-toan");
        await DienThongTinNhanHangAsync();
        await Trang.CheckAsync("#tt-vnpay");
        await Trang.ClickAsync("button:has-text('Đặt hàng')");

        // Chua cau hinh khoa VNPay nen duoc chuyen sang cong gia lap
        await Expect(Trang).ToHaveURLAsync(new Regex("/vnpay-gia-lap"));
        await Expect(Trang.Locator("h1")).ToContainTextAsync("Chọn phương thức thanh toán");
        await Expect(Trang.Locator(".vnp-dong .vnp-tien")).ToContainTextAsync(new Regex(@"133[.,]000\s*VND"));

        await Trang.ClickAsync("text=Thẻ nội địa và tài khoản ngân hàng");
        await Trang.FillAsync("#so-the", "9704198526191432198");
        await Trang.FillAsync("#ten-chu-the", "NGUYEN VAN A");
        await Trang.FillAsync("#ngay-phat-hanh", "07/15");
        await Trang.ClickAsync("button:has-text('Tiếp tục')");

        // Sai OTP thi bao loi va cho nhap lai
        await Expect(Trang.Locator("h1")).ToContainTextAsync("Xác thực OTP");
        await Trang.FillAsync("#ma-otp", "000000");
        await Trang.ClickAsync("button:has-text('Xác nhận')");
        await Expect(Trang.Locator(".alert-danger")).ToContainTextAsync("Mã OTP không đúng");

        // Nhan Enter trong o OTP phai gui ma, khong duoc kich hoat nut Huy giao dich
        await Trang.FillAsync("#ma-otp", "123456");
        await Trang.PressAsync("#ma-otp", "Enter");

        await Expect(Trang).ToHaveURLAsync(new Regex(@"/ket-qua-dat-hang/\d+"));
        await Expect(Trang.Locator(".badge.bg-primary")).ToContainTextAsync("Chờ xác nhận");
    }

    [Fact]
    public async Task DatHang_VNPayGiaLap_SaiThongTinThe_BaoLoiVaKhongThanhToan()
    {
        await DangKyMoiAsync();
        await ThemVaoGioTuChiTietAsync("Nha gia kim");

        await Trang.GotoAsync("/thanh-toan");
        await DienThongTinNhanHangAsync();
        await Trang.CheckAsync("#tt-vnpay");
        await Trang.ClickAsync("button:has-text('Đặt hàng')");
        await Expect(Trang).ToHaveURLAsync(new Regex("/vnpay-gia-lap"));

        await Trang.ClickAsync("text=Thẻ nội địa và tài khoản ngân hàng");
        await Trang.FillAsync("#so-the", "9704000000000000");
        await Trang.FillAsync("#ten-chu-the", "NGUYEN VAN B");
        await Trang.FillAsync("#ngay-phat-hanh", "01/20");
        await Trang.ClickAsync("button:has-text('Tiếp tục')");

        await Expect(Trang.Locator(".alert-danger")).ToContainTextAsync("Thông tin thẻ không chính xác");
        await Expect(Trang.Locator("#so-the")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DatHang_VNPayGiaLap_QuetMaQR_HienMaVaXacNhan()
    {
        await DangKyMoiAsync();
        await ThemVaoGioTuChiTietAsync("Mat biec");

        await Trang.GotoAsync("/thanh-toan");
        await DienThongTinNhanHangAsync();
        await Trang.CheckAsync("#tt-vnpay");
        await Trang.ClickAsync("button:has-text('Đặt hàng')");
        await Expect(Trang).ToHaveURLAsync(new Regex("/vnpay-gia-lap"));

        await Trang.ClickAsync("text=Ứng dụng thanh toán hỗ trợ VNPAY-QR");
        await Expect(Trang.Locator(".vnp-qr img")).ToBeVisibleAsync();
        await Trang.ClickAsync("button:has-text('Tôi đã thanh toán')");

        await Expect(Trang).ToHaveURLAsync(new Regex(@"/ket-qua-dat-hang/\d+"));
        await Expect(Trang.Locator(".badge.bg-primary")).ToContainTextAsync("Chờ xác nhận");
    }

    [Fact]
    public async Task DatHang_VNPayGiaLap_HuyGiaoDich_DonVanChoThanhToan()
    {
        await DangKyMoiAsync();
        await ThemVaoGioTuChiTietAsync("Toi tai gioi");

        await Trang.GotoAsync("/thanh-toan");
        await DienThongTinNhanHangAsync();
        await Trang.CheckAsync("#tt-vnpay");
        await Trang.ClickAsync("button:has-text('Đặt hàng')");
        await Expect(Trang).ToHaveURLAsync(new Regex("/vnpay-gia-lap"));

        await Trang.ClickAsync("button:has-text('Hủy thanh toán')");

        await Expect(Trang).ToHaveURLAsync(new Regex(@"/ket-qua-dat-hang/\d+"));
        await Expect(Trang.Locator("h1")).ToContainTextAsync("chờ thanh toán");
        await Expect(Trang.GetByRole(AriaRole.Link, new() { Name = "Thanh toán ngay qua VNPay" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ThanhToan_GioTrong_KhongChoDatHang()
    {
        await DangKyMoiAsync();
        await Trang.GotoAsync("/thanh-toan");

        await Expect(Trang.GetByText("Giỏ hàng đang trống nên chưa thể thanh toán")).ToBeVisibleAsync();
        await Expect(Trang.Locator("button:has-text('Đặt hàng')")).ToHaveCountAsync(0);
    }

    /// <summary>
    /// Trang gio hang chay Interactive Server: truoc khi circuit Blazor ket noi, bam "+" khong co tac dung.
    /// Bam lai cho den khi o so luong doi thanh 2.
    /// </summary>
    private async Task TangSoLuongLenHaiAsync()
    {
        var tang = Trang.Locator("button:has-text('+')");
        var soLuong = Trang.Locator("td input.form-control").First;
        for (var lan = 0; lan < 15; lan++)
        {
            await tang.ClickAsync();
            try
            {
                await Expect(soLuong).ToHaveValueAsync("2", new() { Timeout = 1000 });
                return;
            }
            catch (PlaywrightException) { /* chua ket noi, thu lai */ }
        }
        throw new TimeoutException("Khong tang duoc so luong trong gio hang.");
    }
}
