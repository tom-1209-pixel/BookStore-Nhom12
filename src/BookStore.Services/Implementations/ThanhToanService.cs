using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookStore.Services.Implementations;

/// <summary>
/// Cau hinh VNPay, doc tu muc "VnPay". TmnCode va HashSecret la thong tin bi mat cua
/// merchant nen dat trong user-secrets, khong dua len GitHub.
/// </summary>
public class CauHinhVnPay
{
    public string? TmnCode { get; set; }
    public string? HashSecret { get; set; }
    public string DiaChiCong { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    /// <summary>Khoa ky dung cho cong gia lap noi bo khi chua co TmnCode / HashSecret that.</summary>
    public string KhoaGiaLap { get; set; } = "BookStore-Nhom12-CongThanhToanGiaLap-ChiDungDeDemo";

    public bool DaCauHinh => !string.IsNullOrWhiteSpace(TmnCode) && !string.IsNullOrWhiteSpace(HashSecret);
}

/// <summary>
/// Cac ham ky va kiem tra chu ky theo dac ta VNPay 2.1.0: sap xep tham so vnp_* theo
/// ten, ma hoa URL tung cap khoa = gia tri, noi bang dau &amp; roi bam HMAC-SHA512.
/// </summary>
public static class VnPayChuKy
{
    public static string TaoChuoiTruyVan(IEnumerable<KeyValuePair<string, string>> thamSo)
        => string.Join("&", thamSo
            .Where(p => p.Key.StartsWith("vnp_", StringComparison.Ordinal)
                        && p.Key is not ("vnp_SecureHash" or "vnp_SecureHashType")
                        && !string.IsNullOrEmpty(p.Value))
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => WebUtility.UrlEncode(p.Key) + "=" + WebUtility.UrlEncode(p.Value)));

    public static string Bam(string khoa, string duLieu)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(khoa));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(duLieu))).ToLowerInvariant();
    }

    public static string Ky(IEnumerable<KeyValuePair<string, string>> thamSo, string khoa)
        => Bam(khoa, TaoChuoiTruyVan(thamSo));

    public static bool KiemTra(IReadOnlyDictionary<string, string> thamSo, string khoa)
    {
        if (!thamSo.TryGetValue("vnp_SecureHash", out var chuKy) || string.IsNullOrEmpty(chuKy)) return false;
        var tinhLai = Ky(thamSo, khoa);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(tinhLai), Encoding.ASCII.GetBytes(chuKy.ToLowerInvariant()));
    }

    public static string MoTaMaPhanHoi(string? ma) => ma switch
    {
        "00" => "Giao dịch thành công",
        "07" => "Trừ tiền thành công, giao dịch bị nghi ngờ",
        "09" => "Thẻ/tài khoản chưa đăng ký dịch vụ InternetBanking",
        "10" => "Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
        "11" => "Đã hết hạn chờ thanh toán",
        "12" => "Thẻ/tài khoản bị khóa",
        "13" => "Nhập sai mật khẩu xác thực giao dịch (OTP)",
        "24" => "Khách hàng hủy giao dịch",
        "51" => "Tài khoản không đủ số dư",
        "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày",
        "75" => "Ngân hàng thanh toán đang bảo trì",
        "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định",
        _ => "Giao dịch không thành công" + (string.IsNullOrEmpty(ma) ? "" : $" (mã {ma})")
    };
}

/// <summary>
/// Thanh toan truc tuyen qua VNPay. Khi da cau hinh TmnCode / HashSecret thi chuyen khach
/// sang cong sandbox that; neu chua thi chuyen sang cong gia lap noi bo, nhung van di
/// qua dung cac buoc ky va kiem tra chu ky nhu cong that.
/// </summary>
public class ThanhToanService : IThanhToanService
{
    private readonly BookStoreDbContext _db;
    private readonly IDonHangService _donHang;
    private readonly CauHinhVnPay _cauHinh;
    private readonly ILogger<ThanhToanService> _log;

    public ThanhToanService(BookStoreDbContext db, IDonHangService donHang,
        IOptions<CauHinhVnPay> cauHinh, ILogger<ThanhToanService> log)
    {
        _db = db;
        _donHang = donHang;
        _cauHinh = cauHinh.Value;
        _log = log;
    }

    public bool DungCongThat => _cauHinh.DaCauHinh;

    private string KhoaKy => _cauHinh.DaCauHinh ? _cauHinh.HashSecret! : _cauHinh.KhoaGiaLap;

    public async Task<KetQua<string>> TaoYeuCauThanhToanAsync(
        int maDonHang, int maKhachHang, string ipKhach, string urlTraVe, string urlCongGiaLap)
    {
        var don = await _db.DonHang.Include(d => d.ThanhToans)
            .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang && d.MaKhachHang == maKhachHang);

        if (don is null) return KetQua<string>.Loi("Không tìm thấy đơn hàng của bạn.");
        if (don.HinhThucThanhToan != HinhThucThanhToan.VNPay)
            return KetQua<string>.Loi("Đơn hàng này thanh toán khi nhận hàng, không cần thanh toán trực tuyến.");
        if (don.TrangThai != TrangThaiDonHang.ChoThanhToan)
            return KetQua<string>.Loi($"Đơn hàng đang ở trạng thái {Dtos.DonHangDto.MoTaTrangThai(don.TrangThai)}, không thể thanh toán.");

        var hetHan = don.NgayDat.AddMinutes(QuyTacNghiepVu.SoPhutHetHanThanhToan);
        if (hetHan <= DateTime.Now)
            return KetQua<string>.Loi("Đơn hàng đã quá hạn thanh toán và sẽ bị hủy.", "BR05");

        // Lan thanh toan truoc that bai thi mo mot ban ghi thanh toan moi o trang thai Cho
        if (!don.ThanhToans.Any(t => t.TrangThai == TrangThaiThanhToan.Cho))
        {
            _db.ThanhToan.Add(new ThanhToan
            {
                MaDonHang = don.MaDonHang,
                PhuongThuc = HinhThucThanhToan.VNPay,
                SoTien = don.TongTien,
                TrangThai = TrangThaiThanhToan.Cho,
                NoiDung = "Khách thanh toán lại qua VNPay"
            });
            await _db.SaveChangesAsync();
        }

        var bayGio = GioVietNam(DateTime.Now);
        var thamSo = new Dictionary<string, string>
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _cauHinh.DaCauHinh ? _cauHinh.TmnCode! : "GIALAP01",
            ["vnp_Amount"] = ((long)(don.TongTien * 100)).ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = bayGio.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = GioVietNam(hetHan).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(ipKhach) ? "127.0.0.1" : ipKhach,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don hang {don.MaDonHang}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = urlTraVe,
            // Ma tham chieu phai duy nhat cho moi lan thanh toan: ma don + thoi diem
            ["vnp_TxnRef"] = $"{don.MaDonHang}-{bayGio:HHmmss}"
        };

        var truyVan = VnPayChuKy.TaoChuoiTruyVan(thamSo);
        var chuKy = VnPayChuKy.Bam(KhoaKy, truyVan);
        var goc = _cauHinh.DaCauHinh ? _cauHinh.DiaChiCong : urlCongGiaLap;
        var url = $"{goc}?{truyVan}&vnp_SecureHash={chuKy}";

        _log.LogInformation("Tạo yêu cầu thanh toán đơn {MaDonHang} qua {Cong}.", maDonHang,
            _cauHinh.DaCauHinh ? "VNPay sandbox" : "cổng giả lập");
        return KetQua<string>.Dung(url);
    }

    public async Task<KetQua<int>> XuLyKetQuaTraVeAsync(IReadOnlyDictionary<string, string> thamSo)
    {
        if (!VnPayChuKy.KiemTra(thamSo, KhoaKy))
        {
            _log.LogWarning("Kết quả thanh toán trả về có chữ ký không hợp lệ: {TxnRef}.",
                thamSo.GetValueOrDefault("vnp_TxnRef"));
            return KetQua<int>.Loi("Chữ ký kết quả thanh toán không hợp lệ, giao dịch không được ghi nhận.");
        }

        var txnRef = thamSo.GetValueOrDefault("vnp_TxnRef") ?? string.Empty;
        if (!int.TryParse(txnRef.Split('-')[0], out var maDon))
            return KetQua<int>.Loi("Mã tham chiếu giao dịch không hợp lệ.");

        var don = await _db.DonHang.Include(d => d.ThanhToans).FirstOrDefaultAsync(d => d.MaDonHang == maDon);
        if (don is null) return KetQua<int>.Loi("Không tìm thấy đơn hàng của giao dịch.");

        if (!long.TryParse(thamSo.GetValueOrDefault("vnp_Amount"), out var soTien) || soTien != (long)(don.TongTien * 100))
            return KetQua<int>.Loi("Số tiền thanh toán không khớp với đơn hàng.");

        var maGd = thamSo.GetValueOrDefault("vnp_TransactionNo");
        var maPhanHoi = thamSo.GetValueOrDefault("vnp_ResponseCode");
        var thanhCong = maPhanHoi == "00" && thamSo.GetValueOrDefault("vnp_TransactionStatus") == "00";

        // Cung mot ket qua co the bi gui lai nhieu lan (tai lai trang): chi ghi nhan mot lan
        if (don.ThanhToans.Any(t => t.TrangThai == TrangThaiThanhToan.ThanhCong))
            return KetQua<int>.Dung(maDon, "Đơn hàng đã được thanh toán trước đó.");

        if (don.TrangThai != TrangThaiDonHang.ChoThanhToan)
            return KetQua<int>.Loi($"Đơn hàng đang ở trạng thái {Dtos.DonHangDto.MoTaTrangThai(don.TrangThai)} nên không ghi nhận thanh toán.");

        var banGhi = don.ThanhToans.Where(t => t.TrangThai == TrangThaiThanhToan.Cho)
                         .OrderByDescending(t => t.MaThanhToan).FirstOrDefault()
                     ?? new ThanhToan { MaDonHang = maDon, PhuongThuc = HinhThucThanhToan.VNPay, SoTien = don.TongTien };
        if (banGhi.MaThanhToan == 0) _db.ThanhToan.Add(banGhi);

        banGhi.MaGiaoDich = maGd;
        banGhi.ThoiDiem = DateTime.Now;

        if (!thanhCong)
        {
            banGhi.TrangThai = TrangThaiThanhToan.ThatBai;
            banGhi.NoiDung = VnPayChuKy.MoTaMaPhanHoi(maPhanHoi);
            await _db.SaveChangesAsync();
            _log.LogInformation("Thanh toán đơn {MaDonHang} không thành công: {MoTa}.", maDon, banGhi.NoiDung);
            return KetQua<int>.Loi($"Thanh toán không thành công: {banGhi.NoiDung}. Bạn có thể thanh toán lại trước khi đơn hết hạn.");
        }

        banGhi.TrangThai = TrangThaiThanhToan.ThanhCong;
        banGhi.NoiDung = $"Thanh toán VNPay thành công, ngân hàng {thamSo.GetValueOrDefault("vnp_BankCode")}";
        await _db.SaveChangesAsync();

        var kq = await _donHang.ChuyenTrangThaiAsync(maDon, TrangThaiDonHang.ChoXacNhan,
            "Cổng thanh toán VNPay", null, $"Đã thanh toán qua VNPay, mã giao dịch {maGd}");
        if (!kq.ThanhCong) return KetQua<int>.Loi(kq.ThongBao);

        _log.LogInformation("Đơn {MaDonHang} thanh toán VNPay thành công, mã giao dịch {MaGd}.", maDon, maGd);
        return KetQua<int>.Dung(maDon, $"Thanh toán thành công. Đơn hàng #{maDon} đang chờ nhà sách xác nhận.");
    }

    public bool KiemTraYeuCauGiaLap(IReadOnlyDictionary<string, string> thamSoYeuCau)
        => !_cauHinh.DaCauHinh && VnPayChuKy.KiemTra(thamSoYeuCau, _cauHinh.KhoaGiaLap);

    public string KyBuocGiaLap(string duLieu) => VnPayChuKy.Bam(_cauHinh.KhoaGiaLap, "buoc|" + duLieu);

    public KetQua<string> TaoKetQuaGiaLap(IReadOnlyDictionary<string, string> thamSoYeuCau, bool thanhCong,
        string? maPhanHoi = null, string nganHang = "NCB", string loaiThe = "ATM")
    {
        maPhanHoi ??= thanhCong ? "00" : "24";
        if (_cauHinh.DaCauHinh)
            return KetQua<string>.Loi("Hệ thống đang dùng cổng VNPay thật, cổng giả lập đã bị tắt.");
        if (!VnPayChuKy.KiemTra(thamSoYeuCau, _cauHinh.KhoaGiaLap))
            return KetQua<string>.Loi("Yêu cầu thanh toán có chữ ký không hợp lệ.");

        var urlTraVe = thamSoYeuCau.GetValueOrDefault("vnp_ReturnUrl");
        if (string.IsNullOrWhiteSpace(urlTraVe)) return KetQua<string>.Loi("Thiếu địa chỉ trả về.");

        var ketQua = new Dictionary<string, string>
        {
            ["vnp_Amount"] = thamSoYeuCau.GetValueOrDefault("vnp_Amount") ?? "0",
            ["vnp_BankCode"] = nganHang,
            ["vnp_BankTranNo"] = thanhCong ? $"VNP{Random.Shared.Next(10_000_000, 99_999_999)}" : "",
            ["vnp_CardType"] = loaiThe,
            ["vnp_OrderInfo"] = thamSoYeuCau.GetValueOrDefault("vnp_OrderInfo") ?? "",
            ["vnp_PayDate"] = GioVietNam(DateTime.Now).ToString("yyyyMMddHHmmss"),
            ["vnp_ResponseCode"] = thanhCong ? "00" : maPhanHoi,
            ["vnp_TmnCode"] = thamSoYeuCau.GetValueOrDefault("vnp_TmnCode") ?? "",
            ["vnp_TransactionNo"] = thanhCong ? Random.Shared.Next(10_000_000, 99_999_999).ToString() : "0",
            ["vnp_TransactionStatus"] = thanhCong ? "00" : "02",
            ["vnp_TxnRef"] = thamSoYeuCau.GetValueOrDefault("vnp_TxnRef") ?? ""
        };
        var truyVan = VnPayChuKy.TaoChuoiTruyVan(ketQua);
        var chuKy = VnPayChuKy.Bam(_cauHinh.KhoaGiaLap, truyVan);
        return KetQua<string>.Dung($"{urlTraVe}?{truyVan}&vnp_SecureHash={chuKy}");
    }

    /// <summary>VNPay yeu cau thoi gian theo mui gio GMT+7.</summary>
    private static DateTime GioVietNam(DateTime gioMay)
    {
        foreach (var id in new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh", "Asia/Bangkok" })
        {
            try { return TimeZoneInfo.ConvertTime(gioMay, TimeZoneInfo.FindSystemTimeZoneById(id)); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return gioMay.ToUniversalTime().AddHours(7);
    }
}
