using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace BookStore.Admin.Views;

/// <summary>Man hinh D04 - Quan ly loai sach, tac gia, nha xuat ban, nha cung cap.</summary>
public sealed partial class TrangDanhMuc : Page
{
    public DanhMucViewModel ViewModel { get; }

    public TrangDanhMuc()
    {
        ViewModel = App.LayDichVu<DanhMucViewModel>();
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
