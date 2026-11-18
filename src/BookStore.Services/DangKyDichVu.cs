using BookStore.Data;
using BookStore.Services.Implementations;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Services;

/// <summary>
/// Dang ky tat ca dich vu cua tang nghiep vu vao container DI.
/// Ca BookStore.Web va BookStore.Api goi ham nay nen hai ung dung dung
/// cung mot bo nghiep vu, tranh viet lap logic.
/// </summary>
public static class DangKyDichVu
{
    public static IServiceCollection ThemTangNghiepVu(this IServiceCollection services, string chuoiKetNoi,
        IConfiguration? cauHinh = null)
    {
        // DbContext va cac Service dung vong doi Transient thay vi Scoped.
        // Ly do: trong Blazor Server, nhieu component tren cung mot trang co the
        // chay OnInitializedAsync dong thoi. Neu dung chung mot DbContext thi se
        // gap loi "A second operation was started on this context". Moi diem tiem
        // phu thuoc nhan rieng mot DbContext nen khong con tranh chap.
        services.AddDbContext<BookStoreDbContext>(
            opt => opt.UseSqlServer(chuoiKetNoi, sql => sql.EnableRetryOnFailure(3)),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddTransient<ITaiKhoanService, TaiKhoanService>();
        services.AddTransient<ISachService, SachService>();
        services.AddTransient<IGioHangService, GioHangService>();
        services.AddTransient<IDonHangService, DonHangService>();
        services.AddTransient<IBaoCaoService, BaoCaoService>();
        services.AddTransient<IDanhMucService, DanhMucService>();
        services.AddTransient<IKhoService, KhoService>();
        services.AddTransient<IKhachHangService, KhachHangService>();
        services.AddTransient<IXuatExcelService, XuatExcelService>();
        services.AddTransient<IThanhToanService, ThanhToanService>();
        services.AddTransient<IEmailService, EmailService>();

        // Cau hinh email va VNPay (thong tin bi mat dat trong user-secrets hoac bien moi truong)
        if (cauHinh is not null)
        {
            services.Configure<CauHinhEmail>(cauHinh.GetSection("Email"));
            services.Configure<CauHinhVnPay>(cauHinh.GetSection("VnPay"));
        }
        else
        {
            services.AddOptions<CauHinhEmail>();
            services.AddOptions<CauHinhVnPay>();
        }

        return services;
    }
}
