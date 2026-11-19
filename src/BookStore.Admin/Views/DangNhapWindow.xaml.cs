using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace BookStore.Admin.Views;

/// <summary>Man hinh dang nhap ung dung quan tri (D01).</summary>
public sealed partial class DangNhapWindow : Window
{
    public DangNhapViewModel ViewModel { get; }

    public DangNhapWindow()
    {
        ViewModel = App.LayDichVu<DangNhapViewModel>();
        InitializeComponent();

        ViewModel.DangNhapThanhCong += () =>
            DispatcherQueue.TryEnqueue(() => ((App)Application.Current).MoCuaSoChinh());

        AppWindow.Resize(new Windows.Graphics.SizeInt32(640, 760));

        // Dat con tro vao o Ten dang nhap ngay khi cua so hien ra
        Activated += DatTieuDiemLanDau;
    }

    private void DatTieuDiemLanDau(object sender, WindowActivatedEventArgs e)
    {
        Activated -= DatTieuDiemLanDau;
        OTenDangNhap.Focus(FocusState.Programmatic);
    }

    private void OMatKhau_PasswordChanged(object sender, RoutedEventArgs e)
        => ViewModel.MatKhau = OMatKhau.Password;

    private void OMatKhau_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter) return;
        if (ViewModel.DangNhapCommand.CanExecute(null))
            ViewModel.DangNhapCommand.Execute(null);
    }
}
