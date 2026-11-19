using BookStore.Data;
using BookStore.Services;
using BookStore.Web.Components;
using BookStore.Web.Endpoints;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var chuoiKetNoi = builder.Configuration.GetConnectionString("BookStore")
    ?? throw new InvalidOperationException("Thiếu chuỗi kết nối 'BookStore' trong appsettings.json.");

// Tang nghiep vu dung chung voi BookStore.Api: trang Web goi truc tiep
// cac Services qua DI, khong di qua Web API.
builder.Services.ThemTangNghiepVu(chuoiKetNoi, builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Xac thuc bang cookie cho khach hang mua sach
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/dang-nhap";
        opt.LogoutPath = "/dang-xuat";
        opt.AccessDeniedPath = "/dang-nhap";
        opt.ExpireTimeSpan = TimeSpan.FromDays(7);
        opt.SlidingExpiration = true;
        opt.Cookie.Name = "BookStore.Auth";
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// BR04: tac vu nen dinh ky huy cac don VNPay qua han thanh toan
builder.Services.AddHostedService<BookStore.Web.HuyDonQuaHanNen>();

var app = builder.Build();

// Khoi tao CSDL va du lieu mau
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookStoreDbContext>();
    await SeedData.KhoiTaoAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/loi", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/khong-tim-thay", createScopeForStatusCodePages: true);

// Thu tu bat buoc: xac thuc truoc, roi moi kiem tra antiforgery. Neu dao nguoc,
// token antiforgery duoc sinh cho nguoi dung da dang nhap se bi doi chieu voi
// nguoi dung an danh va moi form deu tra ve loi 400.
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

// Cac endpoint xu ly form: dang nhap, dang ky, dang xuat, gio hang, dat hang
app.MapTaiKhoanEndpoints();
app.MapGioHangEndpoints();
app.MapThanhToanEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
