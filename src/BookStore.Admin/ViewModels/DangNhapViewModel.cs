using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

public partial class DangNhapViewModel : ObservableObject
{
    private readonly ApiClient _api;

    public DangNhapViewModel(ApiClient api) => _api = api;

    [ObservableProperty] private string _tenDangNhap = string.Empty;
    [ObservableProperty] private string _matKhau = string.Empty;
    [ObservableProperty] private string _thongBaoLoi = string.Empty;
    [ObservableProperty] private bool _dangXuLy;

    public bool CoLoi => !string.IsNullOrEmpty(ThongBaoLoi);

    /// <summary>Tat nut Dang nhap trong luc dang goi API.</summary>
    public bool ChoPhepBam => !DangXuLy;

    partial void OnThongBaoLoiChanged(string value) => OnPropertyChanged(nameof(CoLoi));

    partial void OnDangXuLyChanged(bool value) => OnPropertyChanged(nameof(ChoPhepBam));

    /// <summary>Su kien bao cho cua so dang nhap biet da xac thuc xong.</summary>
    public event Action? DangNhapThanhCong;

    [RelayCommand]
    private async Task DangNhapAsync()
    {
        ThongBaoLoi = string.Empty;

        if (string.IsNullOrWhiteSpace(TenDangNhap) || string.IsNullOrWhiteSpace(MatKhau))
        {
            ThongBaoLoi = "Vui lòng nhập tên đăng nhập và mật khẩu.";
            return;
        }

        DangXuLy = true;
        var kq = await _api.DangNhapAsync(TenDangNhap.Trim(), MatKhau);
        DangXuLy = false;

        if (!kq.ThanhCong)
        {
            ThongBaoLoi = kq.ThongBao;
            return;
        }

        DangNhapThanhCong?.Invoke();
    }
}
