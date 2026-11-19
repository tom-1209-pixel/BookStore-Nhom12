using System.Collections.ObjectModel;
using BookStore.Admin.Models;
using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

/// <summary>Man hinh D07 - Quan ly khach hang: tra cuu, xem thong tin, khoa / mo khoa tai khoan.</summary>
public partial class KhachHangViewModel : ObservableObject
{
    private readonly ApiClient _api;

    public record MucBoLoc(string Ten, byte? Ma)
    {
        public override string ToString() => Ten;
    }

    public List<MucBoLoc> CacBoLoc { get; } = new()
    {
        new("Tất cả trạng thái", null),
        new("Hoạt động", 1),
        new("Bị khóa", 0)
    };

    public KhachHangViewModel(ApiClient api)
    {
        _api = api;
        _boLoc = CacBoLoc[0];
    }

    public ObservableCollection<KhachHang> DanhSach { get; } = new();

    [ObservableProperty] private MucBoLoc _boLoc;
    [ObservableProperty] private string _tuKhoa = string.Empty;
    [ObservableProperty] private KhachHang? _khachDangChon;
    [ObservableProperty] private KhachHang? _chiTiet;
    [ObservableProperty] private string _lyDo = string.Empty;
    [ObservableProperty] private bool _dangTai;
    [ObservableProperty] private string _thongBao = string.Empty;

    public bool CoThongBao => !string.IsNullOrEmpty(ThongBao);
    partial void OnThongBaoChanged(string value) => OnPropertyChanged(nameof(CoThongBao));

    public bool CoChiTiet => ChiTiet is not null;
    public bool ChoPhepKhoa => ChiTiet is { BiKhoa: false };
    public bool ChoPhepMoKhoa => ChiTiet is { BiKhoa: true };

    partial void OnChiTietChanged(KhachHang? value)
    {
        OnPropertyChanged(nameof(CoChiTiet));
        OnPropertyChanged(nameof(ChoPhepKhoa));
        OnPropertyChanged(nameof(ChoPhepMoKhoa));
    }

    partial void OnBoLocChanged(MucBoLoc value) => _ = NapAsync();

    partial void OnKhachDangChonChanged(KhachHang? value)
    {
        if (value is not null) _ = NapChiTietAsync(value.MaKhachHang);
    }

    [RelayCommand]
    public async Task NapAsync()
    {
        DangTai = true;
        var kq = await _api.TimKhachHangAsync(TuKhoa, BoLoc?.Ma);
        DanhSach.Clear();
        if (kq.ThanhCong && kq.DuLieu is not null)
            foreach (var k in kq.DuLieu) DanhSach.Add(k);
        else ThongBao = kq.ThongBao;
        DangTai = false;
    }

    private async Task NapChiTietAsync(int ma)
    {
        var kq = await _api.LayKhachHangAsync(ma);
        if (kq.ThanhCong) ChiTiet = kq.DuLieu;
        else ThongBao = kq.ThongBao;
    }

    [RelayCommand]
    private Task KhoaAsync() => DoiTrangThaiAsync(true);

    [RelayCommand]
    private Task MoKhoaAsync() => DoiTrangThaiAsync(false);

    private async Task DoiTrangThaiAsync(bool khoa)
    {
        if (ChiTiet is null) return;
        var ma = ChiTiet.MaKhachHang;

        DangTai = true;
        var kq = await _api.DoiTrangThaiKhachHangAsync(ma, khoa, string.IsNullOrWhiteSpace(LyDo) ? null : LyDo.Trim());
        DangTai = false;

        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã cập nhật.") : kq.ThongBao;
        if (!kq.ThanhCong) return;

        LyDo = string.Empty;
        await NapAsync();
        await NapChiTietAsync(ma);
    }
}
