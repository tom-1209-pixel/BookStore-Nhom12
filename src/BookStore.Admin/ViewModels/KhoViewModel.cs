using System.Collections.ObjectModel;
using BookStore.Admin.Models;
using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

/// <summary>
/// Man hinh D06 - Quan ly kho va nhap hang. The Ton kho: xem ton, canh bao, dat nguong.
/// The Phieu nhap: lap, sua, hoan thanh (cong ton - BR08), xoa phieu chua hoan thanh.
/// </summary>
public partial class KhoViewModel : ObservableObject
{
    private readonly ApiClient _api;

    public KhoViewModel(ApiClient api) => _api = api;

    [ObservableProperty] private bool _dangTai;
    [ObservableProperty] private string _thongBao = string.Empty;
    public bool CoThongBao => !string.IsNullOrEmpty(ThongBao);
    partial void OnThongBaoChanged(string value) => OnPropertyChanged(nameof(CoThongBao));

    // ------------------------------------------------------------ Ton kho

    public ObservableCollection<TonKho> DanhSachTonKho { get; } = new();

    [ObservableProperty] private string _tuKhoaTon = string.Empty;
    [ObservableProperty] private bool _chiSapHetHang;
    [ObservableProperty] private TonKho? _tonDangChon;
    [ObservableProperty] private int _nguongMoi;

    public bool CoTonDangChon => TonDangChon is not null;

    partial void OnTonDangChonChanged(TonKho? value)
    {
        OnPropertyChanged(nameof(CoTonDangChon));
        if (value is not null) NguongMoi = value.NguongCanhBao;
    }

    partial void OnChiSapHetHangChanged(bool value) => _ = NapTonKhoAsync();

    [RelayCommand]
    public async Task NapTonKhoAsync()
    {
        DangTai = true;
        var kq = await _api.LayTonKhoAsync(TuKhoaTon, ChiSapHetHang);
        DanhSachTonKho.Clear();
        if (kq.ThanhCong && kq.DuLieu is not null)
            foreach (var t in kq.DuLieu) DanhSachTonKho.Add(t);
        else ThongBao = kq.ThongBao;
        DangTai = false;
    }

    [RelayCommand]
    private async Task LuuNguongAsync()
    {
        if (TonDangChon is null) return;
        var kq = await _api.CapNhatNguongAsync(TonDangChon.MaSach, NguongMoi);
        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã lưu ngưỡng.") : kq.ThongBao;
        if (kq.ThanhCong) await NapTonKhoAsync();
    }

    // ------------------------------------------------------------ Phieu nhap

    public ObservableCollection<PhieuNhap> DanhSachPhieu { get; } = new();
    public ObservableCollection<DanhMuc> NhaCungCaps { get; } = new();
    public ObservableCollection<Sach> SachCoTheNhap { get; } = new();
    public ObservableCollection<DongPhieuNhap> DongPhieu { get; } = new();

    [ObservableProperty] private PhieuNhap? _phieuDangChon;
    [ObservableProperty] private int _formMaPhieu;
    [ObservableProperty] private bool _formDaHoanThanh;
    [ObservableProperty] private DanhMuc? _formNhaCungCap;
    [ObservableProperty] private string _formGhiChu = string.Empty;
    [ObservableProperty] private Sach? _dongMoiSach;
    [ObservableProperty] private int _dongMoiSoLuong = 1;
    [ObservableProperty] private decimal _dongMoiGia;
    [ObservableProperty] private DongPhieuNhap? _dongDangChon;

    public string TieuDeForm => FormMaPhieu == 0 ? "Lập phiếu nhập mới"
        : FormDaHoanThanh ? $"Phiếu nhập #{FormMaPhieu} (đã hoàn thành)" : $"Sửa phiếu nhập #{FormMaPhieu}";
    public bool ChoPhepSua => !FormDaHoanThanh;
    public bool ChoPhepHoanThanh => FormMaPhieu > 0 && !FormDaHoanThanh;
    public bool ChoPhepXoa => FormMaPhieu > 0 && !FormDaHoanThanh;
    public string TongTienForm => DongPhieu.Sum(d => d.ThanhTien).ToString("#,##0") + " đ";

    private void CapNhatForm()
    {
        OnPropertyChanged(nameof(TieuDeForm));
        OnPropertyChanged(nameof(ChoPhepSua));
        OnPropertyChanged(nameof(ChoPhepHoanThanh));
        OnPropertyChanged(nameof(ChoPhepXoa));
        OnPropertyChanged(nameof(TongTienForm));
    }

    partial void OnFormMaPhieuChanged(int value) => CapNhatForm();
    partial void OnFormDaHoanThanhChanged(bool value) => CapNhatForm();

    partial void OnPhieuDangChonChanged(PhieuNhap? value)
    {
        if (value is not null) _ = NapChiTietPhieuAsync(value.MaPhieuNhap);
    }

    [RelayCommand]
    public async Task NapPhieuNhapAsync()
    {
        DangTai = true;

        if (NhaCungCaps.Count == 0)
        {
            var ncc = await _api.LayDanhMucChungAsync("nhacungcap");
            if (ncc.ThanhCong && ncc.DuLieu is not null) foreach (var n in ncc.DuLieu) NhaCungCaps.Add(n);
        }

        var sach = await _api.TimSachAsync(null, null, 1, 500);
        if (sach.ThanhCong && sach.DuLieu is not null)
        {
            SachCoTheNhap.Clear();
            foreach (var s in sach.DuLieu.DanhSach) SachCoTheNhap.Add(s);
        }

        var kq = await _api.LayPhieuNhapAsync(null);
        DanhSachPhieu.Clear();
        if (kq.ThanhCong && kq.DuLieu is not null)
            foreach (var p in kq.DuLieu) DanhSachPhieu.Add(p);
        else ThongBao = kq.ThongBao;

        DangTai = false;
    }

    private async Task NapChiTietPhieuAsync(int ma)
    {
        var kq = await _api.LayChiTietPhieuNhapAsync(ma);
        if (!kq.ThanhCong || kq.DuLieu is null) { ThongBao = kq.ThongBao; return; }

        var p = kq.DuLieu;
        FormMaPhieu = p.MaPhieuNhap;
        FormDaHoanThanh = p.DaHoanThanh;
        FormNhaCungCap = NhaCungCaps.FirstOrDefault(n => n.Ma == p.MaNCC);
        FormGhiChu = p.GhiChu ?? string.Empty;
        DongPhieu.Clear();
        foreach (var d in p.Dong) DongPhieu.Add(d);
        CapNhatForm();
    }

    [RelayCommand]
    private void PhieuMoi()
    {
        PhieuDangChon = null;
        FormMaPhieu = 0;
        FormDaHoanThanh = false;
        FormNhaCungCap = NhaCungCaps.FirstOrDefault();
        FormGhiChu = string.Empty;
        DongPhieu.Clear();
        ThongBao = string.Empty;
        CapNhatForm();
    }

    [RelayCommand]
    private void ThemDong()
    {
        if (DongMoiSach is null) { ThongBao = "Chọn sách cần nhập."; return; }
        if (DongMoiSoLuong <= 0) { ThongBao = "Số lượng nhập phải lớn hơn 0."; return; }
        if (DongPhieu.Any(d => d.MaSach == DongMoiSach.MaSach))
        {
            ThongBao = "Sách này đã có trong phiếu, hãy xóa dòng cũ nếu muốn đổi số lượng.";
            return;
        }

        DongPhieu.Add(new DongPhieuNhap
        {
            MaSach = DongMoiSach.MaSach,
            TenSach = DongMoiSach.TenSach,
            SoLuong = DongMoiSoLuong,
            DonGiaNhap = DongMoiGia
        });
        DongMoiSach = null;
        DongMoiSoLuong = 1;
        DongMoiGia = 0;
        ThongBao = string.Empty;
        CapNhatForm();
    }

    [RelayCommand]
    private void XoaDong()
    {
        if (DongDangChon is null) return;
        DongPhieu.Remove(DongDangChon);
        CapNhatForm();
    }

    [RelayCommand]
    private async Task LuuPhieuAsync()
    {
        if (FormNhaCungCap is null) { ThongBao = "Chọn nhà cung cấp."; return; }
        if (DongPhieu.Count == 0) { ThongBao = "Phiếu nhập phải có ít nhất một sách."; return; }

        var duLieu = new
        {
            maNCC = FormNhaCungCap.Ma,
            ghiChu = FormGhiChu,
            dong = DongPhieu.Select(d => new { maSach = d.MaSach, soLuong = d.SoLuong, donGiaNhap = d.DonGiaNhap }).ToList()
        };

        DangTai = true;
        var kq = await _api.LuuPhieuNhapAsync(FormMaPhieu, duLieu);
        DangTai = false;

        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã lưu phiếu nhập.") : kq.ThongBao;
        if (!kq.ThanhCong) return;

        var ma = FormMaPhieu > 0 ? FormMaPhieu : kq.DuLieu?.Ma ?? 0;
        await NapPhieuNhapAsync();
        if (ma > 0) await NapChiTietPhieuAsync(ma);
    }

    [RelayCommand]
    private async Task HoanThanhAsync()
    {
        if (!ChoPhepHoanThanh) return;
        var ma = FormMaPhieu;
        DangTai = true;
        var kq = await _api.HoanThanhPhieuNhapAsync(ma);
        DangTai = false;
        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã hoàn thành phiếu nhập.") : kq.ThongBao;
        if (!kq.ThanhCong) return;

        await NapPhieuNhapAsync();
        await NapChiTietPhieuAsync(ma);
        await NapTonKhoAsync();
    }

    [RelayCommand]
    private async Task XoaPhieuAsync()
    {
        if (!ChoPhepXoa) return;
        DangTai = true;
        var kq = await _api.XoaPhieuNhapAsync(FormMaPhieu);
        DangTai = false;
        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã xóa phiếu nhập.") : kq.ThongBao;
        if (!kq.ThanhCong) return;

        PhieuMoi();
        await NapPhieuNhapAsync();
    }
}
