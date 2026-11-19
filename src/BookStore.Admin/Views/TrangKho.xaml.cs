using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace BookStore.Admin.Views;

/// <summary>Man hinh D06 - Quan ly kho va nhap hang.</summary>
public sealed partial class TrangKho : Page
{
    public KhoViewModel ViewModel { get; }

    public TrangKho()
    {
        ViewModel = App.LayDichVu<KhoViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.NapPhieuNhapAsync();
        await ViewModel.NapTonKhoAsync();
    }

    private async void OTuKhoaTon_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) await ViewModel.NapTonKhoAsync();
    }
}
