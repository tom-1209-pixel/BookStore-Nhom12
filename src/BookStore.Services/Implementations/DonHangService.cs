using BookStore.Data;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Services.Common;
using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Services.Implementations;

/// <summary>
/// Nghiep vu don hang. Day la noi cai dat cac quy tac BR02-BR07, BR10
/// va so do trang thai don hang (Hinh 3.3 trong bao cao).
/// </summary>
public class DonHangService : IDonHangService
{
    private readonly BookStoreDbContext _db;
    private readonly ILogger<DonHangService> _log;
    private readonly IEmailService _email;

    public DonHangService(BookStoreDbContext db, ILogger<DonHangService> log, IEmailService email)
    {
        _db = db;
        _log = log;
        _email = email;
    }

    /// <summary>
    /// Cac buoc chuyen trang thai duoc phep. Moi yeu cau khac so do nay se bi tu choi.
    /// </summary>
    private static readonly Dictionary<TrangThaiDonHang, TrangThaiDonHang[]> BuocChuyenHopLe = new()
    {
        [TrangThaiDonHang.ChoThanhToan] = new[] { TrangThaiDonHang.ChoXacNhan, TrangThaiDonHang.DaHuy },
        [TrangThaiDonHang.ChoXacNhan] = new[] { TrangThaiDonHang.DaXacNhan, TrangThaiDonHang.DaHuy },
        [TrangThaiDonHang.DaXacNhan] = new[] { TrangThaiDonHang.DangGiao, TrangThaiDonHang.DaHuy },
        [TrangThaiDonHang.DangGiao] = new[] { TrangThaiDonHang.HoanThanh, TrangThaiDonHang.DaHuy },
        [TrangThaiDonHang.HoanThanh] = Array.Empty<TrangThaiDonHang>(),
        [TrangThaiDonHang.DaHuy] = Array.Empty<TrangThaiDonHang>()
    };

    public async Task<KetQua<DonHangDto>> DatHangAsync(int maKhachHang, DatHangDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.HoTenNguoiNhan))
            return KetQua<DonHangDto>.Loi("Vui lòng nhập họ tên người nhận.");
        if (string.IsNullOrWhiteSpace(dto.SoDienThoaiNhan))
            return KetQua<DonHangDto>.Loi("Vui lòng nhập số điện thoại người nhận.");
        if (string.IsNullOrWhiteSpace(dto.DiaChiGiao))
            return KetQua<DonHangDto>.Loi("Vui lòng nhập địa chỉ giao hàng.");

        var gio = await _db.GioHang
            .Include(g => g.CTGioHangs)
            .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

        if (gio is null || gio.CTGioHangs.Count == 0)
            return KetQua<DonHangDto>.Loi("Giỏ hàng đang trống.");

        // BR03: tru ton kho ngay khi tao don, trong cung mot transaction voi viec ghi don.
        // Chuoi ket noi bat EnableRetryOnFailure nen transaction thu cong phai nam
        // trong mot execution strategy, neu khong EF Core se bao loi.
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        await using var tran = await _db.Database.BeginTransactionAsync();
        try
        {
            var maSachs = gio.CTGioHangs.Select(ct => ct.MaSach).ToList();
            var sachs = await _db.Sach.Where(s => maSachs.Contains(s.MaSach)).ToListAsync();

            var don = new DonHang
            {
                MaKhachHang = maKhachHang,
                NgayDat = DateTime.Now,
                HoTenNguoiNhan = dto.HoTenNguoiNhan.Trim(),
                SoDienThoaiNhan = dto.SoDienThoaiNhan.Trim(),
                DiaChiGiao = dto.DiaChiGiao.Trim(),
                HinhThucThanhToan = dto.HinhThucThanhToan,
                GhiChu = dto.GhiChu,
                // Don VNPay bat dau o Cho thanh toan, don COD vao thang Cho xac nhan.
                TrangThai = dto.HinhThucThanhToan == HinhThucThanhToan.VNPay
                    ? TrangThaiDonHang.ChoThanhToan
                    : TrangThaiDonHang.ChoXacNhan
            };

            decimal tamTinh = 0m;

            foreach (var ct in gio.CTGioHangs)
            {
                var sach = sachs.FirstOrDefault(s => s.MaSach == ct.MaSach);
                if (sach is null)
                    return KetQua<DonHangDto>.Loi("Giỏ hàng chứa sách không còn tồn tại.");

                if (sach.TrangThai == TrangThaiSach.NgungKinhDoanh)
                    return KetQua<DonHangDto>.Loi($"Sách \"{sach.TenSach}\" đã ngừng kinh doanh.", "BR09");

                if (ct.SoLuong > sach.SoLuongTon)
                    return KetQua<DonHangDto>.Loi(
                        $"Sách \"{sach.TenSach}\" chỉ còn {sach.SoLuongTon} quyển, không đủ số lượng đặt.", "BR02");

                // BR04: don gia duoc sao luu tai thoi diem dat hang.
                don.CTDonHangs.Add(new CTDonHang
                {
                    MaSach = sach.MaSach,
                    TenSach = sach.TenSach,
                    SoLuong = ct.SoLuong,
                    DonGia = sach.GiaBan
                });

                tamTinh += sach.GiaBan * ct.SoLuong;
                sach.SoLuongTon -= ct.SoLuong;   // BR03
            }

            don.TamTinh = tamTinh;
            don.PhiVanChuyen = QuyTacNghiepVu.TinhPhiVanChuyen(tamTinh);   // BR07
            don.TongTien = don.TamTinh + don.PhiVanChuyen;

            _db.DonHang.Add(don);
            await _db.SaveChangesAsync();

            // Moi don deu phat sinh mot ban ghi ThanhToan o trang thai Cho.
            _db.ThanhToan.Add(new ThanhToan
            {
                MaDonHang = don.MaDonHang,
                PhuongThuc = don.HinhThucThanhToan,
                SoTien = don.TongTien,
                TrangThai = TrangThaiThanhToan.Cho,
                NoiDung = don.HinhThucThanhToan == HinhThucThanhToan.COD
                    ? "Thu tiền khi giao hàng"
                    : "Chờ khách thanh toán qua VNPay"
            });

            _db.LichSuDonHang.Add(new LichSuDonHang
            {
                MaDonHang = don.MaDonHang,
                TrangThaiCu = null,
                TrangThaiMoi = don.TrangThai,
                NguoiThucHien = "Khách hàng",
                GhiChu = "Khách hàng đặt hàng trên trang Web"
            });

            // Gio hang duoc xoa sau khi dat hang thanh cong.
            _db.CTGioHang.RemoveRange(gio.CTGioHangs);
            gio.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();
            await tran.CommitAsync();

            _log.LogInformation(
                "Đã tạo đơn hàng {MaDonHang} cho khách {MaKhachHang}, tổng tiền {TongTien}.",
                don.MaDonHang, maKhachHang, don.TongTien);

            var chiTiet = await LayChiTietAsync(don.MaDonHang);

            // Gui email xac nhan sau khi transaction da commit; loi email khong anh huong don hang
            var emailKhach = await LayEmailKhachAsync(maKhachHang);
            if (emailKhach is not null)
            {
                var (tieuDe, noiDung) = MauEmail.XacNhanDonHang(chiTiet!);
                await _email.GuiAsync(emailKhach, tieuDe, noiDung);
            }

            return KetQua<DonHangDto>.Dung(chiTiet!, $"Đặt hàng thành công. Mã đơn hàng của bạn là #{don.MaDonHang}.");
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _log.LogError(ex, "Lỗi khi tạo đơn hàng cho khách {MaKhachHang}.", maKhachHang);
            return KetQua<DonHangDto>.Loi("Không thể tạo đơn hàng do lỗi hệ thống. Vui lòng thử lại.");
        }
        });
    }

    public async Task<DonHangDto?> LayChiTietAsync(int maDonHang, int? maKhachHangLoc = null)
    {
        var q = _db.DonHang
            .Include(d => d.KhachHang)
            .Include(d => d.QuanLy)
            .Include(d => d.CTDonHangs).ThenInclude(ct => ct.Sach)
            .Include(d => d.LichSuDonHangs)
            .AsNoTracking()
            .Where(d => d.MaDonHang == maDonHang);

        if (maKhachHangLoc.HasValue)
            q = q.Where(d => d.MaKhachHang == maKhachHangLoc.Value);

        var don = await q.FirstOrDefaultAsync();
        return don is null ? null : ChuyenDoi(don, kemChiTiet: true);
    }

    public async Task<TrangKetQua<DonHangDto>> TimAsync(TimDonHangDto dto)
    {
        var q = _db.DonHang
            .Include(d => d.KhachHang)
            .Include(d => d.QuanLy)
            .AsNoTracking()
            .AsQueryable();

        if (dto.TrangThai.HasValue)
            q = q.Where(d => d.TrangThai == dto.TrangThai.Value);
        if (dto.TuNgay.HasValue)
            q = q.Where(d => d.NgayDat >= dto.TuNgay.Value.Date);
        if (dto.DenNgay.HasValue)
            q = q.Where(d => d.NgayDat < dto.DenNgay.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(dto.TuKhoa))
        {
            var tk = dto.TuKhoa.Trim();
            q = q.Where(d => EF.Functions.Like(d.HoTenNguoiNhan, $"%{tk}%")
                          || EF.Functions.Like(d.SoDienThoaiNhan, $"%{tk}%")
                          || d.MaDonHang.ToString() == tk);
        }

        var tong = await q.CountAsync();
        var trang = Math.Max(1, dto.Trang);
        var soDong = dto.SoDongMoiTrang <= 0 ? 20 : dto.SoDongMoiTrang;

        var ds = await q.OrderByDescending(d => d.MaDonHang)
            .Skip((trang - 1) * soDong).Take(soDong)
            .ToListAsync();

        return new TrangKetQua<DonHangDto>
        {
            DanhSach = ds.Select(d => ChuyenDoi(d, kemChiTiet: false)).ToList(),
            TongSoDong = tong,
            Trang = trang,
            SoDongMoiTrang = soDong
        };
    }

    public async Task<List<DonHangDto>> LayDonCuaKhachAsync(int maKhachHang)
    {
        var ds = await _db.DonHang
            .Include(d => d.KhachHang)
            .Include(d => d.CTDonHangs).ThenInclude(ct => ct.Sach)
            .Where(d => d.MaKhachHang == maKhachHang)
            .OrderByDescending(d => d.MaDonHang)
            .AsNoTracking()
            .ToListAsync();
        return ds.Select(d => ChuyenDoi(d, kemChiTiet: true)).ToList();
    }

    public async Task<KetQua> ChuyenTrangThaiAsync(
        int maDonHang, TrangThaiDonHang trangThaiMoi, string nguoiThucHien, int? maQuanLy, string? ghiChu = null)
    {
        var don = await _db.DonHang
            .Include(d => d.CTDonHangs)
            .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        if (don is null) return KetQua.Loi("Không tìm thấy đơn hàng.");

        var trangThaiCu = don.TrangThai;

        if (trangThaiCu == trangThaiMoi)
            return KetQua.Loi($"Đơn hàng đã ở trạng thái {DonHangDto.MoTaTrangThai(trangThaiMoi)}.");

        if (!BuocChuyenHopLe.TryGetValue(trangThaiCu, out var choPhep) || !choPhep.Contains(trangThaiMoi))
            return KetQua.Loi(
                $"Không thể chuyển đơn từ {DonHangDto.MoTaTrangThai(trangThaiCu)} " +
                $"sang {DonHangDto.MoTaTrangThai(trangThaiMoi)}.");

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        await using var tran = await _db.Database.BeginTransactionAsync();
        try
        {
            // BR03: huy don thi hoan lai so luong ton kho da tru luc tao don.
            if (trangThaiMoi == TrangThaiDonHang.DaHuy)
            {
                await HoanTonKhoAsync(don);

                var tt = await _db.ThanhToan
                    .Where(x => x.MaDonHang == don.MaDonHang && x.TrangThai == TrangThaiThanhToan.Cho)
                    .ToListAsync();
                foreach (var t in tt)
                {
                    t.TrangThai = TrangThaiThanhToan.ThatBai;
                    t.NoiDung = ghiChu ?? "Đơn hàng bị hủy";
                }
            }

            // Don COD duoc ghi nhan thu tien khi don chuyen sang Hoan thanh.
            if (trangThaiMoi == TrangThaiDonHang.HoanThanh)
            {
                var tt = await _db.ThanhToan
                    .Where(x => x.MaDonHang == don.MaDonHang && x.TrangThai == TrangThaiThanhToan.Cho)
                    .ToListAsync();
                foreach (var t in tt)
                {
                    t.TrangThai = TrangThaiThanhToan.ThanhCong;
                    t.ThoiDiem = DateTime.Now;
                    t.NoiDung = "Đã thu tiền khi giao hàng";
                }
            }

            don.TrangThai = trangThaiMoi;
            if (maQuanLy.HasValue) don.MaQuanLy = maQuanLy;

            _db.LichSuDonHang.Add(new LichSuDonHang
            {
                MaDonHang = don.MaDonHang,
                TrangThaiCu = trangThaiCu,
                TrangThaiMoi = trangThaiMoi,
                NguoiThucHien = nguoiThucHien,
                GhiChu = ghiChu
            });

            if (!string.IsNullOrWhiteSpace(ghiChu))
                don.GhiChu = ghiChu;

            await _db.SaveChangesAsync();
            await tran.CommitAsync();

            _log.LogInformation(
                "Đơn {MaDonHang}: {Cu} -> {Moi} do {NguoiThucHien} thực hiện.",
                maDonHang, trangThaiCu, trangThaiMoi, nguoiThucHien);

            // Thong bao cho khach moi khi don doi trang thai
            var emailKhach = await LayEmailKhachAsync(don.MaKhachHang);
            if (emailKhach is not null)
            {
                var (tieuDe, noiDung) = MauEmail.CapNhatTrangThai(don.MaDonHang, don.HoTenNguoiNhan,
                    trangThaiMoi, don.TongTien, ghiChu);
                await _email.GuiAsync(emailKhach, tieuDe, noiDung);
            }

            return KetQua.Dung($"Đã chuyển đơn hàng sang trạng thái {DonHangDto.MoTaTrangThai(trangThaiMoi)}.");
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _log.LogError(ex, "Lỗi khi chuyển trạng thái đơn {MaDonHang}.", maDonHang);
            return KetQua.Loi("Không thể cập nhật đơn hàng do lỗi hệ thống.");
        }
        });
    }

    /// <summary>BR06: khach hang chi huy duoc don o trang thai Cho thanh toan hoac Cho xac nhan.</summary>
    public async Task<KetQua> KhachHuyDonAsync(int maDonHang, int maKhachHang, string? lyDo)
    {
        var don = await _db.DonHang.AsNoTracking()
            .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang && d.MaKhachHang == maKhachHang);

        if (don is null) return KetQua.Loi("Không tìm thấy đơn hàng của bạn.");

        if (don.TrangThai is not (TrangThaiDonHang.ChoThanhToan or TrangThaiDonHang.ChoXacNhan))
            return KetQua.Loi(
                "Đơn hàng đã được xác nhận nên không thể tự hủy. Vui lòng liên hệ nhà sách.", "BR06");

        return await ChuyenTrangThaiAsync(maDonHang, TrangThaiDonHang.DaHuy,
            "Khách hàng", null, lyDo ?? "Khách hàng tự hủy đơn");
    }

    /// <summary>BR05: don VNPay chua thanh toan sau 30 phut thi tu dong huy va hoan ton kho.</summary>
    public async Task<int> HuyDonQuaHanAsync()
    {
        var moc = DateTime.Now.AddMinutes(-QuyTacNghiepVu.SoPhutHetHanThanhToan);

        var dsMa = await _db.DonHang
            .Where(d => d.TrangThai == TrangThaiDonHang.ChoThanhToan && d.NgayDat < moc)
            .Select(d => d.MaDonHang)
            .ToListAsync();

        var dem = 0;
        foreach (var ma in dsMa)
        {
            var kq = await ChuyenTrangThaiAsync(ma, TrangThaiDonHang.DaHuy, "Hệ thống", null,
                $"Tự động hủy: quá {QuyTacNghiepVu.SoPhutHetHanThanhToan} phút chưa thanh toán");
            if (kq.ThanhCong) dem++;
        }

        if (dem > 0)
            _log.LogInformation("Đã tự động hủy {SoDon} đơn quá hạn thanh toán.", dem);

        return dem;
    }

    private async Task<string?> LayEmailKhachAsync(int maKhachHang)
        => await _db.KhachHang.AsNoTracking()
            .Where(k => k.MaKhachHang == maKhachHang)
            .Select(k => k.TaiKhoan!.Email)
            .FirstOrDefaultAsync();

    private async Task HoanTonKhoAsync(DonHang don)
    {
        var maSachs = don.CTDonHangs.Select(ct => ct.MaSach).ToList();
        var sachs = await _db.Sach.Where(s => maSachs.Contains(s.MaSach)).ToListAsync();

        foreach (var ct in don.CTDonHangs)
        {
            var sach = sachs.FirstOrDefault(s => s.MaSach == ct.MaSach);
            if (sach is not null) sach.SoLuongTon += ct.SoLuong;
        }
    }

    private static DonHangDto ChuyenDoi(DonHang d, bool kemChiTiet) => new()
    {
        MaDonHang = d.MaDonHang,
        MaKhachHang = d.MaKhachHang,
        TenKhachHang = d.KhachHang?.HoTen ?? string.Empty,
        NgayDat = d.NgayDat,
        TrangThai = d.TrangThai,
        HoTenNguoiNhan = d.HoTenNguoiNhan,
        SoDienThoaiNhan = d.SoDienThoaiNhan,
        DiaChiGiao = d.DiaChiGiao,
        TamTinh = d.TamTinh,
        PhiVanChuyen = d.PhiVanChuyen,
        TongTien = d.TongTien,
        HinhThucThanhToan = d.HinhThucThanhToan,
        GhiChu = d.GhiChu,
        TenQuanLyXuLy = d.QuanLy?.HoTen,
        Dong = !kemChiTiet ? new() : d.CTDonHangs.Select(ct => new DongDonHangDto
        {
            MaSach = ct.MaSach,
            TenSach = ct.TenSach,
            AnhBia = ct.Sach?.AnhBia,
            SoLuong = ct.SoLuong,
            DonGia = ct.DonGia
        }).ToList(),
        LichSu = !kemChiTiet ? new() : d.LichSuDonHangs
            .OrderBy(l => l.ThoiDiem)
            .Select(l => new LichSuDonHangDto
            {
                ThoiDiem = l.ThoiDiem,
                TrangThaiCu = l.TrangThaiCu.HasValue ? DonHangDto.MoTaTrangThai(l.TrangThaiCu.Value) : null,
                TrangThaiMoi = DonHangDto.MoTaTrangThai(l.TrangThaiMoi),
                NguoiThucHien = l.NguoiThucHien,
                GhiChu = l.GhiChu
            }).ToList()
    };
}
