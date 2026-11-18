using BookStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data;

/// <summary>
/// DbContext cua he thong, anh xa 19 bang trong luoc do CSDL o Chuong 3 bao cao.
/// Dung EF Core 10 theo huong Code First.
/// </summary>
public class BookStoreDbContext : DbContext
{
    public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options) { }

    public DbSet<VaiTro> VaiTro => Set<VaiTro>();
    public DbSet<TaiKhoan> TaiKhoan => Set<TaiKhoan>();
    public DbSet<KhachHang> KhachHang => Set<KhachHang>();
    public DbSet<DiaChiGiaoHang> DiaChiGiaoHang => Set<DiaChiGiaoHang>();
    public DbSet<QuanLy> QuanLy => Set<QuanLy>();
    public DbSet<LoaiSach> LoaiSach => Set<LoaiSach>();
    public DbSet<TacGia> TacGia => Set<TacGia>();
    public DbSet<NhaXuatBan> NhaXuatBan => Set<NhaXuatBan>();
    public DbSet<Sach> Sach => Set<Sach>();
    public DbSet<Sach_TacGia> Sach_TacGia => Set<Sach_TacGia>();
    public DbSet<NhaCungCap> NhaCungCap => Set<NhaCungCap>();
    public DbSet<PhieuNhap> PhieuNhap => Set<PhieuNhap>();
    public DbSet<CTPhieuNhap> CTPhieuNhap => Set<CTPhieuNhap>();
    public DbSet<GioHang> GioHang => Set<GioHang>();
    public DbSet<CTGioHang> CTGioHang => Set<CTGioHang>();
    public DbSet<DonHang> DonHang => Set<DonHang>();
    public DbSet<CTDonHang> CTDonHang => Set<CTDonHang>();
    public DbSet<LichSuDonHang> LichSuDonHang => Set<LichSuDonHang>();
    public DbSet<ThanhToan> ThanhToan => Set<ThanhToan>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ---------- Phan he tai khoan va nguoi dung ----------
        b.Entity<VaiTro>(e =>
        {
            e.ToTable("VaiTro");
            e.HasKey(x => x.MaVaiTro);
            e.Property(x => x.TenVaiTro).HasMaxLength(50).IsRequired();
            e.Property(x => x.MoTa).HasMaxLength(200);
            e.HasIndex(x => x.TenVaiTro).IsUnique();
        });

        b.Entity<TaiKhoan>(e =>
        {
            e.ToTable("TaiKhoan");
            e.HasKey(x => x.MaTaiKhoan);
            e.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
            e.Property(x => x.MatKhauHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.SoDienThoai).HasMaxLength(20);
            e.Property(x => x.TrangThai).HasColumnType("tinyint");
            e.Property(x => x.TokenDatLaiMatKhau).HasMaxLength(255);
            e.HasIndex(x => x.TenDangNhap).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.VaiTro).WithMany(v => v.TaiKhoans)
                .HasForeignKey(x => x.MaVaiTro).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<KhachHang>(e =>
        {
            e.ToTable("KhachHang");
            e.HasKey(x => x.MaKhachHang);
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.GioiTinh).HasColumnType("tinyint");
            e.HasIndex(x => x.MaTaiKhoan).IsUnique();
            e.HasOne(x => x.TaiKhoan).WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(x => x.MaTaiKhoan).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<DiaChiGiaoHang>(e =>
        {
            e.ToTable("DiaChiGiaoHang");
            e.HasKey(x => x.MaDiaChi);
            e.Ignore(x => x.DiaChiDayDu);
            e.Property(x => x.HoTenNguoiNhan).HasMaxLength(100).IsRequired();
            e.Property(x => x.SoDienThoai).HasMaxLength(20).IsRequired();
            e.Property(x => x.DiaChiChiTiet).HasMaxLength(255).IsRequired();
            e.Property(x => x.PhuongXa).HasMaxLength(100).IsRequired();
            e.Property(x => x.QuanHuyen).HasMaxLength(100).IsRequired();
            e.Property(x => x.TinhThanh).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.KhachHang).WithMany(k => k.DiaChiGiaoHangs)
                .HasForeignKey(x => x.MaKhachHang).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<QuanLy>(e =>
        {
            e.ToTable("QuanLy");
            e.HasKey(x => x.MaQuanLy);
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.SoDienThoai).HasMaxLength(20);
            e.Property(x => x.DiaChi).HasMaxLength(255);
            e.Property(x => x.ChucVu).HasMaxLength(50);
            e.HasIndex(x => x.MaTaiKhoan).IsUnique();
            e.HasOne(x => x.TaiKhoan).WithOne(t => t.QuanLy)
                .HasForeignKey<QuanLy>(x => x.MaTaiKhoan).OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Phan he danh muc sach ----------
        b.Entity<LoaiSach>(e =>
        {
            e.ToTable("LoaiSach");
            e.HasKey(x => x.MaLoaiSach);
            e.Property(x => x.TenLoaiSach).HasMaxLength(100).IsRequired();
            e.Property(x => x.MoTa).HasMaxLength(500);
        });

        b.Entity<TacGia>(e =>
        {
            e.ToTable("TacGia");
            e.HasKey(x => x.MaTacGia);
            e.Property(x => x.TenTacGia).HasMaxLength(100).IsRequired();
            e.Property(x => x.TieuSu).HasMaxLength(1000);
        });

        b.Entity<NhaXuatBan>(e =>
        {
            e.ToTable("NhaXuatBan");
            e.HasKey(x => x.MaNXB);
            e.Property(x => x.TenNXB).HasMaxLength(100).IsRequired();
            e.Property(x => x.DiaChi).HasMaxLength(255);
            e.Property(x => x.SoDienThoai).HasMaxLength(20);
        });

        b.Entity<Sach>(e =>
        {
            e.ToTable("Sach");
            e.HasKey(x => x.MaSach);
            e.Property(x => x.ISBN).HasMaxLength(20);
            e.Property(x => x.TenSach).HasMaxLength(255).IsRequired();
            e.Property(x => x.GiaBan).HasColumnType("decimal(18,2)");
            e.Property(x => x.MoTa).HasMaxLength(4000);
            e.Property(x => x.AnhBia).HasMaxLength(255);
            e.Property(x => x.TrangThai).HasColumnType("tinyint");
            e.Property(x => x.NguongCanhBao).HasDefaultValue(5);
            e.HasIndex(x => x.TenSach);
            e.HasOne(x => x.LoaiSach).WithMany(l => l.Sachs)
                .HasForeignKey(x => x.MaLoaiSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NhaXuatBan).WithMany(n => n.Sachs)
                .HasForeignKey(x => x.MaNXB).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Sach_GiaBan", "[GiaBan] >= 0");
                t.HasCheckConstraint("CK_Sach_SoLuongTon", "[SoLuongTon] >= 0");
            });
        });

        b.Entity<Sach_TacGia>(e =>
        {
            e.ToTable("Sach_TacGia");
            e.HasKey(x => new { x.MaSach, x.MaTacGia });
            e.Property(x => x.VaiTroTacGia).HasMaxLength(50);
            e.HasOne(x => x.Sach).WithMany(s => s.Sach_TacGias)
                .HasForeignKey(x => x.MaSach).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.TacGia).WithMany(t => t.Sach_TacGias)
                .HasForeignKey(x => x.MaTacGia).OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Phan he nhap hang ----------
        b.Entity<NhaCungCap>(e =>
        {
            e.ToTable("NhaCungCap");
            e.HasKey(x => x.MaNCC);
            e.Property(x => x.TenNCC).HasMaxLength(150).IsRequired();
            e.Property(x => x.DiaChi).HasMaxLength(255);
            e.Property(x => x.SoDienThoai).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(100);
        });

        b.Entity<PhieuNhap>(e =>
        {
            e.ToTable("PhieuNhap");
            e.HasKey(x => x.MaPhieuNhap);
            e.Property(x => x.TongTien).HasColumnType("decimal(18,2)");
            e.Property(x => x.TrangThai).HasColumnType("tinyint");
            e.Property(x => x.GhiChu).HasMaxLength(500);
            e.HasOne(x => x.NhaCungCap).WithMany(n => n.PhieuNhaps)
                .HasForeignKey(x => x.MaNCC).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.QuanLy).WithMany(n => n.PhieuNhaps)
                .HasForeignKey(x => x.MaQuanLy).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CTPhieuNhap>(e =>
        {
            e.ToTable("CTPhieuNhap");
            e.HasKey(x => new { x.MaPhieuNhap, x.MaSach });
            e.Ignore(x => x.ThanhTien);
            e.Property(x => x.DonGiaNhap).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.PhieuNhap).WithMany(p => p.CTPhieuNhaps)
                .HasForeignKey(x => x.MaPhieuNhap).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sach).WithMany(s => s.CTPhieuNhaps)
                .HasForeignKey(x => x.MaSach).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_CTPhieuNhap_SoLuong", "[SoLuong] > 0"));
        });

        // ---------- Phan he gio hang ----------
        b.Entity<GioHang>(e =>
        {
            e.ToTable("GioHang");
            e.HasKey(x => x.MaGioHang);
            e.HasIndex(x => x.MaKhachHang).IsUnique();
            e.HasOne(x => x.KhachHang).WithOne(k => k.GioHang)
                .HasForeignKey<GioHang>(x => x.MaKhachHang).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CTGioHang>(e =>
        {
            e.ToTable("CTGioHang");
            e.HasKey(x => new { x.MaGioHang, x.MaSach });
            e.HasOne(x => x.GioHang).WithMany(g => g.CTGioHangs)
                .HasForeignKey(x => x.MaGioHang).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sach).WithMany()
                .HasForeignKey(x => x.MaSach).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_CTGioHang_SoLuong", "[SoLuong] > 0"));
        });

        // ---------- Phan he don hang ----------
        b.Entity<DonHang>(e =>
        {
            e.ToTable("DonHang");
            e.HasKey(x => x.MaDonHang);
            e.Property(x => x.TrangThai).HasColumnType("tinyint");
            e.Property(x => x.HinhThucThanhToan).HasColumnType("tinyint");
            e.Property(x => x.HoTenNguoiNhan).HasMaxLength(100).IsRequired();
            e.Property(x => x.SoDienThoaiNhan).HasMaxLength(20).IsRequired();
            e.Property(x => x.DiaChiGiao).HasMaxLength(500).IsRequired();
            e.Property(x => x.TamTinh).HasColumnType("decimal(18,2)");
            e.Property(x => x.PhiVanChuyen).HasColumnType("decimal(18,2)");
            e.Property(x => x.TongTien).HasColumnType("decimal(18,2)");
            e.Property(x => x.GhiChu).HasMaxLength(500);
            e.HasIndex(x => x.NgayDat);
            e.HasIndex(x => x.TrangThai);
            e.HasOne(x => x.KhachHang).WithMany(k => k.DonHangs)
                .HasForeignKey(x => x.MaKhachHang).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.QuanLy).WithMany(n => n.DonHangs)
                .HasForeignKey(x => x.MaQuanLy).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<CTDonHang>(e =>
        {
            e.ToTable("CTDonHang");
            e.HasKey(x => new { x.MaDonHang, x.MaSach });
            e.Ignore(x => x.ThanhTien);
            e.Property(x => x.TenSach).HasMaxLength(255).IsRequired();
            e.Property(x => x.DonGia).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.DonHang).WithMany(d => d.CTDonHangs)
                .HasForeignKey(x => x.MaDonHang).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sach).WithMany(s => s.CTDonHangs)
                .HasForeignKey(x => x.MaSach).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_CTDonHang_SoLuong", "[SoLuong] > 0"));
        });

        b.Entity<LichSuDonHang>(e =>
        {
            e.ToTable("LichSuDonHang");
            e.HasKey(x => x.MaLichSu);
            e.Property(x => x.TrangThaiCu).HasColumnType("tinyint");
            e.Property(x => x.TrangThaiMoi).HasColumnType("tinyint");
            e.Property(x => x.NguoiThucHien).HasMaxLength(50);
            e.Property(x => x.GhiChu).HasMaxLength(500);
            e.HasOne(x => x.DonHang).WithMany(d => d.LichSuDonHangs)
                .HasForeignKey(x => x.MaDonHang).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ThanhToan>(e =>
        {
            e.ToTable("ThanhToan");
            e.HasKey(x => x.MaThanhToan);
            e.Property(x => x.PhuongThuc).HasColumnType("tinyint");
            e.Property(x => x.TrangThai).HasColumnType("tinyint");
            e.Property(x => x.SoTien).HasColumnType("decimal(18,2)");
            e.Property(x => x.MaGiaoDich).HasMaxLength(100);
            e.Property(x => x.NoiDung).HasMaxLength(500);
            e.HasOne(x => x.DonHang).WithMany(d => d.ThanhToans)
                .HasForeignKey(x => x.MaDonHang).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
