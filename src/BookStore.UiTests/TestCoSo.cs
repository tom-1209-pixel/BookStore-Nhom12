using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace BookStore.UiTests;

/// <summary>Moi test dung mot browser context moi (cookie sach) va mot khach hang dang ky rieng
/// de gio hang cua cac test khong dung nhau.</summary>
[Collection("Web")]
public abstract class TestCoSo : IAsyncLifetime
{
    protected readonly WebFixture Web;
    protected IBrowserContext Ctx = default!;
    protected IPage Trang = default!;

    protected TestCoSo(WebFixture web) => Web = web;

    protected virtual ViewportSize? KichThuoc => new() { Width = 1280, Height = 900 };

    public async Task InitializeAsync()
    {
        Ctx = await Web.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = Web.DiaChi,
            ViewportSize = KichThuoc,
            Locale = "vi-VN"
        });
        Ctx.SetDefaultTimeout(15000);
        Trang = await Ctx.NewPageAsync();
    }

    public async Task DisposeAsync() => await Ctx.CloseAsync();

    protected static string TenMoi() => "u" + Guid.NewGuid().ToString("N")[..10];

    /// <summary>Dang ky tai khoan moi (tu dong dang nhap) va tra ve ten dang nhap.</summary>
    protected async Task<string> DangKyMoiAsync()
    {
        var ten = TenMoi();
        await Trang.GotoAsync("/dang-ky");
        await Trang.FillAsync("#dk-ho-ten", "Khach Thu " + ten);
        await Trang.FillAsync("#dk-ten", ten);
        await Trang.FillAsync("#dk-email", ten + "@example.com");
        await Trang.FillAsync("#dk-mk", "123456");
        await Trang.FillAsync("#dk-mk2", "123456");
        await Trang.ClickAsync("button:has-text('Tạo tài khoản')");
        await Trang.WaitForURLAsync(u => !u.Contains("/dang-ky"));
        return ten;
    }

    protected async Task DangNhapAsync(string ten, string mk = "123456")
    {
        await Trang.GotoAsync("/dang-nhap");
        await Trang.FillAsync("#ten-dang-nhap", ten);
        await Trang.FillAsync("#mat-khau", mk);
        await Trang.ClickAsync("main button:has-text('Đăng nhập')");
    }

    /// <summary>Mo trang sach theo tu khoa roi bam vao the sach dau tien.</summary>
    protected async Task MoChiTietSachAsync(string tuKhoa)
    {
        await Trang.GotoAsync("/sach?tuKhoa=" + Uri.EscapeDataString(tuKhoa));
        await Trang.Locator(".the-sach a.ten").First.ClickAsync();
        await Trang.WaitForURLAsync(new Regex(@"/sach/\d+$"));
    }

    protected async Task ThemVaoGioTuChiTietAsync(string tuKhoa)
    {
        await MoChiTietSachAsync(tuKhoa);
        await Trang.ClickAsync("button:has-text('Thêm vào giỏ hàng')");
        await Trang.WaitForURLAsync(u => u.Contains("thongBao="));
    }

    protected async Task<int> TonKhoTrenChiTietAsync(string tuKhoa)
    {
        await MoChiTietSachAsync(tuKhoa);
        var chu = await Trang.Locator(".badge.bg-success").First.InnerTextAsync();
        return int.Parse(Regex.Match(chu, @"\d+").Value);
    }

    protected async Task DienThongTinNhanHangAsync()
    {
        await Trang.FillAsync("#ho-ten", "Nguyen Van Thu");
        await Trang.FillAsync("#dien-thoai", "0901234567");
        await Trang.FillAsync("#dia-chi", "273 An Duong Vuong, Quan 5, TP.HCM");
    }

    /// <summary>So tien dang "203.000" khop ca dau cham lan dau phay tuy culture may chu.</summary>
    protected static Regex Tien(string so) => new(so.Replace(".", @"[.,]") + @"\s*đ");
}
