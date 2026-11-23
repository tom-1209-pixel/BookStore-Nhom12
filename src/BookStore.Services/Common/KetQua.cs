namespace BookStore.Services.Common;

/// <summary>
/// Kieu tra ve chung cho tang Services. Thay cho viec nem ngoai le khi gap loi
/// nghiep vu: tang goi (Web, API) chi can doc ThanhCong va ThongBao de hien thi.
/// </summary>
public class KetQua
{
    public bool ThanhCong { get; init; }
    public string ThongBao { get; init; } = string.Empty;

    /// <summary>Ma loi nghiep vu, vi du BR02, de ghi log va doi chieu bao cao.</summary>
    public string? MaLoi { get; init; }

    public static KetQua Dung(string thongBao = "Thành công")
        => new() { ThanhCong = true, ThongBao = thongBao };

    public static KetQua Loi(string thongBao, string? maLoi = null)
        => new() { ThanhCong = false, ThongBao = thongBao, MaLoi = maLoi };
}

/// <summary>Ket qua co kem du lieu tra ve.</summary>
public class KetQua<T> : KetQua
{
    public T? DuLieu { get; init; }

    public static KetQua<T> Dung(T duLieu, string thongBao = "Thành công")
        => new() { ThanhCong = true, ThongBao = thongBao, DuLieu = duLieu };

    public static new KetQua<T> Loi(string thongBao, string? maLoi = null)
        => new() { ThanhCong = false, ThongBao = thongBao, MaLoi = maLoi };
}
