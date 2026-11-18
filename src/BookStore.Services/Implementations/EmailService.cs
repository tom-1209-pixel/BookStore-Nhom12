using BookStore.Services.Common;
using BookStore.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BookStore.Services.Implementations;

/// <summary>Cau hinh gui email, doc tu muc "Email" trong appsettings hoac user-secrets.</summary>
public class CauHinhEmail
{
    /// <summary>De trong thi khong gui that ma ghi thu ra tap tin .eml (che do du phong).</summary>
    public string? MayChuSmtp { get; set; }
    public int Cong { get; set; } = 587;
    public string? TenDangNhap { get; set; }
    public string? MatKhau { get; set; }
    public string DiaChiGui { get; set; } = "no-reply@bookstore.local";
    public string TenNguoiGui { get; set; } = "Nhà sách trực tuyến";

    /// <summary>Thu muc luu thu khi khong co SMTP. Mac dinh: Documents\BookStore\EmailGuiDi.</summary>
    public string? ThuMucLuu { get; set; }
}

/// <summary>
/// Gui email bang MailKit. Neu chua cau hinh may chu SMTP (vi du khi demo tren may la),
/// thu duoc ghi ra tap tin .eml de van xem duoc noi dung, va nghiep vu khong bao gio
/// that bai chi vi khong gui duoc email.
/// </summary>
public class EmailService : IEmailService
{
    private readonly CauHinhEmail _cauHinh;
    private readonly ILogger<EmailService> _log;

    public EmailService(IOptions<CauHinhEmail> cauHinh, ILogger<EmailService> log)
    {
        _cauHinh = cauHinh.Value;
        _log = log;
    }

    public bool DaCauHinhSmtp => !string.IsNullOrWhiteSpace(_cauHinh.MayChuSmtp);

    /// <summary>
    /// Thu muc luu thu khi khong co SMTP. Uu tien Documents\BookStore\EmailGuiDi; neu khong tao
    /// duoc (vi du Documents bi OneDrive chuyen sang duong dan khong ton tai) thi dung thu muc
    /// EmailGuiDi canh tap tin chay cua ung dung.
    /// </summary>
    public string ThuMucLuu
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_cauHinh.ThuMucLuu)) return Path.GetFullPath(_cauHinh.ThuMucLuu);
            try
            {
                var docs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BookStore", "EmailGuiDi");
                Directory.CreateDirectory(docs);
                return docs;
            }
            catch (Exception)
            {
                return Path.Combine(AppContext.BaseDirectory, "EmailGuiDi");
            }
        }
    }

    public async Task<KetQua> GuiAsync(string nguoiNhan, string tieuDe, string noiDungHtml)
    {
        if (string.IsNullOrWhiteSpace(nguoiNhan) || !nguoiNhan.Contains('@'))
            return KetQua.Loi("Địa chỉ email người nhận không hợp lệ.");

        var thu = new MimeMessage();
        thu.From.Add(new MailboxAddress(_cauHinh.TenNguoiGui, _cauHinh.DiaChiGui));
        thu.To.Add(MailboxAddress.Parse(nguoiNhan));
        thu.Subject = tieuDe;
        thu.Body = new BodyBuilder { HtmlBody = noiDungHtml }.ToMessageBody();

        try
        {
            if (DaCauHinhSmtp)
            {
                using var smtp = new SmtpClient { Timeout = 15000 };
                await smtp.ConnectAsync(_cauHinh.MayChuSmtp, _cauHinh.Cong, SecureSocketOptions.Auto);
                if (!string.IsNullOrWhiteSpace(_cauHinh.TenDangNhap))
                    await smtp.AuthenticateAsync(_cauHinh.TenDangNhap, _cauHinh.MatKhau ?? string.Empty);
                await smtp.SendAsync(thu);
                await smtp.DisconnectAsync(true);
                _log.LogInformation("Đã gửi email \"{TieuDe}\" tới {NguoiNhan}.", tieuDe, nguoiNhan);
                return KetQua.Dung("Đã gửi email.");
            }

            Directory.CreateDirectory(ThuMucLuu);
            var tenTep = $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{LamSachTenTep(nguoiNhan)}.eml";
            var duongDan = Path.Combine(ThuMucLuu, tenTep);
            await thu.WriteToAsync(duongDan);
            _log.LogInformation("Chưa cấu hình SMTP, email \"{TieuDe}\" tới {NguoiNhan} được lưu tại {DuongDan}.",
                tieuDe, nguoiNhan, duongDan);
            return KetQua.Dung("Đã lưu email ra tập tin (chưa cấu hình SMTP).");
        }
        catch (Exception ex)
        {
            // Loi email chi ghi log, khong lam hong nghiep vu dang chay (thiet ke thich ung)
            _log.LogError(ex, "Không gửi được email \"{TieuDe}\" tới {NguoiNhan}.", tieuDe, nguoiNhan);
            return KetQua.Loi("Không gửi được email: " + ex.Message);
        }
    }

    private static string LamSachTenTep(string s)
        => string.Concat(s.Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '_'));
}
