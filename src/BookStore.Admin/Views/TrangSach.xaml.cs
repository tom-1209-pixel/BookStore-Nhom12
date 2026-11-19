using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace BookStore.Admin.Views;

/// <summary>Man hinh D03 - Quan ly sach (them, sua, xoa, tim kiem).</summary>
public sealed partial class TrangSach : Page
{
    public SachViewModel ViewModel { get; }

    public TrangSach()
    {
        ViewModel = App.LayDichVu<SachViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.NapDuLieuAsync();
    }

    private async void OTuKhoa_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) await ViewModel.NapDuLieuAsync();
    }
}
