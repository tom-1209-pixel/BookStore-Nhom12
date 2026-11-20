using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BookStore.UiTests;

/// <summary>De bai yeu cau trang Web responsive: kiem tra khung nhin dien thoai.</summary>
public class ResponsiveTests : TestCoSo
{
    public ResponsiveTests(WebFixture web) : base(web) { }

    protected override ViewportSize? KichThuoc => new() { Width = 375, Height = 812 };

    [Theory]
    [InlineData("/")]
    [InlineData("/sach")]
    [InlineData("/dang-nhap")]
    public async Task KhungDienThoai_KhongCuonNgang(string duongDan)
    {
        await Trang.GotoAsync(duongDan);
        await Expect(Trang.Locator("main")).ToBeVisibleAsync();

        var rong = await Trang.EvaluateAsync<int[]>("() => [document.documentElement.scrollWidth, window.innerWidth]");
        Assert.True(rong[0] <= rong[1], $"Trang {duongDan} cuon ngang: noi dung {rong[0]}px > khung {rong[1]}px");
    }

    [Fact]
    public async Task KhungDienThoai_VanTimKiemVaMoChiTietDuoc()
    {
        await Trang.GotoAsync("/");
        await Trang.FillAsync("header input[name=tuKhoa]", "Doraemon");
        await Trang.ClickAsync("header button:has-text('Tìm')");

        await Expect(Trang.Locator(".the-sach")).ToHaveCountAsync(1);
        await Trang.Locator(".the-sach a.ten").ClickAsync();
        await Expect(Trang.Locator("h1")).ToContainTextAsync("Doraemon");
        await Expect(Trang.GetByRole(AriaRole.Button, new() { Name = "Thêm vào giỏ hàng" })).ToBeVisibleAsync();
    }
}
