using BookStore.Services.Interfaces;

namespace BookStore.Web;

/// <summary>
/// Tac vu nen chay moi phut: huy cac don VNPay da qua 30 phut ma chua thanh toan va
/// hoan lai ton kho (BR05). Truoc day phai goi thu cong qua Web API.
/// </summary>
public class HuyDonQuaHanNen : BackgroundService
{
    private static readonly TimeSpan ChuKy = TimeSpan.FromMinutes(1);

    private readonly IServiceProvider _sp;
    private readonly ILogger<HuyDonQuaHanNen> _log;

    public HuyDonQuaHanNen(IServiceProvider sp, ILogger<HuyDonQuaHanNen> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var dongHo = new PeriodicTimer(ChuKy);
        do
        {
            try
            {
                using var scope = _sp.CreateScope();
                var donHang = scope.ServiceProvider.GetRequiredService<IDonHangService>();
                var soDon = await donHang.HuyDonQuaHanAsync();
                if (soDon > 0) _log.LogInformation("Tác vụ nền đã hủy {SoDon} đơn quá hạn thanh toán.", soDon);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Loi tam thoi (mat ket noi CSDL...) chi ghi log, lan sau chay lai
                _log.LogError(ex, "Lỗi khi tự động hủy đơn quá hạn thanh toán.");
            }
        } while (await dongHo.WaitForNextTickAsync(stoppingToken));
    }
}
