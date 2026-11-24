using BookStore.Admin.Services;
using BookStore.Admin.ViewModels;
using BookStore.Admin.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace BookStore.Admin;

/// <summary>
/// Diem khoi dong ung dung quan tri. Ung dung dung Generic Host de co san
/// Dependency Injection, cau hinh va ghi log giong nhu phia may chu.
/// </summary>
public partial class App : Application
{
    /// <summary>Dia chi Web API. Ung dung Desktop khong biet chuoi ket noi CSDL.</summary>
    public const string DiaChiApi = "http://localhost:5171/";

    public static IHost Host { get; private set; } = default!;

    public static T LayDichVu<T>() where T : notnull => Host.Services.GetRequiredService<T>();

    private Window? _cuaSo;

    /// <summary>Cua so dang mo, can cho hop thoai chon tep (ung dung unpackaged phai gan HWND).</summary>
    public static Window? CuaSoHienTai => (Current as App)?._cuaSo;

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureLogging(log =>
            {
                log.ClearProviders();
                log.AddDebug();
                log.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<PhienDangNhap>();

                services.AddHttpClient<ApiClient>(c =>
                {
                    c.BaseAddress = new Uri(DiaChiApi);
                    c.Timeout = TimeSpan.FromSeconds(30);
                });

                services.AddTransient<DangNhapViewModel>();
                services.AddSingleton<ThongKeViewModel>();
                services.AddSingleton<DonHangViewModel>();
                services.AddSingleton<SachViewModel>();
                services.AddSingleton<DanhMucViewModel>();
                services.AddSingleton<KhoViewModel>();
                services.AddSingleton<KhachHangViewModel>();
                services.AddSingleton<BaoCaoViewModel>();
            })
            .Build();

        UnhandledException += (_, e) =>
        {
            Host.Services.GetRequiredService<ILogger<App>>()
                .LogError(e.Exception, "Lỗi chưa xử lý trong ứng dụng quản trị.");
            e.Handled = true;
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _cuaSo = new DangNhapWindow();
        _cuaSo.Activate();
    }

    /// <summary>Dong cua so dang nhap va mo cua so chinh sau khi xac thuc thanh cong.</summary>
    public void MoCuaSoChinh()
    {
        var cuaSoCu = _cuaSo;
        _cuaSo = new MainWindow();
        _cuaSo.Activate();
        cuaSoCu?.Close();
    }
}
