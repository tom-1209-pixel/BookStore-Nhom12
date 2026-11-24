using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BookStore.Admin.Models;
using Microsoft.Extensions.Logging;

namespace BookStore.Admin.Services;

/// <summary>Luu token JWT va thong tin nguoi dung cua phien lam viec hien tai.</summary>
public class PhienDangNhap
{
    public string? Token { get; private set; }
    public DateTime HetHan { get; private set; }
    public NguoiDung? NguoiDung { get; private set; }

    public bool DaDangNhap => !string.IsNullOrEmpty(Token) && HetHan > DateTime.UtcNow;

    public void Luu(KetQuaDangNhap kq)
    {
        Token = kq.Token;
        HetHan = kq.HetHan;
        NguoiDung = kq.NguoiDung;
    }

    public void Xoa()
    {
        Token = null;
        HetHan = default;
        NguoiDung = null;
    }
}

/// <summary>Ket qua mot loi goi API: thanh cong kem du lieu, hoac that bai kem thong bao.</summary>
public class KetQuaApi<T>
{
    public bool ThanhCong { get; init; }
    public string ThongBao { get; init; } = string.Empty;
    public T? DuLieu { get; init; }

    public static KetQuaApi<T> Dung(T duLieu, string thongBao = "") => new()
    { ThanhCong = true, DuLieu = duLieu, ThongBao = thongBao };

    public static KetQuaApi<T> Loi(string thongBao) => new() { ThanhCong = false, ThongBao = thongBao };
}

/// <summary>
/// Lop goi Web API. Tat ca man hinh cua ung dung quan tri deu di qua lop nay,
/// nho vay viec gan token, bat loi mang va doc thong bao loi chi viet mot lan.
/// </summary>
public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly PhienDangNhap _phien;
    private readonly ILogger<ApiClient> _log;

    public ApiClient(HttpClient http, PhienDangNhap phien, ILogger<ApiClient> log)
    {
        _http = http;
        _phien = phien;
        _log = log;
    }

    public async Task<KetQuaApi<KetQuaDangNhap>> DangNhapAsync(string tenDangNhap, string matKhau)
    {
        var kq = await GoiAsync<KetQuaDangNhap>(HttpMethod.Post, "api/xacthuc/dangnhap",
            new { tenDangNhap, matKhau }, canToken: false);

        if (kq.ThanhCong && kq.DuLieu is not null)
        {
            _phien.Luu(kq.DuLieu);
            _log.LogInformation("Đăng nhập ứng dụng quản trị thành công với tài khoản {Ten}.", tenDangNhap);
        }

        return kq;
    }

    public Task<KetQuaApi<ThongKe>> LayThongKeAsync()
        => GoiAsync<ThongKe>(HttpMethod.Get, "api/baocao/thongke");

    public Task<KetQuaApi<TrangKetQua<DonHang>>> TimDonHangAsync(
        string? tuKhoa, byte? trangThai, int trang = 1, int soDongMoiTrang = 20)
    {
        var tham = new List<string> { $"trang={trang}", $"soDongMoiTrang={soDongMoiTrang}" };
        if (!string.IsNullOrWhiteSpace(tuKhoa)) tham.Add($"tuKhoa={Uri.EscapeDataString(tuKhoa)}");
        if (trangThai.HasValue) tham.Add($"trangThai={trangThai}");
        return GoiAsync<TrangKetQua<DonHang>>(HttpMethod.Get, "api/donhang?" + string.Join("&", tham));
    }

    public Task<KetQuaApi<DonHang>> LayDonHangAsync(int maDonHang)
        => GoiAsync<DonHang>(HttpMethod.Get, $"api/donhang/{maDonHang}");

    public Task<KetQuaApi<PhanHoi>> ChuyenTrangThaiDonAsync(int maDonHang, byte trangThaiMoi, string? ghiChu)
        => GoiAsync<PhanHoi>(HttpMethod.Put, $"api/donhang/{maDonHang}/trangthai",
            new { trangThaiMoi, ghiChu });

    public Task<KetQuaApi<TrangKetQua<Sach>>> TimSachAsync(
        string? tuKhoa, int? maLoaiSach, int trang = 1, int soDongMoiTrang = 20)
    {
        var tham = new List<string> { $"trang={trang}", $"soDongMoiTrang={soDongMoiTrang}" };
        if (!string.IsNullOrWhiteSpace(tuKhoa)) tham.Add($"tuKhoa={Uri.EscapeDataString(tuKhoa)}");
        if (maLoaiSach.HasValue) tham.Add($"maLoaiSach={maLoaiSach}");
        return GoiAsync<TrangKetQua<Sach>>(HttpMethod.Get, "api/sach?" + string.Join("&", tham));
    }

    public Task<KetQuaApi<List<LoaiSach>>> LayDanhMucAsync()
        => GoiAsync<List<LoaiSach>>(HttpMethod.Get, "api/sach/danhmuc");

    public Task<KetQuaApi<PhanHoi>> ThemSachAsync(object sach)
        => GoiAsync<PhanHoi>(HttpMethod.Post, "api/sach", sach);

    public Task<KetQuaApi<PhanHoi>> CapNhatSachAsync(int maSach, object sach)
        => GoiAsync<PhanHoi>(HttpMethod.Put, $"api/sach/{maSach}", sach);

    public Task<KetQuaApi<PhanHoi>> XoaSachAsync(int maSach)
        => GoiAsync<PhanHoi>(HttpMethod.Delete, $"api/sach/{maSach}");

    /// <summary>Tai anh bia len Web API, tra ve duong dan anh de luu vao sach.</summary>
    public async Task<KetQuaApi<string>> TaiAnhBiaAsync(string tenTep, byte[] noiDung)
    {
        try
        {
            if (!_phien.DaDangNhap) return KetQuaApi<string>.Loi("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");

            using var form = new MultipartFormDataContent();
            var tep = new ByteArrayContent(noiDung);
            tep.Headers.ContentType = new MediaTypeHeaderValue(Path.GetExtension(tenTep).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            });
            form.Add(tep, "tep", tenTep);

            using var req = new HttpRequestMessage(HttpMethod.Post, "api/sach/anhbia") { Content = form };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _phien.Token);
            using var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return KetQuaApi<string>.Loi(await DocThongBaoLoiAsync(res));

            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            return KetQuaApi<string>.Dung(doc.RootElement.GetProperty("duongDan").GetString() ?? string.Empty);
        }
        catch (HttpRequestException ex)
        {
            _log.LogError(ex, "Không tải được ảnh bìa lên.");
            return KetQuaApi<string>.Loi("Không kết nối được máy chủ. Kiểm tra Web API đã chạy chưa.");
        }
    }

    // ---------------- Danh muc (loai: loaisach | tacgia | nhaxuatban | nhacungcap) ----------------

    public Task<KetQuaApi<List<DanhMuc>>> LayDanhMucChungAsync(string loai, string? tuKhoa = null)
        => GoiAsync<List<DanhMuc>>(HttpMethod.Get, $"api/danhmuc/{loai}"
            + (string.IsNullOrWhiteSpace(tuKhoa) ? "" : $"?tuKhoa={Uri.EscapeDataString(tuKhoa)}"));

    public Task<KetQuaApi<PhanHoiCoMa>> LuuDanhMucAsync(string loai, DanhMuc muc)
        => muc.Ma > 0
            ? GoiAsync<PhanHoiCoMa>(HttpMethod.Put, $"api/danhmuc/{loai}/{muc.Ma}", muc)
            : GoiAsync<PhanHoiCoMa>(HttpMethod.Post, $"api/danhmuc/{loai}", muc);

    public Task<KetQuaApi<PhanHoi>> XoaDanhMucAsync(string loai, int ma)
        => GoiAsync<PhanHoi>(HttpMethod.Delete, $"api/danhmuc/{loai}/{ma}");

    // ---------------- Kho va phieu nhap ----------------

    public Task<KetQuaApi<List<TonKho>>> LayTonKhoAsync(string? tuKhoa, bool chiSapHetHang)
        => GoiAsync<List<TonKho>>(HttpMethod.Get, $"api/kho/tonkho?chiSapHetHang={chiSapHetHang.ToString().ToLowerInvariant()}"
            + (string.IsNullOrWhiteSpace(tuKhoa) ? "" : $"&tuKhoa={Uri.EscapeDataString(tuKhoa)}"));

    public Task<KetQuaApi<PhanHoi>> CapNhatNguongAsync(int maSach, int nguong)
        => GoiAsync<PhanHoi>(HttpMethod.Put, $"api/kho/tonkho/{maSach}/nguong", new { nguong });

    public Task<KetQuaApi<List<PhieuNhap>>> LayPhieuNhapAsync(byte? trangThai)
        => GoiAsync<List<PhieuNhap>>(HttpMethod.Get, "api/kho/phieunhap" + (trangThai.HasValue ? $"?trangThai={trangThai}" : ""));

    public Task<KetQuaApi<PhieuNhap>> LayChiTietPhieuNhapAsync(int ma)
        => GoiAsync<PhieuNhap>(HttpMethod.Get, $"api/kho/phieunhap/{ma}");

    public Task<KetQuaApi<PhanHoiCoMa>> LuuPhieuNhapAsync(int ma, object phieu)
        => ma > 0
            ? GoiAsync<PhanHoiCoMa>(HttpMethod.Put, $"api/kho/phieunhap/{ma}", phieu)
            : GoiAsync<PhanHoiCoMa>(HttpMethod.Post, "api/kho/phieunhap", phieu);

    public Task<KetQuaApi<PhanHoi>> HoanThanhPhieuNhapAsync(int ma)
        => GoiAsync<PhanHoi>(HttpMethod.Post, $"api/kho/phieunhap/{ma}/hoanthanh", new { });

    public Task<KetQuaApi<PhanHoi>> XoaPhieuNhapAsync(int ma)
        => GoiAsync<PhanHoi>(HttpMethod.Delete, $"api/kho/phieunhap/{ma}");

    // ---------------- Khach hang ----------------

    public Task<KetQuaApi<List<KhachHang>>> TimKhachHangAsync(string? tuKhoa, byte? trangThai)
    {
        var tham = new List<string>();
        if (!string.IsNullOrWhiteSpace(tuKhoa)) tham.Add($"tuKhoa={Uri.EscapeDataString(tuKhoa)}");
        if (trangThai.HasValue) tham.Add($"trangThai={trangThai}");
        return GoiAsync<List<KhachHang>>(HttpMethod.Get, "api/khachhang" + (tham.Count > 0 ? "?" + string.Join("&", tham) : ""));
    }

    public Task<KetQuaApi<KhachHang>> LayKhachHangAsync(int ma)
        => GoiAsync<KhachHang>(HttpMethod.Get, $"api/khachhang/{ma}");

    public Task<KetQuaApi<PhanHoi>> DoiTrangThaiKhachHangAsync(int ma, bool khoa, string? lyDo)
        => GoiAsync<PhanHoi>(HttpMethod.Put, $"api/khachhang/{ma}/trangthai", new { khoa, lyDo });

    // ---------------- Bao cao ----------------

    private static string Ky(DateTime tu, DateTime den) => $"tuNgay={tu:yyyy-MM-dd}&denNgay={den:yyyy-MM-dd}";

    public Task<KetQuaApi<BaoCaoDoanhThu>> LayBaoCaoDoanhThuAsync(DateTime tu, DateTime den)
        => GoiAsync<BaoCaoDoanhThu>(HttpMethod.Get, "api/baocao/doanhthu/chitiet?" + Ky(tu, den));

    public Task<KetQuaApi<List<NhapXuatTon>>> LayNhapXuatTonAsync(DateTime tu, DateTime den)
        => GoiAsync<List<NhapXuatTon>>(HttpMethod.Get, "api/baocao/nhapxuatton?" + Ky(tu, den));

    public Task<KetQuaApi<List<DonTheoTrangThai>>> LayDonTheoTrangThaiAsync(DateTime tu, DateTime den)
        => GoiAsync<List<DonTheoTrangThai>>(HttpMethod.Get, "api/baocao/dontheotrangthai?" + Ky(tu, den));

    /// <summary>Tai tap tin Excel tu API ve mang byte (loai: doanhthu | nhapxuatton).</summary>
    public async Task<KetQuaApi<byte[]>> TaiExcelAsync(string loai, DateTime tu, DateTime den)
    {
        try
        {
            if (!_phien.DaDangNhap) return KetQuaApi<byte[]>.Loi("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            using var req = new HttpRequestMessage(HttpMethod.Get, $"api/baocao/xuatexcel/{loai}?" + Ky(tu, den));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _phien.Token);
            using var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return KetQuaApi<byte[]>.Loi(await DocThongBaoLoiAsync(res));
            return KetQuaApi<byte[]>.Dung(await res.Content.ReadAsByteArrayAsync());
        }
        catch (HttpRequestException ex)
        {
            _log.LogError(ex, "Không tải được tập tin Excel.");
            return KetQuaApi<byte[]>.Loi("Không kết nối được máy chủ. Kiểm tra Web API đã chạy chưa.");
        }
    }

    private async Task<KetQuaApi<T>> GoiAsync<T>(
        HttpMethod phuongThuc, string duongDan, object? noiDung = null, bool canToken = true)
    {
        try
        {
            using var req = new HttpRequestMessage(phuongThuc, duongDan);

            if (canToken)
            {
                if (!_phien.DaDangNhap)
                    return KetQuaApi<T>.Loi("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _phien.Token);
            }

            if (noiDung is not null)
                req.Content = JsonContent.Create(noiDung, options: JsonOptions);

            using var res = await _http.SendAsync(req);

            if (res.IsSuccessStatusCode)
            {
                if (res.StatusCode == HttpStatusCode.NoContent)
                    return KetQuaApi<T>.Dung(default!);

                var duLieu = await res.Content.ReadFromJsonAsync<T>(JsonOptions);
                return KetQuaApi<T>.Dung(duLieu!);
            }

            var thongBao = await DocThongBaoLoiAsync(res);
            _log.LogWarning("API {PhuongThuc} {DuongDan} trả về {Ma}: {ThongBao}",
                phuongThuc, duongDan, (int)res.StatusCode, thongBao);
            return KetQuaApi<T>.Loi(thongBao);
        }
        catch (HttpRequestException ex)
        {
            _log.LogError(ex, "Không kết nối được Web API khi gọi {DuongDan}.", duongDan);
            return KetQuaApi<T>.Loi("Không kết nối được máy chủ. Kiểm tra Web API đã chạy chưa.");
        }
        catch (TaskCanceledException)
        {
            return KetQuaApi<T>.Loi("Máy chủ phản hồi quá chậm. Vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Lỗi không xác định khi gọi {DuongDan}.", duongDan);
            return KetQuaApi<T>.Loi("Đã xảy ra lỗi không mong muốn: " + ex.Message);
        }
    }

    private static async Task<string> DocThongBaoLoiAsync(HttpResponseMessage res)
    {
        if (res.StatusCode == HttpStatusCode.Unauthorized)
            return "Tên đăng nhập hoặc mật khẩu không đúng, hoặc phiên đã hết hạn.";
        if (res.StatusCode == HttpStatusCode.Forbidden)
            return "Tài khoản của bạn không có quyền thực hiện thao tác này.";

        try
        {
            var chuoi = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(chuoi)) return $"Máy chủ trả về mã lỗi {(int)res.StatusCode}.";

            using var doc = JsonDocument.Parse(chuoi);
            if (doc.RootElement.TryGetProperty("thongBao", out var tb))
                return tb.GetString() ?? chuoi;
            if (doc.RootElement.TryGetProperty("detail", out var d))
                return d.GetString() ?? chuoi;
            return chuoi;
        }
        catch
        {
            return $"Máy chủ trả về mã lỗi {(int)res.StatusCode}.";
        }
    }
}
