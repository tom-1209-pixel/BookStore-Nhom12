using System.Collections.ObjectModel;
using BookStore.Admin.Helpers;
using BookStore.Admin.Models;
using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

/// <summary>
/// Man hinh D05 - Quan ly don hang. Day la phia quan tri cua quy trinh nghiep vu
/// "dat hang - xu ly don": nhan vien xem don khach dat tren trang Web roi chuyen
/// trang thai theo so do Cho xac nhan -> Da xac nhan -> Dang giao -> Hoan thanh.
/// </summary>
public partial class DonHangViewModel : ObservableObject
{
    private readonly ApiClient _api;

    public DonHangViewModel(ApiClient api)
    {
        _api = api;
        BoLocTrangThai = DanhSachBoLoc[0];
    }

    public ObservableCollection<DonHang> DanhSachDon { get; } = new();

    [ObservableProperty] private DonHang? _donDangChon;
    [ObservableProperty] private string _tuKhoa = string.Empty;
    [ObservableProperty] private bool _dangTai;
    [ObservableProperty] private string _thongBao = string.Empty;
    [ObservableProperty] private bool _thongBaoLaLoi;
    [ObservableProperty] private string _ghiChuThaoTac = string.Empty;

    public record MucBoLoc(string Ten, byte? Ma)
    {
        public override string ToString() => Ten;
    }

    public List<MucBoLoc> DanhSachBoLoc { get; } = new()
    {
        new("Tất cả trạng thái", null),
        new("Chờ thanh toán", 0),
        new("Chờ xác nhận", 1),
        new("Đã xác nhận", 2),
        new("Đang giao", 3),
        new("Hoàn thành", 4),
        new("Đã hủy", 5)
    };

    [ObservableProperty] private MucBoLoc _boLocTrangThai;

    public bool CoThongBao => !string.IsNullOrEmpty(ThongBao);
    partial void OnThongBaoChanged(string value) => OnPropertyChanged(nameof(CoThongBao));

    partial void OnDonDangChonChanged(DonHang? value) => CapNhatTrangThaiNut();

    // Cac nut thao tac chi bat khi buoc chuyen trang thai hop le (so do Hinh 3.3)
    public bool ChoPhepXacNhan => DonDangChon?.TrangThai == 1;
    public bool ChoPhepGiao => DonDangChon?.TrangThai == 2;
    public bool ChoPhepHoanThanh => DonDangChon?.TrangThai == 3;
    public bool ChoPhepHuy => DonDangChon is not null && DonDangChon.TrangThai <= 3;

    private void CapNhatTrangThaiNut()
    {
        OnPropertyChanged(nameof(ChoPhepXacNhan));
        OnPropertyChanged(nameof(ChoPhepGiao));
        OnPropertyChanged(nameof(ChoPhepHoanThanh));
        OnPropertyChanged(nameof(ChoPhepHuy));
    }

    [RelayCommand]
    public async Task NapDanhSachAsync()
    {
        DangTai = true;
        ThongBao = string.Empty;

        var kq = await _api.TimDonHangAsync(TuKhoa, BoLocTrangThai?.Ma);

        DanhSachDon.Clear();
        if (kq.ThanhCong && kq.DuLieu is not null)
        {
            foreach (var d in kq.DuLieu.DanhSach) DanhSachDon.Add(d);
        }
        else
        {
            ThongBaoLaLoi = true;
            ThongBao = kq.ThongBao;
        }

        DangTai = false;
    }

    [RelayCommand]
    private async Task XemChiTietAsync(int maDonHang)
    {
        var kq = await _api.LayDonHangAsync(maDonHang);
        if (kq.ThanhCong && kq.DuLieu is not null)
            DonDangChon = kq.DuLieu;
    }

    /// <summary>In hoa don ban hang cua don dang chon (mo bang trinh duyet, tu goi lenh in).</summary>
    [RelayCommand]
    private void InHoaDon()
    {
        if (DonDangChon is null) return;
        try
        {
            TienIch.MoTep(TienIch.TaoHoaDon(DonDangChon));
            ThongBaoLaLoi = false;
            ThongBao = $"Đã mở hóa đơn đơn hàng #{DonDangChon.MaDonHang} để in.";
        }
        catch (Exception ex)
        {
            ThongBaoLaLoi = true;
            ThongBao = "Không tạo được hóa đơn: " + ex.Message;
        }
    }

    [RelayCommand]
    private Task XacNhanAsync() => ChuyenTrangThaiAsync(2);

    [RelayCommand]
    private Task GiaoHangAsync() => ChuyenTrangThaiAsync(3);

    [RelayCommand]
    private Task HoanThanhAsync() => ChuyenTrangThaiAsync(4);

    [RelayCommand]
    private Task HuyDonAsync() => ChuyenTrangThaiAsync(5);

    private async Task ChuyenTrangThaiAsync(byte trangThaiMoi)
    {
        if (DonDangChon is null) return;

        var ma = DonDangChon.MaDonHang;
        DangTai = true;

        var kq = await _api.ChuyenTrangThaiDonAsync(ma, trangThaiMoi,
            string.IsNullOrWhiteSpace(GhiChuThaoTac) ? null : GhiChuThaoTac.Trim());

        ThongBaoLaLoi = !kq.ThanhCong;
        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã cập nhật đơn hàng.") : kq.ThongBao;

        if (kq.ThanhCong)
        {
            GhiChuThaoTac = string.Empty;
            await NapDanhSachAsync();
            await XemChiTietAsync(ma);
        }

        DangTai = false;
    }
}
