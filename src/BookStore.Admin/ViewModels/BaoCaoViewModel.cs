using System.Collections.ObjectModel;
using BookStore.Admin.Helpers;
using BookStore.Admin.Models;
using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

/// <summary>
/// Man hinh D08 - Bao cao thong ke: doanh thu theo ngay (BR09), sach ban chay,
/// don theo trang thai, nhap - xuat - ton va xuat bao cao ra Excel.
/// </summary>
public partial class BaoCaoViewModel : ObservableObject
{
    private const double DoRongToiDa = 260;

    private readonly ApiClient _api;

    public BaoCaoViewModel(ApiClient api)
    {
        _api = api;
        var homNay = DateTime.Today;
        _tuNgay = new DateTimeOffset(new DateTime(homNay.Year, homNay.Month, 1));
        _denNgay = new DateTimeOffset(homNay);
    }

    // CalendarDatePicker lam viec voi DateTimeOffset?
    [ObservableProperty] private DateTimeOffset? _tuNgay;
    [ObservableProperty] private DateTimeOffset? _denNgay;
    [ObservableProperty] private BaoCaoDoanhThu _doanhThu = new();
    [ObservableProperty] private bool _dangTai;
    [ObservableProperty] private string _thongBao = string.Empty;

    public ObservableCollection<DoanhThuNgay> TheoNgay { get; } = new();
    public ObservableCollection<SachBanChay> TopSach { get; } = new();
    public ObservableCollection<DonTheoTrangThai> DonTheoTrangThai { get; } = new();
    public ObservableCollection<NhapXuatTon> NhapXuatTon { get; } = new();

    public bool CoThongBao => !string.IsNullOrEmpty(ThongBao);
    partial void OnThongBaoChanged(string value) => OnPropertyChanged(nameof(CoThongBao));

    private DateTime Tu => (TuNgay ?? DateTimeOffset.Now).Date;
    private DateTime Den => (DenNgay ?? DateTimeOffset.Now).Date;

    [RelayCommand]
    private Task BayNgayQuaAsync()
    {
        TuNgay = new DateTimeOffset(DateTime.Today.AddDays(-6));
        DenNgay = new DateTimeOffset(DateTime.Today);
        return XemAsync();
    }

    [RelayCommand]
    private Task ThangNayAsync()
    {
        TuNgay = new DateTimeOffset(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
        DenNgay = new DateTimeOffset(DateTime.Today);
        return XemAsync();
    }

    [RelayCommand]
    public async Task XemAsync()
    {
        if (Tu > Den) { ThongBao = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc."; return; }

        DangTai = true;
        ThongBao = string.Empty;

        var dt = await _api.LayBaoCaoDoanhThuAsync(Tu, Den);
        var dem = await _api.LayDonTheoTrangThaiAsync(Tu, Den);
        var nxt = await _api.LayNhapXuatTonAsync(Tu, Den);

        TheoNgay.Clear();
        TopSach.Clear();
        if (dt.ThanhCong && dt.DuLieu is not null)
        {
            DoanhThu = dt.DuLieu;
            var max = dt.DuLieu.TheoNgay.Count == 0 ? 0 : dt.DuLieu.TheoNgay.Max(x => x.DoanhThu);
            foreach (var n in dt.DuLieu.TheoNgay)
            {
                n.DoRongThanh = max <= 0 ? 0 : (double)(n.DoanhThu / max) * DoRongToiDa;
                TheoNgay.Add(n);
            }
            foreach (var s in dt.DuLieu.TopSachBanChay) TopSach.Add(s);
        }
        else ThongBao = dt.ThongBao;

        DonTheoTrangThai.Clear();
        if (dem.ThanhCong && dem.DuLieu is not null)
            foreach (var d in dem.DuLieu) DonTheoTrangThai.Add(d);

        NhapXuatTon.Clear();
        if (nxt.ThanhCong && nxt.DuLieu is not null)
            foreach (var x in nxt.DuLieu) NhapXuatTon.Add(x);

        DangTai = false;
    }

    [RelayCommand]
    private Task XuatExcelDoanhThuAsync() => XuatAsync("doanhthu", "BaoCaoDoanhThu");

    [RelayCommand]
    private Task XuatExcelNhapXuatTonAsync() => XuatAsync("nhapxuatton", "BaoCaoNhapXuatTon");

    private async Task XuatAsync(string loai, string tenTep)
    {
        if (Tu > Den) { ThongBao = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc."; return; }

        DangTai = true;
        var kq = await _api.TaiExcelAsync(loai, Tu, Den);
        DangTai = false;

        if (!kq.ThanhCong || kq.DuLieu is null) { ThongBao = kq.ThongBao; return; }

        try
        {
            var duongDan = TienIch.LuuTep($"{tenTep}_{Tu:yyyyMMdd}_{Den:yyyyMMdd}.xlsx", kq.DuLieu);
            ThongBao = $"Đã lưu báo cáo tại {duongDan}";
            TienIch.MoTep(duongDan);
        }
        catch (IOException ex)
        {
            ThongBao = "Không lưu được tập tin (có thể tập tin đang mở trong Excel): " + ex.Message;
        }
    }
}
