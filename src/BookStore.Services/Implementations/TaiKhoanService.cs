using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services.Implementations;

public class TaiKhoanService : ITaiKhoanService
{
    private readonly BookStoreDbContext _db;
    private readonly ILogger<TaiKhoanService> _log;
    private readonly IEmailService _email;
    private readonly PasswordHasher<TaiKhoan> _hasher = new();

    /// <summary>Lien ket dat lai mat khau chi co hieu luc trong so phut nay.</summary>
    public const int SoPhutHieuLucToken = 30;

    public TaiKhoanService(BookStoreDbContext db, ILogger<TaiKhoanService> log, IEmailService email)
    {
        _db = db;
        _log = log;
        _email = email;
    }

    public async Task<KetQua<NguoiDungDto>> DangNhapAsync(DangNhapDto dto, bool laUngDungQuanTri)
    {
        if (string.IsNullOrWhiteSpace(dto.TenDangNhap) || string.IsNullOrWhiteSpace(dto.MatKhau))
            return KetQua<NguoiDungDto>.Loi("Vui lòng nhập tên đăng nhập và mật khẩu.");

        var ten = dto.TenDangNhap.Trim();
        var tk = await _db.TaiKhoan
            .Include(x => x.VaiTro)
            .Include(x => x.KhachHang)
            .Include(x => x.QuanLy)
            .FirstOrDefaultAsync(x => x.TenDangNhap == ten);

        if (tk is null)
        {
            _log.LogWarning("Đăng nhập thất bại: tài khoản {TenDangNhap} không tồn tại.", ten);
            return KetQua<NguoiDungDto>.Loi("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        if (tk.TrangThai == TrangThaiTaiKhoan.BiKhoa)
            return KetQua<NguoiDungDto>.Loi("Tài khoản đã bị khóa. Vui lòng liên hệ quản lý.");

        if (tk.ThoiDiemHetKhoa.HasValue && tk.ThoiDiemHetKhoa > DateTime.Now)
        {
            var conLai = (int)Math.Ceiling((tk.ThoiDiemHetKhoa.Value - DateTime.Now).TotalMinutes);
            return KetQua<NguoiDungDto>.Loi($"Tài khoản đang tạm khóa, vui lòng thử lại sau {conLai} phút.");
        }

        var kq = _hasher.VerifyHashedPassword(tk, tk.MatKhauHash, dto.MatKhau);
        if (kq == PasswordVerificationResult.Failed)
        {
            tk.SoLanDangNhapSai++;
            if (tk.SoLanDangNhapSai >= QuyTacNghiepVu.SoLanDangNhapSaiToiDa)
            {
                tk.ThoiDiemHetKhoa = DateTime.Now.AddMinutes(QuyTacNghiepVu.SoPhutKhoaTaiKhoan);
                tk.SoLanDangNhapSai = 0;
                await _db.SaveChangesAsync();
                _log.LogWarning("Tạm khóa tài khoản {TenDangNhap} vì đăng nhập sai quá số lần cho phép.", ten);
                return KetQua<NguoiDungDto>.Loi(
                    $"Nhập sai mật khẩu quá {QuyTacNghiepVu.SoLanDangNhapSaiToiDa} lần. " +
                    $"Tài khoản bị tạm khóa {QuyTacNghiepVu.SoPhutKhoaTaiKhoan} phút.");
            }
            await _db.SaveChangesAsync();
            return KetQua<NguoiDungDto>.Loi("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        var tenVaiTro = tk.VaiTro?.TenVaiTro ?? string.Empty;

        // Tai khoan Khach hang khong duoc dang nhap ung dung quan tri.
        if (laUngDungQuanTri && tenVaiTro == TenVaiTro.KhachHang)
        {
            _log.LogWarning("Tài khoản khách hàng {TenDangNhap} cố đăng nhập ứng dụng quản trị.", ten);
            return KetQua<NguoiDungDto>.Loi("Tài khoản này không có quyền sử dụng ứng dụng quản trị.");
        }

        // Tai khoan nhan vien, quan ly khong dang nhap trang Web ban hang.
        if (!laUngDungQuanTri && tenVaiTro != TenVaiTro.KhachHang)
            return KetQua<NguoiDungDto>.Loi("Tài khoản nội bộ vui lòng dùng ứng dụng quản trị.");

        tk.SoLanDangNhapSai = 0;
        tk.ThoiDiemHetKhoa = null;
        await _db.SaveChangesAsync();

        _log.LogInformation("Tài khoản {TenDangNhap} ({VaiTro}) đăng nhập thành công.", ten, tenVaiTro);
        return KetQua<NguoiDungDto>.Dung(ChuyenDoi(tk), "Đăng nhập thành công.");
    }

    public async Task<KetQua<NguoiDungDto>> DangKyAsync(DangKyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TenDangNhap) || dto.TenDangNhap.Trim().Length < 4)
            return KetQua<NguoiDungDto>.Loi("Tên đăng nhập phải có ít nhất 4 ký tự.");
        if (string.IsNullOrWhiteSpace(dto.HoTen))
            return KetQua<NguoiDungDto>.Loi("Vui lòng nhập họ tên.");
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return KetQua<NguoiDungDto>.Loi("Email không hợp lệ.");
        if (dto.MatKhau.Length < 6)
            return KetQua<NguoiDungDto>.Loi("Mật khẩu phải có ít nhất 6 ký tự.");
        if (dto.MatKhau != dto.XacNhanMatKhau)
            return KetQua<NguoiDungDto>.Loi("Mật khẩu xác nhận không khớp.");

        var ten = dto.TenDangNhap.Trim();
        var email = dto.Email.Trim();

        if (await _db.TaiKhoan.AnyAsync(x => x.TenDangNhap == ten))
            return KetQua<NguoiDungDto>.Loi("Tên đăng nhập đã được sử dụng.");
        if (await _db.TaiKhoan.AnyAsync(x => x.Email == email))
            return KetQua<NguoiDungDto>.Loi("Email đã được sử dụng.");

        var vaiTro = await _db.VaiTro.FirstOrDefaultAsync(x => x.TenVaiTro == TenVaiTro.KhachHang);
        if (vaiTro is null)
            return KetQua<NguoiDungDto>.Loi("Hệ thống chưa khởi tạo vai trò KHÁCH HÀNG.");

        var tk = new TaiKhoan
        {
            TenDangNhap = ten,
            Email = email,
            SoDienThoai = dto.SoDienThoai?.Trim(),
            MaVaiTro = vaiTro.MaVaiTro,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        };
        tk.MatKhauHash = _hasher.HashPassword(tk, dto.MatKhau);
        _db.TaiKhoan.Add(tk);
        await _db.SaveChangesAsync();

        var kh = new KhachHang { MaTaiKhoan = tk.MaTaiKhoan, HoTen = dto.HoTen.Trim() };
        _db.KhachHang.Add(kh);
        await _db.SaveChangesAsync();

        _log.LogInformation("Đã tạo tài khoản khách hàng mới {TenDangNhap} (mã {MaKhachHang}).", ten, kh.MaKhachHang);

        return KetQua<NguoiDungDto>.Dung(new NguoiDungDto
        {
            MaTaiKhoan = tk.MaTaiKhoan,
            TenDangNhap = tk.TenDangNhap,
            Email = tk.Email,
            VaiTro = TenVaiTro.KhachHang,
            HoTen = kh.HoTen,
            MaKhachHang = kh.MaKhachHang
        }, "Đăng ký thành công.");
    }

    public async Task<KetQua> DoiMatKhauAsync(int maTaiKhoan, DoiMatKhauDto dto)
    {
        if (dto.MatKhauMoi.Length < 6)
            return KetQua.Loi("Mật khẩu mới phải có ít nhất 6 ký tự.");
        if (dto.MatKhauMoi != dto.XacNhanMatKhauMoi)
            return KetQua.Loi("Mật khẩu xác nhận không khớp.");

        var tk = await _db.TaiKhoan.FindAsync(maTaiKhoan);
        if (tk is null) return KetQua.Loi("Không tìm thấy tài khoản.");

        if (_hasher.VerifyHashedPassword(tk, tk.MatKhauHash, dto.MatKhauCu) == PasswordVerificationResult.Failed)
            return KetQua.Loi("Mật khẩu hiện tại không đúng.");

        tk.MatKhauHash = _hasher.HashPassword(tk, dto.MatKhauMoi);
        await _db.SaveChangesAsync();
        _log.LogInformation("Tài khoản {MaTaiKhoan} đã đổi mật khẩu.", maTaiKhoan);
        return KetQua.Dung("Đổi mật khẩu thành công.");
    }

    public async Task<NguoiDungDto?> LayNguoiDungAsync(int maTaiKhoan)
    {
        var tk = await _db.TaiKhoan
            .Include(x => x.VaiTro)
            .Include(x => x.KhachHang)
            .Include(x => x.QuanLy)
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
        return tk is null ? null : ChuyenDoi(tk);
    }

    public async Task<DiaChiGiaoHangDto?> LayDiaChiMacDinhAsync(int maKhachHang)
        => await _db.DiaChiGiaoHang
            .AsNoTracking()
            .Where(d => d.MaKhachHang == maKhachHang)
            .OrderByDescending(d => d.LaMacDinh)
            .ThenByDescending(d => d.MaDiaChi)
            .Select(ChieuDiaChi)
            .FirstOrDefaultAsync();

    private static readonly System.Linq.Expressions.Expression<Func<DiaChiGiaoHang, DiaChiGiaoHangDto>> ChieuDiaChi =
        d => new DiaChiGiaoHangDto
        {
            MaDiaChi = d.MaDiaChi,
            HoTenNguoiNhan = d.HoTenNguoiNhan,
            SoDienThoai = d.SoDienThoai,
            DiaChiChiTiet = d.DiaChiChiTiet,
            PhuongXa = d.PhuongXa,
            QuanHuyen = d.QuanHuyen,
            TinhThanh = d.TinhThanh,
            DiaChiDayDu = d.DiaChiChiTiet + ", " + d.PhuongXa + ", " + d.QuanHuyen + ", " + d.TinhThanh,
            LaMacDinh = d.LaMacDinh
        };

    // ---------------- Tai khoan cua toi ----------------

    public const int SoDiaChiToiDa = 10;

    private static bool LaSoDienThoai(string? s)
        => !string.IsNullOrWhiteSpace(s) && System.Text.RegularExpressions.Regex.IsMatch(s.Trim(), @"^0\d{9,10}$");

    public async Task<HoSoKhachHangDto?> LayHoSoAsync(int maKhachHang)
        => await _db.KhachHang.AsNoTracking()
            .Where(k => k.MaKhachHang == maKhachHang)
            .Select(k => new HoSoKhachHangDto
            {
                TenDangNhap = k.TaiKhoan!.TenDangNhap,
                Email = k.TaiKhoan.Email,
                HoTen = k.HoTen,
                SoDienThoai = k.TaiKhoan.SoDienThoai,
                NgaySinh = k.NgaySinh,
                GioiTinh = k.GioiTinh,
                NgayDangKy = k.NgayDangKy
            })
            .FirstOrDefaultAsync();

    public async Task<KetQua> CapNhatHoSoAsync(int maKhachHang, HoSoKhachHangDto dto)
    {
        var hoTen = dto.HoTen?.Trim() ?? string.Empty;
        if (hoTen.Length == 0) return KetQua.Loi("Vui lòng nhập họ tên.");
        if (hoTen.Length > 100) return KetQua.Loi("Họ tên tối đa 100 ký tự.");
        if (!string.IsNullOrWhiteSpace(dto.SoDienThoai) && !LaSoDienThoai(dto.SoDienThoai))
            return KetQua.Loi("Số điện thoại phải gồm 10–11 chữ số và bắt đầu bằng 0.");
        if (dto.NgaySinh.HasValue && (dto.NgaySinh.Value.Date >= DateTime.Today || dto.NgaySinh.Value.Year < 1900))
            return KetQua.Loi("Ngày sinh không hợp lệ.");
        if (dto.GioiTinh is not (null or 0 or 1)) return KetQua.Loi("Giới tính không hợp lệ.");

        var kh = await _db.KhachHang.Include(k => k.TaiKhoan).FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);
        if (kh is null) return KetQua.Loi("Không tìm thấy khách hàng.");

        kh.HoTen = hoTen;
        kh.NgaySinh = dto.NgaySinh?.Date;
        kh.GioiTinh = dto.GioiTinh;
        kh.TaiKhoan!.SoDienThoai = string.IsNullOrWhiteSpace(dto.SoDienThoai) ? null : dto.SoDienThoai.Trim();
        await _db.SaveChangesAsync();
        return KetQua.Dung("Đã cập nhật thông tin cá nhân.");
    }

    public async Task<List<DiaChiGiaoHangDto>> LayDanhSachDiaChiAsync(int maKhachHang)
        => await _db.DiaChiGiaoHang.AsNoTracking()
            .Where(d => d.MaKhachHang == maKhachHang)
            .OrderByDescending(d => d.LaMacDinh).ThenBy(d => d.MaDiaChi)
            .Select(ChieuDiaChi)
            .ToListAsync();

    public async Task<KetQua> LuuDiaChiAsync(int maKhachHang, DiaChiGiaoHangDto dto)
    {
        string T(string? s) => s?.Trim() ?? string.Empty;
        if (T(dto.HoTenNguoiNhan).Length == 0 || T(dto.DiaChiChiTiet).Length == 0 || T(dto.PhuongXa).Length == 0
            || T(dto.QuanHuyen).Length == 0 || T(dto.TinhThanh).Length == 0)
            return KetQua.Loi("Vui lòng nhập đầy đủ người nhận và địa chỉ.");
        if (!LaSoDienThoai(dto.SoDienThoai))
            return KetQua.Loi("Số điện thoại phải gồm 10–11 chữ số và bắt đầu bằng 0.");

        var ds = await _db.DiaChiGiaoHang.Where(d => d.MaKhachHang == maKhachHang).ToListAsync();
        DiaChiGiaoHang dc;
        if (dto.MaDiaChi == 0)
        {
            if (ds.Count >= SoDiaChiToiDa) return KetQua.Loi($"Mỗi tài khoản lưu tối đa {SoDiaChiToiDa} địa chỉ.");
            dc = new DiaChiGiaoHang { MaKhachHang = maKhachHang };
            _db.DiaChiGiaoHang.Add(dc);
        }
        else
        {
            // Chi sua duoc dia chi cua chinh minh
            dc = ds.FirstOrDefault(d => d.MaDiaChi == dto.MaDiaChi)!;
            if (dc is null) return KetQua.Loi("Không tìm thấy địa chỉ.");
        }

        dc.HoTenNguoiNhan = T(dto.HoTenNguoiNhan);
        dc.SoDienThoai = T(dto.SoDienThoai);
        dc.DiaChiChiTiet = T(dto.DiaChiChiTiet);
        dc.PhuongXa = T(dto.PhuongXa);
        dc.QuanHuyen = T(dto.QuanHuyen);
        dc.TinhThanh = T(dto.TinhThanh);

        // Dia chi dau tien luon la mac dinh; chon mac dinh thi bo mac dinh o dia chi khac
        var conMacDinhKhac = ds.Any(d => d != dc && d.LaMacDinh);
        var lamMacDinh = dto.LaMacDinh || !conMacDinhKhac;
        if (lamMacDinh)
            foreach (var d in ds) d.LaMacDinh = false;
        dc.LaMacDinh = lamMacDinh;

        await _db.SaveChangesAsync();
        return KetQua.Dung(dto.MaDiaChi == 0 ? "Đã thêm địa chỉ giao hàng." : "Đã cập nhật địa chỉ giao hàng.");
    }

    public async Task<KetQua> XoaDiaChiAsync(int maKhachHang, int maDiaChi)
    {
        var ds = await _db.DiaChiGiaoHang.Where(d => d.MaKhachHang == maKhachHang).ToListAsync();
        var dc = ds.FirstOrDefault(d => d.MaDiaChi == maDiaChi);
        if (dc is null) return KetQua.Loi("Không tìm thấy địa chỉ.");

        _db.DiaChiGiaoHang.Remove(dc);
        // Xoa dia chi mac dinh thi dia chi con lai dau tien tro thanh mac dinh
        if (dc.LaMacDinh && ds.FirstOrDefault(d => d != dc) is { } thayThe)
            thayThe.LaMacDinh = true;
        await _db.SaveChangesAsync();
        return KetQua.Dung("Đã xóa địa chỉ giao hàng.");
    }

    public async Task<KetQua> DatDiaChiMacDinhAsync(int maKhachHang, int maDiaChi)
    {
        var ds = await _db.DiaChiGiaoHang.Where(d => d.MaKhachHang == maKhachHang).ToListAsync();
        if (ds.All(d => d.MaDiaChi != maDiaChi)) return KetQua.Loi("Không tìm thấy địa chỉ.");
        foreach (var d in ds) d.LaMacDinh = d.MaDiaChi == maDiaChi;
        await _db.SaveChangesAsync();
        return KetQua.Dung("Đã đặt địa chỉ mặc định.");
    }

    /// <summary>
    /// Quen mat khau (use case mo rong cua Dang nhap trang Web). Token ngau nhien 32 byte
    /// chi duoc gui qua email; CSDL chi luu ban bam SHA-256 cua token nen du lo CSDL cung
    /// khong dung duoc token. Thong bao tra ve giong nhau du email co ton tai hay khong,
    /// tranh bi do tim email da dang ky.
    /// </summary>
    public async Task<KetQua> YeuCauDatLaiMatKhauAsync(string email, Func<string, string> taoLienKet)
    {
        const string thongBaoChung = "Nếu email đã đăng ký, một liên kết đặt lại mật khẩu đã được gửi tới hộp thư của bạn.";

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return KetQua.Loi("Vui lòng nhập email hợp lệ.");

        var e = email.Trim();
        var tk = await _db.TaiKhoan.Include(x => x.KhachHang).Include(x => x.VaiTro)
            .FirstOrDefaultAsync(x => x.Email == e);

        if (tk is null || tk.TrangThai == TrangThaiTaiKhoan.BiKhoa || tk.VaiTro?.TenVaiTro != TenVaiTro.KhachHang)
        {
            _log.LogInformation("Yêu cầu đặt lại mật khẩu cho email {Email} không hợp lệ hoặc không phải khách hàng.", e);
            return KetQua.Dung(thongBaoChung);
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        tk.TokenDatLaiMatKhau = BamToken(token);
        tk.HanToken = DateTime.Now.AddMinutes(SoPhutHieuLucToken);
        await _db.SaveChangesAsync();

        var (tieuDe, noiDung) = MauEmail.DatLaiMatKhau(tk.KhachHang?.HoTen ?? tk.TenDangNhap, taoLienKet(token), SoPhutHieuLucToken);
        await _email.GuiAsync(tk.Email, tieuDe, noiDung);

        _log.LogInformation("Đã tạo liên kết đặt lại mật khẩu cho tài khoản {TenDangNhap}.", tk.TenDangNhap);
        return KetQua.Dung(thongBaoChung);
    }

    public async Task<KetQua> DatLaiMatKhauAsync(string token, string matKhauMoi, string xacNhanMatKhau)
    {
        if (string.IsNullOrWhiteSpace(token)) return KetQua.Loi("Liên kết đặt lại mật khẩu không hợp lệ.");
        if (matKhauMoi.Length < 6) return KetQua.Loi("Mật khẩu mới phải có ít nhất 6 ký tự.");
        if (matKhauMoi != xacNhanMatKhau) return KetQua.Loi("Mật khẩu xác nhận không khớp.");

        var bam = BamToken(token.Trim());
        var tk = await _db.TaiKhoan.FirstOrDefaultAsync(x => x.TokenDatLaiMatKhau == bam);

        if (tk is null || tk.HanToken is null || tk.HanToken < DateTime.Now)
            return KetQua.Loi("Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại.");

        tk.MatKhauHash = _hasher.HashPassword(tk, matKhauMoi);
        tk.TokenDatLaiMatKhau = null;   // token chi dung duoc mot lan
        tk.HanToken = null;
        tk.SoLanDangNhapSai = 0;
        tk.ThoiDiemHetKhoa = null;
        await _db.SaveChangesAsync();

        _log.LogInformation("Tài khoản {TenDangNhap} đã đặt lại mật khẩu qua email.", tk.TenDangNhap);
        return KetQua.Dung("Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.");
    }

    private static string BamToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static NguoiDungDto ChuyenDoi(TaiKhoan tk) => new()
    {
        MaTaiKhoan = tk.MaTaiKhoan,
        TenDangNhap = tk.TenDangNhap,
        Email = tk.Email,
        VaiTro = tk.VaiTro?.TenVaiTro ?? string.Empty,
        HoTen = tk.KhachHang?.HoTen ?? tk.QuanLy?.HoTen ?? tk.TenDangNhap,
        MaKhachHang = tk.KhachHang?.MaKhachHang,
        MaQuanLy = tk.QuanLy?.MaQuanLy
    };
}
