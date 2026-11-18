namespace BookStore.Domain.Enums;

/// <summary>Trang thai don hang (BR03, BR04).</summary>
public enum TrangThaiDonHang : byte
{
    ChoThanhToan = 0,
    ChoXacNhan = 1,
    DaXacNhan = 2,
    DangGiao = 3,
    HoanThanh = 4,
    DaHuy = 5
}

public enum HinhThucThanhToan : byte
{
    COD = 0,
    VNPay = 1
}

public enum TrangThaiThanhToan : byte
{
    Cho = 0,
    ThanhCong = 1,
    ThatBai = 2
}

/// <summary>0 = Ngung kinh doanh, 1 = Dang kinh doanh (BR07).</summary>
public enum TrangThaiSach : byte
{
    NgungKinhDoanh = 0,
    DangKinhDoanh = 1
}

public enum TrangThaiTaiKhoan : byte
{
    BiKhoa = 0,
    HoatDong = 1
}

/// <summary>Phieu nhap chi cong ton khi Hoan thanh (BR06).</summary>
public enum TrangThaiPhieuNhap : byte
{
    Nhap = 0,
    HoanThanh = 1,
    DaHuy = 2
}
