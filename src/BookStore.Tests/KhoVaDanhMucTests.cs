using BookStore.Domain.Enums;
using BookStore.Services.Dtos;

namespace BookStore.Tests;

/// <summary>Kiem thu phan he kho, phieu nhap (BR08), danh muc va quan ly khach hang.</summary>
public class KhoVaDanhMucTests : CoSoKiemThu
{
    private PhieuNhapLuuDto Phieu(params (int MaSach, int SoLuong, decimal Gia)[] dong) => new()
    {
        MaNCC = MaNhaCungCap,
        Dong = dong.Select(d => new DongPhieuNhapDto { MaSach = d.MaSach, SoLuong = d.SoLuong, DonGiaNhap = d.Gia }).ToList()
    };

    [Fact]
    public async Task LapPhieuNhap_ChuaCongTonKho_VaTinhDungTongTien()
    {
        var kq = await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu((MaSachA, 20, 60_000m), (MaSachB, 4, 150_000m)));

        Assert.True(kq.ThanhCong, kq.ThongBao);
        var p = await KhoService.LayPhieuNhapAsync(kq.DuLieu);
        Assert.Equal(TrangThaiPhieuNhap.Nhap, p!.TrangThai);
        Assert.Equal(20 * 60_000m + 4 * 150_000m, p.TongTien);
        Assert.Equal(10, LayTonKho(MaSachA));   // BR08: chua hoan thanh thi chua cong ton
    }

    [Fact]
    public async Task HoanThanhPhieuNhap_CongTonKho_VaKhongSuaXoaDuocNua()
    {
        var ma = (await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu((MaSachA, 20, 60_000m)))).DuLieu;

        var kq = await KhoService.HoanThanhPhieuNhapAsync(ma);

        Assert.True(kq.ThanhCong, kq.ThongBao);
        Assert.Equal(30, LayTonKho(MaSachA));
        Assert.NotNull((await KhoService.LayPhieuNhapAsync(ma))!.NgayHoanThanh);

        Assert.False((await KhoService.CapNhatPhieuNhapAsync(ma, Phieu((MaSachA, 1, 1m)))).ThanhCong);
        Assert.False((await KhoService.XoaPhieuNhapAsync(ma)).ThanhCong);
        Assert.False((await KhoService.HoanThanhPhieuNhapAsync(ma)).ThanhCong);
        Assert.Equal(30, LayTonKho(MaSachA));   // khong cong hai lan
    }

    [Fact]
    public async Task PhieuNhap_DuLieuKhongHopLe_BiTuChoi()
    {
        Assert.False((await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu())).ThanhCong);
        Assert.False((await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu((MaSachA, 0, 1m)))).ThanhCong);
        Assert.False((await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu((MaSachA, 1, 1m), (MaSachA, 2, 1m)))).ThanhCong);
        Assert.False((await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu((9999, 1, 1m)))).ThanhCong);
    }

    [Fact]
    public async Task XoaPhieuNhapChuaHoanThanh_ThanhCong()
    {
        var ma = (await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu((MaSachB, 3, 100_000m)))).DuLieu;

        Assert.True((await KhoService.XoaPhieuNhapAsync(ma)).ThanhCong);
        Assert.Null(await KhoService.LayPhieuNhapAsync(ma));
        Assert.Equal(5, LayTonKho(MaSachB));
    }

    [Fact]
    public async Task CanhBaoTonKho_DungNguongRiengCuaTungSach()
    {
        // Sach A ton 10, sach B ton 5; nguong mac dinh 5 => chi sach B sap het
        var sapHet = await KhoService.LayTonKhoAsync(chiSapHetHang: true);
        Assert.Equal(new[] { MaSachB }, sapHet.Select(x => x.MaSach));

        Assert.True((await KhoService.CapNhatNguongAsync(MaSachA, 12)).ThanhCong);
        sapHet = await KhoService.LayTonKhoAsync(chiSapHetHang: true);
        Assert.Contains(sapHet, x => x.MaSach == MaSachA);
        Assert.Equal(2, (await BaoCaoService.LayThongKeAsync()).SoSachSapHetHang);

        Assert.False((await KhoService.CapNhatNguongAsync(MaSachA, -1)).ThanhCong);
    }

    [Fact]
    public async Task DanhMuc_ThemSuaXoa_VaChanXoaKhiDangDuocDung()
    {
        var them = await DanhMucService.LuuAsync(LoaiDanhMuc.TacGia, new DanhMucDto { Ten = "Nguyễn Nhật Ánh" });
        Assert.True(them.ThanhCong, them.ThongBao);

        var sua = await DanhMucService.LuuAsync(LoaiDanhMuc.TacGia,
            new DanhMucDto { Ma = them.DuLieu, Ten = "Nguyễn Nhật Ánh", MoTa = "Nhà văn" });
        Assert.True(sua.ThanhCong);

        // Gan tac gia cho sach roi thu xoa: phai bi chan
        var sach = await SachService.LayTheoMaAsync(MaSachA);
        var luu = await SachService.CapNhatAsync(new SachLuuDto
        {
            MaSach = MaSachA, TenSach = sach!.TenSach, MaLoaiSach = sach.MaLoaiSach, GiaBan = sach.GiaBan,
            SoLuongTon = sach.SoLuongTon, MaTacGias = new List<int> { them.DuLieu }
        });
        Assert.True(luu.ThanhCong, luu.ThongBao);
        Assert.Equal("Nguyễn Nhật Ánh", (await SachService.LayTheoMaAsync(MaSachA))!.TenTacGia);

        Assert.False((await DanhMucService.XoaAsync(LoaiDanhMuc.TacGia, them.DuLieu)).ThanhCong);

        // Loai sach dang co sach cung khong xoa duoc; loai sach moi thi xoa duoc
        var loai = (await DanhMucService.LayDanhSachAsync(LoaiDanhMuc.LoaiSach)).First();
        Assert.False((await DanhMucService.XoaAsync(LoaiDanhMuc.LoaiSach, loai.Ma)).ThanhCong);
        var loaiMoi = await DanhMucService.LuuAsync(LoaiDanhMuc.LoaiSach, new DanhMucDto { Ten = "Truyện tranh" });
        Assert.True((await DanhMucService.XoaAsync(LoaiDanhMuc.LoaiSach, loaiMoi.DuLieu)).ThanhCong);
    }

    [Fact]
    public async Task DanhMuc_TenTrungHoacRong_BiTuChoi()
    {
        Assert.False((await DanhMucService.LuuAsync(LoaiDanhMuc.NhaCungCap, new DanhMucDto { Ten = "  " })).ThanhCong);
        Assert.False((await DanhMucService.LuuAsync(LoaiDanhMuc.NhaCungCap, new DanhMucDto { Ten = "NCC Kiểm thử" })).ThanhCong);
        Assert.False((await DanhMucService.LuuAsync(LoaiDanhMuc.NhaCungCap,
            new DanhMucDto { Ten = "NCC mới", Email = "khong-hop-le" })).ThanhCong);
    }

    [Fact]
    public async Task NhaCungCapCoPhieuNhap_KhongXoaDuoc()
    {
        await KhoService.TaoPhieuNhapAsync(MaQuanLy, Phieu((MaSachA, 1, 1m)));
        Assert.False((await DanhMucService.XoaAsync(LoaiDanhMuc.NhaCungCap, MaNhaCungCap)).ThanhCong);
    }

    [Fact]
    public async Task KhoaTaiKhoanKhachHang_KhongDangNhapDuoc_MoKhoaThiDuoc()
    {
        Assert.False((await KhachHangService.DoiTrangThaiAsync(MaKhachHang, true, null)).ThanhCong);   // thieu ly do
        Assert.True((await KhachHangService.DoiTrangThaiAsync(MaKhachHang, true, "Bom hàng nhiều lần")).ThanhCong);

        var dn = await TaiKhoanService.DangNhapAsync(new DangNhapDto { TenDangNhap = "khach01", MatKhau = "123456" }, false);
        Assert.False(dn.ThanhCong);
        Assert.Contains("khóa", dn.ThongBao);

        Assert.True((await KhachHangService.DoiTrangThaiAsync(MaKhachHang, false, null)).ThanhCong);
        dn = await TaiKhoanService.DangNhapAsync(new DangNhapDto { TenDangNhap = "khach01", MatKhau = "123456" }, false);
        Assert.True(dn.ThanhCong);
    }

    [Fact]
    public async Task TimKhachHang_TheoTuKhoa_KemSoDonVaChiTieu()
    {
        var ma = await DatDonAsync(HinhThucThanhToan.COD, (MaSachA, 1));
        foreach (var tt in new[] { TrangThaiDonHang.DaXacNhan, TrangThaiDonHang.DangGiao, TrangThaiDonHang.HoanThanh })
            await DonHangService.ChuyenTrangThaiAsync(ma, tt, "ql01", MaQuanLy);

        var ds = await KhachHangService.TimAsync("khach01");
        var kh = Assert.Single(ds);
        Assert.Equal(1, kh.SoDonHang);
        Assert.Equal(125_000m, kh.TongChiTieu);   // 100.000 + 25.000 phi van chuyen
        Assert.Empty(await KhachHangService.TimAsync("khong-ton-tai"));
    }
}
