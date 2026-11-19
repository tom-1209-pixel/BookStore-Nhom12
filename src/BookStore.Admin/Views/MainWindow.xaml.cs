using BookStore.Admin.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace BookStore.Admin.Views;

/// <summary>
/// Cua so chinh cua ung dung quan tri. Dung NavigationView lam khung dieu huong,
/// moi muc menu nap mot Page vao Frame ben phai.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly PhienDangNhap _phien;

    public MainWindow()
    {
        _phien = App.LayDichVu<PhienDangNhap>();
        InitializeComponent();

        ONguoiDung.Text = _phien.NguoiDung is null
            ? string.Empty
            : $"{_phien.NguoiDung.HoTen}\n{_phien.NguoiDung.VaiTro}";

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));
        OKhungNoiDung.Navigate(typeof(TrangThongKe));
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem muc) return;

        Type trang = muc.Tag switch
        {
            "donhang" => typeof(TrangDonHang),
            "sach" => typeof(TrangSach),
            "danhmuc" => typeof(TrangDanhMuc),
            "kho" => typeof(TrangKho),
            "khachhang" => typeof(TrangKhachHang),
            "baocao" => typeof(TrangBaoCao),
            _ => typeof(TrangThongKe)
        };

        if (OKhungNoiDung.CurrentSourcePageType != trang)
            OKhungNoiDung.Navigate(trang, null, new EntranceNavigationTransitionInfo());
    }

    private void DangXuat_Click(object sender, RoutedEventArgs e)
    {
        _phien.Xoa();
        var cuaSo = new DangNhapWindow();
        cuaSo.Activate();
        Close();
    }
}
