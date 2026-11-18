namespace BookStore.Domain.Entities;

public class DiaChiGiaoHang
{
    public int MaDiaChi { get; set; }
    public int MaKhachHang { get; set; }
    public string HoTenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string DiaChiChiTiet { get; set; } = string.Empty;
    public string PhuongXa { get; set; } = string.Empty;
    public string QuanHuyen { get; set; } = string.Empty;
    public string TinhThanh { get; set; } = string.Empty;
    public bool LaMacDinh { get; set; }

    public KhachHang? KhachHang { get; set; }

    public string DiaChiDayDu => $"{DiaChiChiTiet}, {PhuongXa}, {QuanHuyen}, {TinhThanh}";
}
