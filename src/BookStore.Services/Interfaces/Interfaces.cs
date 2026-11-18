using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;

namespace BookStore.Services.Interfaces;

/// <summary>Xac thuc va quan ly tai khoan. Dung chung cho Web va Web API.</summary>
public interface ITaiKhoanService
{
    Task<KetQua<NguoiDungDto>> DangNhapAsync(DangNhapDto dto, bool laUngDungQuanTri);
    Task<KetQua<NguoiDungDto>> DangKyAsync(DangKyDto dto);
    Task<KetQua> DoiMatKhauAsync(int maTaiKhoan, DoiMatKhauDto dto);
    Task<NguoiDungDto?> LayNguoiDungAsync(int maTaiKhoan);

    /// <summary>Lay dia chi giao hang mac dinh de dien san vao trang thanh toan.</summary>
    Task<DiaChiGiaoHangDto?> LayDiaChiMacDinhAsync(int maKhachHang);

    /// <summary>Quen mat khau: sinh token, gui lien ket dat lai qua email. Luon tra ve cung mot thong bao.</summary>
    Task<KetQua> YeuCauDatLaiMatKhauAsync(string email, Func<string, string> taoLienKet);

    Task<KetQua> DatLaiMatKhauAsync(string token, string matKhauMoi, string xacNhanMatKhau);
}

public interface ISachService
{
    Task<TrangKetQua<SachDto>> TimAsync(TimSachDto dto);
    Task<SachDto?> LayTheoMaAsync(int maSach);
    Task<List<SachDto>> LaySachMoiNhatAsync(int soLuong = 8);
    Task<KetQua<int>> ThemAsync(SachLuuDto dto);
    Task<KetQua> CapNhatAsync(SachLuuDto dto);
    Task<KetQua> XoaAsync(int maSach);
    Task<List<LoaiSachDto>> LayDanhMucAsync();
}

public interface IGioHangService
{
    Task<GioHangDto> LayGioHangAsync(int maKhachHang);
    Task<KetQua> ThemVaoGioAsync(int maKhachHang, int maSach, int soLuong);
    Task<KetQua> CapNhatSoLuongAsync(int maKhachHang, int maSach, int soLuong);
    Task<KetQua> XoaDongAsync(int maKhachHang, int maSach);
    Task<KetQua> XoaGioAsync(int maKhachHang);
}

public interface IDonHangService
{
    /// <summary>Tao don hang tu gio hang. Thuc hien BR01, BR02, BR10 trong mot transaction.</summary>
    Task<KetQua<DonHangDto>> DatHangAsync(int maKhachHang, DatHangDto dto);

    Task<DonHangDto?> LayChiTietAsync(int maDonHang, int? maKhachHangLoc = null);
    Task<TrangKetQua<DonHangDto>> TimAsync(TimDonHangDto dto);
    Task<List<DonHangDto>> LayDonCuaKhachAsync(int maKhachHang);

    /// <summary>Nhan vien doi trang thai don theo so do trang thai (Hinh 3.3).</summary>
    Task<KetQua> ChuyenTrangThaiAsync(int maDonHang, TrangThaiDonHang trangThaiMoi, string nguoiThucHien, int? maQuanLy, string? ghiChu = null);

    /// <summary>Khach hang tu huy don. Chi cho phep o trang thai 0 hoac 1 (BR05).</summary>
    Task<KetQua> KhachHuyDonAsync(int maDonHang, int maKhachHang, string? lyDo);

    /// <summary>BR04: huy cac don VNPay qua han thanh toan.</summary>
    Task<int> HuyDonQuaHanAsync();
}

public interface IBaoCaoService
{
    Task<ThongKeDto> LayThongKeAsync();
    Task<decimal> TinhDoanhThuAsync(DateTime tuNgay, DateTime denNgay);
    Task<List<SachBanChayDto>> TopSachBanChayAsync(DateTime tuNgay, DateTime denNgay, int soDong = 10);
    Task<BaoCaoDoanhThuDto> LayBaoCaoDoanhThuAsync(DateTime tuNgay, DateTime denNgay);
    Task<List<NhapXuatTonDto>> LayNhapXuatTonAsync(DateTime tuNgay, DateTime denNgay);
    Task<List<DonTheoTrangThaiDto>> DemDonTheoTrangThaiAsync(DateTime tuNgay, DateTime denNgay);
}

/// <summary>Loai sach, tac gia, nha xuat ban, nha cung cap (D04).</summary>
public interface IDanhMucService
{
    Task<List<DanhMucDto>> LayDanhSachAsync(LoaiDanhMuc loai, string? tuKhoa = null);
    Task<KetQua<int>> LuuAsync(LoaiDanhMuc loai, DanhMucDto dto);
    Task<KetQua> XoaAsync(LoaiDanhMuc loai, int ma);
}

/// <summary>Ton kho va phieu nhap (D06). Cai dat BR06.</summary>
public interface IKhoService
{
    Task<List<TonKhoDto>> LayTonKhoAsync(string? tuKhoa = null, bool chiSapHetHang = false);
    Task<KetQua> CapNhatNguongAsync(int maSach, int nguong);
    Task<List<PhieuNhapDto>> TimPhieuNhapAsync(TrangThaiPhieuNhap? trangThai = null, string? tuKhoa = null);
    Task<PhieuNhapDto?> LayPhieuNhapAsync(int maPhieuNhap);
    Task<KetQua<int>> TaoPhieuNhapAsync(int maQuanLy, PhieuNhapLuuDto dto);
    Task<KetQua> CapNhatPhieuNhapAsync(int maPhieuNhap, PhieuNhapLuuDto dto);
    Task<KetQua> HoanThanhPhieuNhapAsync(int maPhieuNhap);
    Task<KetQua> XoaPhieuNhapAsync(int maPhieuNhap);
}

/// <summary>Quan ly khach hang (D07).</summary>
public interface IKhachHangService
{
    Task<List<KhachHangDto>> TimAsync(string? tuKhoa = null, TrangThaiTaiKhoan? trangThai = null);
    Task<KhachHangDto?> LayChiTietAsync(int maKhachHang);
    Task<KetQua> DoiTrangThaiAsync(int maKhachHang, bool khoa, string? lyDo);
}

public interface IEmailService
{
    Task<KetQua> GuiAsync(string nguoiNhan, string tieuDe, string noiDungHtml);
}

/// <summary>Thanh toan truc tuyen qua VNPay (co cong gia lap du phong).</summary>
public interface IThanhToanService
{
    bool DungCongThat { get; }
    Task<KetQua<string>> TaoYeuCauThanhToanAsync(int maDonHang, int maKhachHang, string ipKhach, string urlTraVe, string urlCongGiaLap);
    Task<KetQua<int>> XuLyKetQuaTraVeAsync(IReadOnlyDictionary<string, string> thamSo);
    KetQua<string> TaoKetQuaGiaLap(IReadOnlyDictionary<string, string> thamSoYeuCau, bool thanhCong);
}

public interface IXuatExcelService
{
    Task<byte[]> XuatDoanhThuAsync(DateTime tuNgay, DateTime denNgay);
    Task<byte[]> XuatNhapXuatTonAsync(DateTime tuNgay, DateTime denNgay);
}
