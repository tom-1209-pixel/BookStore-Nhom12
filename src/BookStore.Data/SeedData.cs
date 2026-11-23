using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data;

/// <summary>
/// Tao du lieu khoi tao: 3 vai tro, 3 tai khoan mau, danh muc sach, tac gia,
/// nha xuat ban, nha cung cap va 12 quyen sach de demo.
/// Ham chi chay khi bang VaiTro con rong nen co the goi moi lan khoi dong.
/// </summary>
public static class SeedData
{
    public const string MatKhauMacDinh = "123456";

    /// <summary>ISBN cua cac sach mau co san anh bia (tep {ISBN}.png trong thu muc anh bia dung chung).</summary>
    private static readonly string[] IsbnCoAnhMau =
    [
        "9786046742011", "9780132350884", "9786046743012", "9786041081338", "9786041093256", "9786041125478",
        "9786045868751", "9786047736333", "9786049558412", "9786049558419", "9786047742215", "9786042108881"
    ];

    public static async Task KhoiTaoAsync(BookStoreDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.VaiTro.AnyAsync())
            await TaoDuLieuMauAsync(db);

        await BoSungAnhBiaMauAsync(db);
    }

    /// <summary>Gan anh bia mau cho sach mau chua co anh (ca CSDL tao tu truoc khi co anh bia).</summary>
    private static async Task BoSungAnhBiaMauAsync(BookStoreDbContext db)
    {
        var ds = await db.Sach
            .Where(s => s.AnhBia == null && s.ISBN != null && IsbnCoAnhMau.Contains(s.ISBN))
            .ToListAsync();
        foreach (var s in ds) s.AnhBia = $"/anh-bia/{s.ISBN}.png";
        if (ds.Count > 0) await db.SaveChangesAsync();
    }

    private static async Task TaoDuLieuMauAsync(BookStoreDbContext db)
    {

        var hasher = new PasswordHasher<TaiKhoan>();
        string Hash(TaiKhoan tk) => hasher.HashPassword(tk, MatKhauMacDinh);

        // ----- Vai tro -----
        var vtKhach = new VaiTro { TenVaiTro = TenVaiTro.KhachHang, MoTa = "Khach hang mua sach tren trang Web" };
        var vtQuanLy = new VaiTro { TenVaiTro = TenVaiTro.QuanLy, MoTa = "Quan ly nha sach, su dung ung dung quan tri" };
        db.VaiTro.AddRange(vtKhach, vtQuanLy);
        await db.SaveChangesAsync();

        // ----- Tai khoan quan ly -----
        var tkQuanLy = new TaiKhoan
        {
            TenDangNhap = "quanly",
            Email = "quanly@bookstore.local",
            SoDienThoai = "0901000001",
            MaVaiTro = vtQuanLy.MaVaiTro
        };
        tkQuanLy.MatKhauHash = Hash(tkQuanLy);

        var tkKhach = new TaiKhoan
        {
            TenDangNhap = "khachhang",
            Email = "khachhang@bookstore.local",
            SoDienThoai = "0901000003",
            MaVaiTro = vtKhach.MaVaiTro
        };
        tkKhach.MatKhauHash = Hash(tkKhach);

        db.TaiKhoan.AddRange(tkQuanLy, tkKhach);
        await db.SaveChangesAsync();

        db.QuanLy.Add(
            new QuanLy
            {
                MaTaiKhoan = tkQuanLy.MaTaiKhoan,
                HoTen = "Pham Hoang Sang",
                SoDienThoai = "0901000001",
                ChucVu = "Quan ly nha sach",
                NgayVaoLam = new DateTime(2024, 1, 2)
            });

        var khach = new KhachHang
        {
            MaTaiKhoan = tkKhach.MaTaiKhoan,
            HoTen = "Ngo Ho Dang Khoa",
            GioiTinh = 1,
            NgayDangKy = new DateTime(2025, 3, 10)
        };
        db.KhachHang.Add(khach);
        await db.SaveChangesAsync();

        db.DiaChiGiaoHang.Add(new DiaChiGiaoHang
        {
            MaKhachHang = khach.MaKhachHang,
            HoTenNguoiNhan = "Ngo Ho Dang Khoa",
            SoDienThoai = "0901000003",
            DiaChiChiTiet = "273 An Duong Vuong",
            PhuongXa = "Phuong 3",
            QuanHuyen = "Quan 5",
            TinhThanh = "TP Ho Chi Minh",
            LaMacDinh = true
        });

        // ----- Danh muc -----
        var loai = new[]
        {
            new LoaiSach { TenLoaiSach = "Cong nghe thong tin", MoTa = "Sach lap trinh, mang, co so du lieu" },
            new LoaiSach { TenLoaiSach = "Van hoc Viet Nam", MoTa = "Truyen, tho, tap but Viet Nam" },
            new LoaiSach { TenLoaiSach = "Van hoc nuoc ngoai", MoTa = "Tac pham dich" },
            new LoaiSach { TenLoaiSach = "Kinh te - Quan tri", MoTa = "Kinh doanh, marketing, quan tri" },
            new LoaiSach { TenLoaiSach = "Ky nang song", MoTa = "Phat trien ban than" },
            new LoaiSach { TenLoaiSach = "Thieu nhi", MoTa = "Sach cho thieu nhi" }
        };
        db.LoaiSach.AddRange(loai);

        var nxb = new[]
        {
            new NhaXuatBan { TenNXB = "NXB Tre", DiaChi = "161B Ly Chinh Thang, TP HCM", SoDienThoai = "02839316289" },
            new NhaXuatBan { TenNXB = "NXB Kim Dong", DiaChi = "55 Quang Trung, Ha Noi", SoDienThoai = "02439434730" },
            new NhaXuatBan { TenNXB = "NXB Thong tin va Truyen thong", DiaChi = "9 Ngo Quyen, Ha Noi", SoDienThoai = "02435772145" },
            new NhaXuatBan { TenNXB = "NXB Lao dong", DiaChi = "175 Giang Vo, Ha Noi", SoDienThoai = "02438515380" }
        };
        db.NhaXuatBan.AddRange(nxb);

        var tacGia = new[]
        {
            new TacGia { TenTacGia = "Nguyen Nhat Anh" },
            new TacGia { TenTacGia = "Pham Huu Khang" },
            new TacGia { TenTacGia = "Dale Carnegie" },
            new TacGia { TenTacGia = "Paulo Coelho" },
            new TacGia { TenTacGia = "To Hoai" },
            new TacGia { TenTacGia = "Robert C. Martin" },
            new TacGia { TenTacGia = "Adam Khoo" },
            new TacGia { TenTacGia = "Fujiko F. Fujio" }
        };
        db.TacGia.AddRange(tacGia);

        db.NhaCungCap.AddRange(
            new NhaCungCap { TenNCC = "Cong ty Phat hanh sach Fahasa", DiaChi = "60-62 Le Loi, Quan 1, TP HCM", SoDienThoai = "02838225796", Email = "cskh@fahasa.com.vn" },
            new NhaCungCap { TenNCC = "Cong ty sach Phuong Nam", DiaChi = "940 Duong 3 Thang 2, Quan 11, TP HCM", SoDienThoai = "02839625456", Email = "info@phuongnambook.com.vn" });

        await db.SaveChangesAsync();

        // ----- Sach -----
        var sachs = new[]
        {
            new Sach { TenSach = "Lap trinh C# tu co ban den nang cao", MaLoaiSach = loai[0].MaLoaiSach, MaNXB = nxb[2].MaNXB, ISBN = "9786046742011", NamXuatBan = 2023, SoTrang = 520, GiaBan = 185000, SoLuongTon = 40, MoTa = "Giao trinh C# tu cu phap den lap trinh huong doi tuong, LINQ va EF Core." },
            new Sach { TenSach = "Clean Code - Ma nguon sach", MaLoaiSach = loai[0].MaLoaiSach, MaNXB = nxb[2].MaNXB, ISBN = "9780132350884", NamXuatBan = 2022, SoTrang = 464, GiaBan = 245000, SoLuongTon = 25, MoTa = "Nguyen tac viet ma nguon de doc, de bao tri." },
            new Sach { TenSach = "Co so du lieu SQL Server tu A den Z", MaLoaiSach = loai[0].MaLoaiSach, MaNXB = nxb[2].MaNXB, ISBN = "9786046743012", NamXuatBan = 2024, SoTrang = 388, GiaBan = 165000, SoLuongTon = 30, MoTa = "Thiet ke, truy van va toi uu CSDL SQL Server." },
            new Sach { TenSach = "Mat biec", MaLoaiSach = loai[1].MaLoaiSach, MaNXB = nxb[0].MaNXB, ISBN = "9786041081338", NamXuatBan = 2021, SoTrang = 268, GiaBan = 110000, SoLuongTon = 50, MoTa = "Truyen dai cua Nguyen Nhat Anh." },
            new Sach { TenSach = "Cho toi xin mot ve di tuoi tho", MaLoaiSach = loai[1].MaLoaiSach, MaNXB = nxb[0].MaNXB, ISBN = "9786041093256", NamXuatBan = 2022, SoTrang = 212, GiaBan = 98000, SoLuongTon = 45, MoTa = "Tac pham duoc giai thuong Van hoc ASEAN." },
            new Sach { TenSach = "Dat rung phuong Nam", MaLoaiSach = loai[1].MaLoaiSach, MaNXB = nxb[0].MaNXB, ISBN = "9786041125478", NamXuatBan = 2020, SoTrang = 340, GiaBan = 125000, SoLuongTon = 20, MoTa = "Tieu thuyet thieu nhi kinh dien." },
            new Sach { TenSach = "Nha gia kim", MaLoaiSach = loai[2].MaLoaiSach, MaNXB = nxb[0].MaNXB, ISBN = "9786045868751", NamXuatBan = 2023, SoTrang = 228, GiaBan = 89000, SoLuongTon = 60, MoTa = "Hanh trinh di tim kho bau cua chang trai Santiago." },
            new Sach { TenSach = "Dac nhan tam", MaLoaiSach = loai[4].MaLoaiSach, MaNXB = nxb[3].MaNXB, ISBN = "9786047736333", NamXuatBan = 2022, SoTrang = 320, GiaBan = 108000, SoLuongTon = 55, MoTa = "Nghe thuat doi nhan xu the." },
            new Sach { TenSach = "Quan tri hoc can ban", MaLoaiSach = loai[3].MaLoaiSach, MaNXB = nxb[3].MaNXB, ISBN = "9786049558412", NamXuatBan = 2023, SoTrang = 456, GiaBan = 195000, SoLuongTon = 18, MoTa = "Giao trinh quan tri hoc danh cho sinh vien kinh te." },
            new Sach { TenSach = "Marketing can ban", MaLoaiSach = loai[3].MaLoaiSach, MaNXB = nxb[3].MaNXB, ISBN = "9786049558419", NamXuatBan = 2024, SoTrang = 402, GiaBan = 178000, SoLuongTon = 22, MoTa = "Cac nguyen ly marketing hien dai." },
            new Sach { TenSach = "Toi tai gioi, ban cung the", MaLoaiSach = loai[4].MaLoaiSach, MaNXB = nxb[3].MaNXB, ISBN = "9786047742215", NamXuatBan = 2021, SoTrang = 376, GiaBan = 132000, SoLuongTon = 35, MoTa = "Phuong phap hoc tap hieu qua." },
            new Sach { TenSach = "Doraemon tap 1", MaLoaiSach = loai[5].MaLoaiSach, MaNXB = nxb[1].MaNXB, ISBN = "9786042108881", NamXuatBan = 2024, SoTrang = 192, GiaBan = 25000, SoLuongTon = 100, MoTa = "Truyen tranh thieu nhi." }
        };
        db.Sach.AddRange(sachs);
        await db.SaveChangesAsync();

        db.Sach_TacGia.AddRange(
            new Sach_TacGia { MaSach = sachs[0].MaSach, MaTacGia = tacGia[1].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[1].MaSach, MaTacGia = tacGia[5].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[2].MaSach, MaTacGia = tacGia[1].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[3].MaSach, MaTacGia = tacGia[0].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[4].MaSach, MaTacGia = tacGia[0].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[5].MaSach, MaTacGia = tacGia[4].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[6].MaSach, MaTacGia = tacGia[3].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[7].MaSach, MaTacGia = tacGia[2].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[10].MaSach, MaTacGia = tacGia[6].MaTacGia, VaiTroTacGia = "Tac gia chinh" },
            new Sach_TacGia { MaSach = sachs[11].MaSach, MaTacGia = tacGia[7].MaTacGia, VaiTroTacGia = "Tac gia chinh" });

        await db.SaveChangesAsync();
    }
}
