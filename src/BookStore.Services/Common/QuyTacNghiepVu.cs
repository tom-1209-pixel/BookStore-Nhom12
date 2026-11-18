namespace BookStore.Services.Common;

/// <summary>
/// Cac hang so cua quy tac nghiep vu BR01-BR10 (muc 3.1.2 bao cao).
/// Dat tap trung mot cho de khi quy tac thay doi chi sua mot noi.
/// </summary>
public static class QuyTacNghiepVu
{
    /// <summary>BR10: phi van chuyen co dinh.</summary>
    public const decimal PhiVanChuyen = 25_000m;

    /// <summary>BR10: mien phi van chuyen tu muc nay tro len.</summary>
    public const decimal NguongMienPhiVanChuyen = 300_000m;

    /// <summary>BR04: don VNPay chua thanh toan sau so phut nay thi tu huy.</summary>
    public const int SoPhutHetHanThanhToan = 30;

    /// <summary>So lan dang nhap sai toi da truoc khi khoa tai khoan.</summary>
    public const int SoLanDangNhapSaiToiDa = 5;

    /// <summary>So phut khoa tai khoan sau khi dang nhap sai qua nhieu lan.</summary>
    public const int SoPhutKhoaTaiKhoan = 15;

    /// <summary>BR10: tinh phi van chuyen theo tam tinh cua don hang.</summary>
    public static decimal TinhPhiVanChuyen(decimal tamTinh)
        => tamTinh >= NguongMienPhiVanChuyen ? 0m : PhiVanChuyen;
}
