using System.Collections.ObjectModel;
using BookStore.Admin.Models;
using BookStore.Admin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookStore.Admin.ViewModels;

/// <summary>Man hinh D03 - Quan ly sach: them, sua, xoa, tim kiem.</summary>
public partial class SachViewModel : ObservableObject
{
    private readonly ApiClient _api;

    public SachViewModel(ApiClient api) => _api = api;

    public ObservableCollection<Sach> DanhSachSach { get; } = new();
    public ObservableCollection<LoaiSach> DanhMuc { get; } = new();
    public ObservableCollection<DanhMuc> NhaXuatBans { get; } = new();

    /// <summary>Danh sach tac gia kem o danh dau, de gan nhieu tac gia cho mot quyen sach.</summary>
    public ObservableCollection<LuaChonTacGia> TacGias { get; } = new();

    [ObservableProperty] private Sach? _sachDangChon;
    [ObservableProperty] private string _tuKhoa = string.Empty;
    [ObservableProperty] private bool _dangTai;
    [ObservableProperty] private string _thongBao = string.Empty;
    [ObservableProperty] private bool _thongBaoLaLoi;

    // Cac truong cua bieu mau them / sua
    [ObservableProperty] private int _formMaSach;
    [ObservableProperty] private string _formTenSach = string.Empty;
    [ObservableProperty] private string _formIsbn = string.Empty;
    [ObservableProperty] private LoaiSach? _formLoaiSach;
    [ObservableProperty] private decimal _formGiaBan;
    [ObservableProperty] private int _formSoLuongTon;
    [ObservableProperty] private int _formNguongCanhBao = 5;
    [ObservableProperty] private DanhMuc? _formNhaXuatBan;
    [ObservableProperty] private string _formMoTa = string.Empty;
    [ObservableProperty] private bool _formDangKinhDoanh = true;

    public bool CoThongBao => !string.IsNullOrEmpty(ThongBao);
    partial void OnThongBaoChanged(string value) => OnPropertyChanged(nameof(CoThongBao));

    public bool DangSua => FormMaSach > 0;
    public string TieuDeForm => DangSua ? $"Sửa sách #{FormMaSach}" : "Thêm sách mới";

    partial void OnFormMaSachChanged(int value)
    {
        OnPropertyChanged(nameof(DangSua));
        OnPropertyChanged(nameof(TieuDeForm));
    }

    partial void OnSachDangChonChanged(Sach? value)
    {
        if (value is null) return;
        FormMaSach = value.MaSach;
        FormTenSach = value.TenSach;
        FormIsbn = value.ISBN ?? string.Empty;
        FormLoaiSach = DanhMuc.FirstOrDefault(l => l.MaLoaiSach == value.MaLoaiSach);
        FormGiaBan = value.GiaBan;
        FormSoLuongTon = value.SoLuongTon;
        FormNguongCanhBao = value.NguongCanhBao;
        FormNhaXuatBan = NhaXuatBans.FirstOrDefault(n => n.Ma == value.MaNXB);
        FormMoTa = value.MoTa ?? string.Empty;
        FormDangKinhDoanh = value.TrangThai == 1;
        foreach (var tg in TacGias) tg.DaChon = value.MaTacGias.Contains(tg.Ma);
    }

    [RelayCommand]
    public async Task NapDuLieuAsync()
    {
        DangTai = true;
        ThongBao = string.Empty;

        // Nap lai danh muc moi lan vao man hinh vi Quan ly co the vua them o man hinh Danh muc
        var kqDm = await _api.LayDanhMucAsync();
        if (kqDm.ThanhCong && kqDm.DuLieu is not null)
        {
            DanhMuc.Clear();
            foreach (var l in kqDm.DuLieu) DanhMuc.Add(l);
        }

        var kqNxb = await _api.LayDanhMucChungAsync("nhaxuatban");
        if (kqNxb.ThanhCong && kqNxb.DuLieu is not null)
        {
            NhaXuatBans.Clear();
            foreach (var n in kqNxb.DuLieu) NhaXuatBans.Add(n);
        }

        var kqTg = await _api.LayDanhMucChungAsync("tacgia");
        if (kqTg.ThanhCong && kqTg.DuLieu is not null)
        {
            TacGias.Clear();
            foreach (var t in kqTg.DuLieu) TacGias.Add(new LuaChonTacGia(t.Ma, t.Ten));
        }

        var kq = await _api.TimSachAsync(TuKhoa, null, 1, 100);
        DanhSachSach.Clear();
        if (kq.ThanhCong && kq.DuLieu is not null)
        {
            foreach (var s in kq.DuLieu.DanhSach) DanhSachSach.Add(s);
        }
        else
        {
            ThongBaoLaLoi = true;
            ThongBao = kq.ThongBao;
        }

        DangTai = false;
    }

    [RelayCommand]
    private void LamMoiForm()
    {
        SachDangChon = null;
        FormMaSach = 0;
        FormTenSach = string.Empty;
        FormIsbn = string.Empty;
        FormLoaiSach = DanhMuc.FirstOrDefault();
        FormGiaBan = 0;
        FormSoLuongTon = 0;
        FormNguongCanhBao = 5;
        FormNhaXuatBan = null;
        foreach (var tg in TacGias) tg.DaChon = false;
        FormMoTa = string.Empty;
        FormDangKinhDoanh = true;
        ThongBao = string.Empty;
    }

    [RelayCommand]
    private async Task LuuAsync()
    {
        if (string.IsNullOrWhiteSpace(FormTenSach))
        {
            ThongBaoLaLoi = true;
            ThongBao = "Vui lòng nhập tên sách.";
            return;
        }

        if (FormLoaiSach is null)
        {
            ThongBaoLaLoi = true;
            ThongBao = "Vui lòng chọn loại sách.";
            return;
        }

        var duLieu = new
        {
            maSach = FormMaSach,
            isbn = string.IsNullOrWhiteSpace(FormIsbn) ? null : FormIsbn.Trim(),
            tenSach = FormTenSach.Trim(),
            maLoaiSach = FormLoaiSach.MaLoaiSach,
            giaBan = FormGiaBan,
            soLuongTon = FormSoLuongTon,
            nguongCanhBao = FormNguongCanhBao,
            maNXB = FormNhaXuatBan?.Ma,
            maTacGias = TacGias.Where(t => t.DaChon).Select(t => t.Ma).ToList(),
            moTa = FormMoTa,
            trangThai = FormDangKinhDoanh ? 1 : 0
        };

        DangTai = true;
        var kq = DangSua
            ? await _api.CapNhatSachAsync(FormMaSach, duLieu)
            : await _api.ThemSachAsync(duLieu);
        DangTai = false;

        ThongBaoLaLoi = !kq.ThanhCong;
        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã lưu.") : kq.ThongBao;

        if (kq.ThanhCong)
        {
            LamMoiForm();
            await NapDuLieuAsync();
        }
    }

    [RelayCommand]
    private async Task XoaAsync()
    {
        if (SachDangChon is null) return;

        DangTai = true;
        var kq = await _api.XoaSachAsync(SachDangChon.MaSach);
        DangTai = false;

        ThongBaoLaLoi = !kq.ThanhCong;
        ThongBao = kq.ThanhCong ? (kq.DuLieu?.ThongBao ?? "Đã xóa.") : kq.ThongBao;

        if (kq.ThanhCong)
        {
            LamMoiForm();
            await NapDuLieuAsync();
        }
    }
}

/// <summary>Mot tac gia trong danh sach chon cua bieu mau sach.</summary>
public partial class LuaChonTacGia : ObservableObject
{
    public LuaChonTacGia(int ma, string ten)
    {
        Ma = ma;
        Ten = ten;
    }

    public int Ma { get; }
    public string Ten { get; }

    [ObservableProperty] private bool _daChon;
}
