using System.Diagnostics;
using System.Net.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;

namespace BookStore.UiTests;

/// <summary>
/// Dung chung cho moi test giao dien: tao CSDL rieng, chay trang Web bang process con
/// tren cong 5199 va mo mot trinh duyet Edge. Khong dung vao CSDL demo.
/// </summary>
public sealed class WebFixture : IAsyncLifetime
{
    public const string TenCsdl = "QuanLyBanSachTrucTuyen_UiTest";
    public const int Cong = 5199;
    public string DiaChi => $"http://localhost:{Cong}";

    private const string ChuoiMaster = "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True";
    private static readonly string ChuoiTest = $"Server=localhost;Database={TenCsdl};Trusted_Connection=True;TrustServerCertificate=True";

    private Process? _web;
    private readonly List<string> _nhatKy = new();

    public IPlaywright Playwright { get; private set; } = default!;
    public IBrowser Browser { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await XoaCsdlAsync();

        var thuMucGoc = TimThuMucGoc();
        var thuMucWeb = Path.Combine(thuMucGoc, "src", "BookStore.Web");
        var dll = new[] { "Debug", "Release" }
            .Select(c => Path.Combine(thuMucWeb, "bin", c, "net10.0", "BookStore.Web.dll"))
            .Where(File.Exists)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new FileNotFoundException("Chua build BookStore.Web. Chay 'dotnet build' truoc.");

        var psi = new ProcessStartInfo("dotnet", $"\"{dll}\"")
        {
            WorkingDirectory = thuMucWeb,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["ASPNETCORE_URLS"] = DiaChi;
        psi.Environment["ConnectionStrings__BookStore"] = ChuoiTest;

        _web = Process.Start(psi)!;
        _web.OutputDataReceived += (_, e) => { if (e.Data != null) lock (_nhatKy) _nhatKy.Add(e.Data); };
        _web.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (_nhatKy) _nhatKy.Add(e.Data); };
        _web.BeginOutputReadLine();
        _web.BeginErrorReadLine();

        await ChoWebSanSangAsync();

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        // Dung Microsoft Edge da cai san tren Windows nen khong phai tai Chromium.
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (Browser != null) await Browser.CloseAsync();
        Playwright?.Dispose();
        if (_web is { HasExited: false })
        {
            _web.Kill(entireProcessTree: true);
            await _web.WaitForExitAsync();
        }
        await XoaCsdlAsync();
    }

    private async Task ChoWebSanSangAsync()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        var han = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < han)
        {
            if (_web!.HasExited)
                throw new InvalidOperationException("Web thoat som:\n" + string.Join("\n", _nhatKy));
            try
            {
                var r = await http.GetAsync(DiaChi + "/");
                if (r.IsSuccessStatusCode) return;
            }
            catch { /* chua len */ }
            await Task.Delay(500);
        }
        throw new TimeoutException("Web khong san sang sau 90 giay:\n" + string.Join("\n", _nhatKy));
    }

    private static async Task XoaCsdlAsync()
    {
        await using var kn = new SqlConnection(ChuoiMaster);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = $@"IF DB_ID('{TenCsdl}') IS NOT NULL
BEGIN
    ALTER DATABASE [{TenCsdl}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{TenCsdl}];
END";
        await lenh.ExecuteNonQueryAsync();
    }

    private static string TimThuMucGoc()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d != null && !File.Exists(Path.Combine(d.FullName, "BookStore.slnx"))) d = d.Parent;
        return d?.FullName ?? throw new DirectoryNotFoundException("Khong tim thay BookStore.slnx");
    }
}

[CollectionDefinition("Web")]
public class WebCollection : ICollectionFixture<WebFixture> { }
