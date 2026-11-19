using System.Collections.ObjectModel;
using BookStore.Admin.Models;
using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

/// <summary>Man hinh D02 - Dashboard tong quan.</summary>
public partial class ThongKeViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly PhienDangNhap _phien;

    public ThongKeViewModel(ApiClient api, PhienDangNhap phien)
    {
        _api = api;
        _phien = phien;
    }

    [ObservableProperty] private ThongKe _soLieu = new();
    [ObservableProperty] private bool _dangTai;
    [ObservableProperty] private string _thongBaoLoi = string.Empty;

    public ObservableCollection<SachBanChay> TopSach { get; } = new();

    public string LoiChao =>
        $"Xin chào {_phien.NguoiDung?.HoTen ?? "bạn"} ({_phien.NguoiDung?.VaiTro ?? "—"})";

    public string NgayHomNay => DateTime.Now.ToString("dddd, dd/MM/yyyy",
        new System.Globalization.CultureInfo("vi-VN"));

    [RelayCommand]
    public async Task NapAsync()
    {
        DangTai = true;
        ThongBaoLoi = string.Empty;

        var kq = await _api.LayThongKeAsync();
        if (kq.ThanhCong && kq.DuLieu is not null)
        {
            SoLieu = kq.DuLieu;
            TopSach.Clear();
            foreach (var s in kq.DuLieu.TopSachBanChay) TopSach.Add(s);
        }
        else
        {
            ThongBaoLoi = kq.ThongBao;
        }

        DangTai = false;
    }
}
