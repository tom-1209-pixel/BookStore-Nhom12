using Microsoft.Extensions.Configuration;

namespace BookStore.Services.Common;

/// <summary>
/// Quy uoc luu anh bia sach. Anh nam trong mot thu muc dung chung giua BookStore.Api
/// (noi nhan anh tai len tu ung dung quan tri) va BookStore.Web (noi hien thi cho khach).
/// CSDL chi luu duong dan tuong doi dang "/anh-bia/ten-tep.jpg" trong cot Sach.AnhBia.
/// </summary>
public static class AnhBia
{
    /// <summary>Duong dan URL ma ca Web va Api dung de phuc vu anh.</summary>
    public const string DuongDanUrl = "/anh-bia";

    public const long DungLuongToiDa = 5 * 1024 * 1024;

    public static readonly string[] DuoiChoPhep = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>
    /// Thu muc chua anh: doc tu cau hinh "AnhBia:ThuMuc"; neu khong co thi dung thu muc
    /// "AnhBia" nam canh cac project (src/AnhBia) de Web va Api cung thay mot noi.
    /// </summary>
    public static string LayThuMuc(IConfiguration cauHinh, string thuMucGocUngDung)
    {
        var thuMuc = cauHinh["AnhBia:ThuMuc"];
        if (string.IsNullOrWhiteSpace(thuMuc))
            thuMuc = Path.Combine(thuMucGocUngDung, "..", "AnhBia");
        else if (!Path.IsPathRooted(thuMuc))
            thuMuc = Path.Combine(thuMucGocUngDung, thuMuc);

        thuMuc = Path.GetFullPath(thuMuc);
        Directory.CreateDirectory(thuMuc);
        return thuMuc;
    }

    /// <summary>Kiem tra noi dung tep dung la anh JPEG / PNG / WebP (khong chi tin vao duoi tep).</summary>
    public static bool LaAnhHopLe(ReadOnlySpan<byte> dauTep, string duoi)
    {
        return duoi switch
        {
            ".jpg" or ".jpeg" => dauTep.Length >= 3 && dauTep[0] == 0xFF && dauTep[1] == 0xD8 && dauTep[2] == 0xFF,
            ".png" => dauTep.Length >= 8 && dauTep[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => dauTep.Length >= 12
                       && dauTep[..4].SequenceEqual("RIFF"u8)
                       && dauTep[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
