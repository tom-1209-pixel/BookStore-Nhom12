using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookStore.Admin.Views;

/// <summary>Man hinh D02 - Tong quan (dashboard).</summary>
public sealed partial class TrangThongKe : Page
{
    public ThongKeViewModel ViewModel { get; }

    public TrangThongKe()
    {
        ViewModel = App.LayDichVu<ThongKeViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.NapAsync();
    }
}
