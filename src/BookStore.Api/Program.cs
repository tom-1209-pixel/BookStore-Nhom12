using System.Text;
using BookStore.Api.Middlewares;
using BookStore.Api.Services;
using BookStore.Data;
using BookStore.Services;
using BookStore.Services.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var chuoiKetNoi = builder.Configuration.GetConnectionString("BookStore")
    ?? throw new InvalidOperationException("Thiếu chuỗi kết nối 'BookStore' trong appsettings.json.");

// Tang nghiep vu dung chung voi BookStore.Web
builder.Services.ThemTangNghiepVu(chuoiKetNoi, builder.Configuration);

// Sinh va kiem tra token JWT cho ung dung Desktop
builder.Services.Configure<CauHinhJwt>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<CauHinhJwt>() ?? new CauHinhJwt();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.KhoaBiMat)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookStore API",
        Version = "v1",
        Description = "Web API cho ứng dụng quản trị Desktop của hệ thống quản lý và bán sách trực tuyến."
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token JWT nhận được từ POST /api/xacthuc/dangnhap.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

// Khoi tao CSDL va du lieu mau khi chay o moi truong phat trien
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BookStoreDbContext>();
    await SeedData.KhoiTaoAsync(db);
}

// Bat moi ngoai le chua xu ly, tra ve ProblemDetails thong nhat (thiet ke thich ung)
app.UseMiddleware<XuLyLoiMiddleware>();

// Phuc vu anh bia (ung dung quan tri xem truoc anh qua dia chi nay)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(AnhBia.LayThuMuc(app.Configuration, app.Environment.ContentRootPath)),
    RequestPath = AnhBia.DuongDanUrl
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore API v1");
    c.DocumentTitle = "BookStore API";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
