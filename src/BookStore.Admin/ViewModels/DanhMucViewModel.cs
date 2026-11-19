using System.Collections.ObjectModel;
using BookStore.Admin.Models;
using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

/// <summary>
/// Man hinh D04 - Quan ly danh muc. Bon danh muc dung chung mot bo cuc: danh sach ben
/// trai, bieu mau ben phai; truong nao hien ra tuy loai danh muc dang chon.
/// </summary>
public partial class DanhMucViewModel : ObservableObject
{
    private readonly ApiClient _api;

    public record LoaiDanhMuc(string Ten, string Ma, string NhanMoTa, bool CoMoTa, bool CoLienHe, bool CoEmail)
    {
        public override string ToString() => Ten;
    }

    public List<LoaiDanhMuc> CacLoai { get; } = new()
    {
        new("Loại sách", "loaisach", "Mô tả", true, false, false),
        new("Tác giả", "tacgia", "Tiểu sử", true, false, false),
        new("Nhà xuất bản", "nhaxuatban", "", false, true, false),
        new("Nhà cung cấp", "nhacungcap", "", false, true, true)
    };

    public DanhMucViewModel(ApiClient api)
    {
        _api = api;
        _loaiDangChon = CacLoai[0];
    }

    public ObservableCollection<DanhMuc> DanhSach { get; } = new();

    [ObservableProperty] private LoaiDanhMuc _loaiDangChon;
    [ObservableProperty] private DanhMuc? _mucDangChon;
    [ObservableProperty] private string _tuKhoa = string.Empty;
    [ObservableProperty] private bool _dangTai;
    [ObservableProperty] private string _thongBao = string.Empty;

    [ObservableProperty] private int _formMa;
    [ObservableProperty] private string _formTen = string.Empty;
    [ObservableProperty] private string _formMoTa = string.Empty;
    [ObservableProperty] private string _formDiaChi = string.Empty;
    [ObservableProperty] private string _formSoDienThoai = string.Empty;
    [ObservableProperty] private string _formEmail = string.Empty;

    public bool CoThongBao => !string.IsNullOrEmpty(ThongBao);
    partial void OnThongBaoChanged(string value) => OnPropertyChanged(nameof(CoThongBao));

    public bool DangSua => FormMa > 0;
    public string TieuDeForm => DangSua ? $"Sửa {LoaiDangChon.Ten.ToLower()} #{FormMa}" : $"Thêm {LoaiDangChon.Ten.ToLower()}";
    public bool HienMoTa => LoaiDangChon.CoMoTa;
    public bool HienLienHe => LoaiDangChon.CoLienHe;
    public bool HienEmail => LoaiDangChon.CoEmail;

    partial void OnFormMaChanged(int value)
    {
        OnPropertyChanged(nameof(DangSua));
        OnPropertyChanged(nameof(TieuDeForm));
    }

    partial void OnLoaiDangChonChanged(LoaiDanhMuc value)
    {
        OnPropertyChanged(nameof(HienMoTa));
        OnPropertyChanged(nameof(HienLienHe));
        OnPropertyChanged(nameof(HienEmail));
        LamMoiForm();
        _ = NapAsync();
    }

    partial void OnMucDangChonChanged(DanhMuc? value)
    {
        if (value is null) return;
        FormMa = value.Ma;
        FormTen = value.Ten;
        FormMoTa = value.MoTa ?? string.Empty;
        FormDiaChi = value.DiaChi ?? string.Empty;
        FormSoDienThoai = value.SoDienThoai ?? string.Empty;
        FormEmail = value.Email ?? string.Empty;
        ThongBao = value.SoLienKet > 0
            ? $"Đang được dùng ở {value.SoLienKet} {(LoaiDangChon.Ma == "nhacungcap" ? "phiếu nhập" : "sách")} nên không xóa được."
            : string.Empty;
    }

    [RelayCommand]
    public async Task NapAsync()
    {
        DangTai = true;
        var kq = await _api.LayDanhMucChungAsync(LoaiDangChon.Ma, TuKhoa);
        DanhSach.Clear();
        if (kq.ThanhCong && kq.DuLieu is not null)
            foreach (var m in kq.DuLieu) DanhSach.Add(m);
        else
            ThongBao = kq.ThongBao;
        DangTai = false;
    }

    [RelayCommand]
    private void LamMoiForm()
    {
        MucDangChon = null;
        FormMa = 0;
        FormTen = FormMoTa = FormDiaChi = FormSoDienThoai = FormEmail = string.Empty;
        ThongBao = string.Empty;
        OnPropertyChanged(nameof(TieuDeForm));
    }

    [RelayCommand]
    private async Task LuuAsync()
    {
        if (string.IsNullOrWhiteSpace(FormTen))
        {
            ThongBao = "Vui lòng nhập tên.";
            return;
        }

        var muc = new DanhMuc
        {
            Ma = FormMa,
            Ten = FormTen.Trim(),
            MoTa = HienMoTa ? FormMoTa : null,
            DiaChi = HienLienHe ? FormDiaChi : null,
            SoDienThoai = HienLienHe ? FormSoDienThoai : null,
            Email = HienEmail ? FormEmail : null
        };

        DangTai = true;
        var kq = await _api.LuuDanhMucAsync(LoaiDangChon.Ma, muc);
        DangTai = false;

        if (kq.ThanhCong)
        {
            LamMoiForm();
            await NapAsync();
            ThongBao = kq.DuLieu?.ThongBao ?? "Đã lưu.";
        }
        else ThongBao = kq.ThongBao;
    }

    [RelayCommand]
    private async Task XoaAsync()
    {
        if (FormMa <= 0) return;
        DangTai = true;
        var kq = await _api.XoaDanhMucAsync(LoaiDangChon.Ma, FormMa);
        DangTai = false;

        if (kq.ThanhCong)
        {
            LamMoiForm();
            await NapAsync();
            ThongBao = kq.DuLieu?.ThongBao ?? "Đã xóa.";
        }
        else ThongBao = kq.ThongBao;
    }
}
