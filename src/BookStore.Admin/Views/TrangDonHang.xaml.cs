using BookStore.Admin.Models;
using BookStore.Admin.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace BookStore.Admin.Views;

/// <summary>
/// Man hinh D05 - Quan ly don hang. Nhan vien chon mot don trong danh sach,
/// xem chi tiet va bam nut chuyen trang thai.
/// </summary>
public sealed partial class TrangDonHang : Page
{
    public DonHangViewModel ViewModel { get; }

    public TrangDonHang()
    {
        ViewModel = App.LayDichVu<DonHangViewModel>();
        InitializeComponent();
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.DonDangChon)) VeChiTiet();
        };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.NapDanhSachAsync();
    }

    private async void ODanhSachDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ODanhSachDon.SelectedItem is not DonHang don) return;
        await ViewModel.XemChiTietCommand.ExecuteAsync(don.MaDonHang);
    }

    private async void OTuKhoa_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) await ViewModel.NapDanhSachAsync();
    }

    private async void OBoLoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        await ViewModel.NapDanhSachAsync();
    }

    /// <summary>Do du lieu don dang chon ra cac o chi tiet.</summary>
    private void VeChiTiet()
    {
        var don = ViewModel.DonDangChon;

        OChiTiet.Visibility = don is null ? Visibility.Collapsed : Visibility.Visible;
        OChuaChon.Visibility = don is null ? Visibility.Visible : Visibility.Collapsed;
        if (don is null) return;

        OMaDon.Text = $"Đơn #{don.MaDonHang} — {don.TenTrangThai}";
        OThongTinNhan.Text =
            $"{don.HoTenNguoiNhan} · {don.SoDienThoaiNhan}\n{don.DiaChiGiao}\n" +
            $"{don.TenHinhThucThanhToan}" +
            (string.IsNullOrWhiteSpace(don.GhiChu) ? "" : $"\nGhi chú: {don.GhiChu}");

        ODongDon.ItemsSource = don.Dong;
        OLichSu.ItemsSource = don.LichSu;

        OTongTien.Text =
            $"Tạm tính {don.TamTinh:#,##0} đ · Phí giao {don.PhiVanChuyen:#,##0} đ · " +
            $"Tổng cộng {don.TongTien:#,##0} đ";
    }
}
