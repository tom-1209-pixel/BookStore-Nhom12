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
    public async Task TaiKhoanCuaToi_CapNhatHoTenVaThemDiaChi_HienTrenTrangVaTrangThanhToan()
    {
        await DangKyMoiAsync();
        await Trang.GotoAsync("/tai-khoan");

        await Trang.FillAsync("#ho-ten", "Tran Thi Kiem Thu");
        await Trang.FillAsync("#sdt", "0987654321");
        await Trang.ClickAsync("button:has-text('Lưu thông tin')");
        await Expect(Trang.Locator(".alert-success")).ToContainTextAsync("Đã cập nhật thông tin cá nhân");
        await Expect(Trang.Locator("#ho-ten")).ToHaveValueAsync("Tran Thi Kiem Thu");

        await Trang.FillAsync("#dc-ten", "Nguoi Nhan UI");
        await Trang.FillAsync("#dc-sdt", "0911222333");
        await Trang.FillAsync("#dc-chi-tiet", "12 Le Loi");
        await Trang.FillAsync("#dc-phuong", "Ben Nghe");
        await Trang.FillAsync("#dc-quan", "Quan 1");
        await Trang.FillAsync("#dc-tinh", "TP Ho Chi Minh");
        await Trang.ClickAsync("button:has-text('Thêm địa chỉ')");

        await Expect(Trang.Locator(".alert-success")).ToContainTextAsync("Đã thêm địa chỉ");
        await Expect(Trang.Locator(".dia-chi")).ToContainTextAsync("Nguoi Nhan UI");
        await Expect(Trang.Locator(".dia-chi .badge")).ToContainTextAsync("Mặc định");

        // Dia chi mac dinh duoc dien san o trang thanh toan
        await ThemVaoGioTuChiTietAsync("Doraemon");
        await Trang.GotoAsync("/thanh-toan");
        await Expect(Trang.Locator("input[name='hoTenNguoiNhan']")).ToHaveValueAsync("Nguoi Nhan UI");
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
