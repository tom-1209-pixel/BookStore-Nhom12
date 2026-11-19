using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace BookStore.Admin.Views;

/// <summary>Man hinh D07 - Quan ly khach hang.</summary>
public sealed partial class TrangKhachHang : Page
{
    public KhachHangViewModel ViewModel { get; }

    public TrangKhachHang()
    {
        ViewModel = App.LayDichVu<KhachHangViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.NapAsync();
    }

    private async void OTuKhoa_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) await ViewModel.NapAsync();
    }
}
