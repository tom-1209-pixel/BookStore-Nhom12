using Microsoft.Playwright;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace BookStore.UiTests;

/// <summary>Khach vang lai: trang chu, tim kiem, xem chi tiet sach (khong can dang nhap).</summary>
public class TrangSachTests : TestCoSo
{
    public TrangSachTests(WebFixture web) : base(web) { }

    [Fact]
    public async Task TrangChu_HienThiThuongHieuVaSachMoi()
    {
        await Trang.GotoAsync("/");

        await Expect(Trang.Locator("header .thuong-hieu")).ToContainTextAsync("Nhà sách trực tuyến");
        await Expect(Trang.Locator(".the-sach").First).ToBeVisibleAsync();
        Assert.True(await Trang.Locator(".the-sach").CountAsync() >= 8);
        await Expect(Trang.Locator("header").GetByRole(AriaRole.Link, new() { Name = "Đăng nhập" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task TimKiem_TheoTuKhoa_ChiTraVeSachKhop()
    {
        await Trang.GotoAsync("/");
        await Trang.FillAsync("header input[name=tuKhoa]", "Nha gia kim");
        await Trang.ClickAsync("header button:has-text('Tìm')");

        await Expect(Trang).ToHaveURLAsync(new Regex("tuKhoa=Nha"));
        await Expect(Trang.Locator(".the-sach")).ToHaveCountAsync(1);
        await Expect(Trang.Locator(".the-sach a.ten")).ToHaveTextAsync("Nha gia kim");
    }

    [Fact]
    public async Task TimKiem_KhongCoKetQua_HienThongBaoRong()
    {
        await Trang.GotoAsync("/sach?tuKhoa=zzzkhongcosachnay");

        await Expect(Trang.GetByText("Không tìm thấy sách phù hợp")).ToBeVisibleAsync();
        await Expect(Trang.Locator(".the-sach")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ChiTietSach_HienThiGiaTonKhoVaNutThemVaoGio()
    {
        await MoChiTietSachAsync("Nha gia kim");

        await Expect(Trang.Locator("h1")).ToHaveTextAsync("Nha gia kim");
        await Expect(Trang.Locator(".gia").First).ToContainTextAsync("89");
        await Expect(Trang.Locator(".badge.bg-success")).ToContainTextAsync("Còn");
        await Expect(Trang.GetByRole(AriaRole.Button, new() { Name = "Thêm vào giỏ hàng" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task LocTheoGia_ChiGiuSachTrongKhoang()
    {
        await Trang.GotoAsync("/sach?giaTu=20000&giaDen=30000");

        // Seed chi co "Doraemon tap 1" gia 25.000 trong khoang nay
        await Expect(Trang.Locator(".the-sach")).ToHaveCountAsync(1);
        await Expect(Trang.Locator(".the-sach a.ten")).ToContainTextAsync("Doraemon");
    }
}
