using Microsoft.Playwright;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace BookStore.UiTests;

/// <summary>Dang ky, dang nhap, truy cap trang can quyen, quen mat khau.</summary>
public class TaiKhoanTests : TestCoSo
{
    public TaiKhoanTests(WebFixture web) : base(web) { }

    [Fact]
    public async Task DangNhap_SaiMatKhau_BaoLoiVaOLaiTrangDangNhap()
    {
        await DangNhapAsync("khachhang", "matkhausai");

        await Expect(Trang).ToHaveURLAsync(new Regex("/dang-nhap"));
        await Expect(Trang.Locator(".alert-danger")).ToBeVisibleAsync();
        await Expect(Trang.Locator("form[action='/tai-khoan/dang-xuat']")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task DangNhap_DungTaiKhoanMau_VaoDuocVaDangXuatDuoc()
    {
        await DangNhapAsync("khachhang");

        await Expect(Trang.Locator("form[action='/tai-khoan/dang-xuat']")).ToHaveCountAsync(1);
        await Expect(Trang.Locator("header").GetByRole(AriaRole.Link, new() { Name = "Đăng ký" })).ToHaveCountAsync(0);

        // Dang xuat: mo menu nguoi dung roi bam nut
        await Trang.ClickAsync("header .dropdown-toggle");
        await Trang.ClickAsync("button:has-text('Đăng xuất')");
        await Expect(Trang.Locator("header").GetByRole(AriaRole.Link, new() { Name = "Đăng nhập" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DangNhap_TaiKhoanQuanLy_KhongVaoDuocTrangWeb()
    {
        // Tai khoan Quan ly chi dung duoc tren ung dung quan tri Desktop
        await DangNhapAsync("quanly");

        await Expect(Trang).ToHaveURLAsync(new Regex("/dang-nhap"));
        await Expect(Trang.Locator(".alert-danger")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DangKy_TenDangNhapDaTonTai_BaoLoi()
    {
        await Trang.GotoAsync("/dang-ky");
        await Trang.FillAsync("#dk-ho-ten", "Nguoi Trung Ten");
        await Trang.FillAsync("#dk-ten", "khachhang");
        await Trang.FillAsync("#dk-email", TenMoi() + "@example.com");
        await Trang.FillAsync("#dk-mk", "123456");
        await Trang.FillAsync("#dk-mk2", "123456");
        await Trang.ClickAsync("button:has-text('Tạo tài khoản')");

        await Expect(Trang).ToHaveURLAsync(new Regex("/dang-ky"));
        await Expect(Trang.Locator(".alert-danger")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DangKy_ThongTinHopLe_TuDongDangNhap()
    {
        await DangKyMoiAsync();

        await Expect(Trang.Locator(".alert-success")).ToContainTextAsync("Đăng ký thành công");
        await Expect(Trang.Locator("form[action='/tai-khoan/dang-xuat']")).ToHaveCountAsync(1);
    }

    [Theory]
    [InlineData("/tai-khoan/don-hang")]
    [InlineData("/thanh-toan")]
    public async Task TrangCanDangNhap_KhiChuaDangNhap_ChuyenSangDangNhap(string duongDan)
    {
        await Trang.GotoAsync(duongDan);

        await Expect(Trang).ToHaveURLAsync(new Regex("/dang-nhap"));
    }

    [Fact]
    public async Task QuenMatKhau_ThongBaoGiongNhauDuEmailCoHayKhong()
    {
        // Khong lo viec email co ton tai trong he thong hay khong
        async Task<string> GuiAsync(string email)
        {
            await Trang.GotoAsync("/quen-mat-khau");
            await Trang.FillAsync("#email", email);
            await Trang.ClickAsync("button:has-text('Gửi liên kết')");
            await Trang.WaitForURLAsync(u => u.Contains("thongBao=") || u.Contains("loi="));
            return (await Trang.Locator(".alert").First.InnerTextAsync()).Trim();
        }

        var emailCo = TenMoi();
        await DangKyMoiAsync(); // dam bao co it nhat mot tai khoan
        var thongBaoKhongCo = await GuiAsync("khongco_" + emailCo + "@example.com");
        var thongBaoCo = await GuiAsync("khachhang@example.com");

        Assert.Equal(thongBaoCo, thongBaoKhongCo);
        await Expect(Trang.Locator(".alert-danger")).ToHaveCountAsync(0);
    }
}
