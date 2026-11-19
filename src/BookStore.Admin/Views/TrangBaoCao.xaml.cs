using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookStore.Admin.Views;

/// <summary>Man hinh D08 - Bao cao thong ke va xuat Excel.</summary>
public sealed partial class TrangBaoCao : Page
{
    public BaoCaoViewModel ViewModel { get; }

    public TrangBaoCao()
    {
        ViewModel = App.LayDichVu<BaoCaoViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.XemAsync();
    }
}
