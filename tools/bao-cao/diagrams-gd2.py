# -*- coding: utf-8 -*-
"""\nVe cac hinh cua Chuong 4 (Thiet ke kien truc) trong bao cao giai doan 2.\n\nChay:  python diagrams-gd2.py\nAnh duoc ghi vao thu muc img/ cung cho voi cac hinh cua Chuong 1-3.\n\nFile nay tach rieng voi diagrams.py de khi can ve lai hinh Chuong 4 thi\nkhong phai ve lai toan bo 34 hinh cua Chuong 1-3.\n"""
import os

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyArrowPatch, FancyBboxPatch, Rectangle

plt.rcParams["font.family"] = ["Arial", "Segoe UI Symbol"]
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "img")

BLUE = "#DCE8F7"
EDGE = "#2F4F7F"
YEL = "#FFF6D5"
GRN = "#E3F2E1"
GRAY = "#F2F2F2"
PINK = "#FAE3E3"
PURP = "#EDE3F7"


def save(fig, name):
    fig.savefig(os.path.join(OUT, name + ".png"), dpi=170, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    print("  ", name)


def new(w, h, xlim, ylim, equal=True):
    fig, ax = plt.subplots(figsize=(w, h))
    ax.set_xlim(*xlim)
    ax.set_ylim(*ylim)
    ax.axis("off")
    if equal:
        ax.set_aspect("equal")
    return fig, ax


def arrow(ax, p, q, text=None, style="-|>", ls="-", color="black", rad=0.0, fs=7.5, lw=1.0,
          tx=None, ty=None):
    ax.add_patch(FancyArrowPatch(p, q, arrowstyle=style, mutation_scale=11, color=color, lw=lw,
                                 linestyle=ls, connectionstyle=f"arc3,rad={rad}",
                                 shrinkA=0, shrinkB=0))
    if text:
        mx = (p[0] + q[0]) / 2 + 0.5 * rad * (q[1] - p[1]) if tx is None else tx
        my = (p[1] + q[1]) / 2 - 0.5 * rad * (q[0] - p[0]) if ty is None else ty
        ax.text(mx, my, text, fontsize=fs, ha="center", va="center",
                bbox=dict(fc="white", ec="none", pad=1.0))


def rbox(ax, x, y, w, h, title, sub=None, fc=BLUE, ec=EDGE, fs=8.5, subfs=6.8, lw=1.1):
    """Hop chu nhat bo goc, dong dau in dam, cac dong sau la chu thich nho."""
    ax.add_patch(FancyBboxPatch((x - w / 2, y - h / 2), w, h,
                                boxstyle="round,pad=0.02,rounding_size=0.12",
                                fc=fc, ec=ec, lw=lw))
    if sub:
        ax.text(x, y + h * 0.17, title, ha="center", va="center", fontsize=fs, weight="bold")
        ax.text(x, y - h * 0.20, sub, ha="center", va="center", fontsize=subfs, color="#333")
    else:
        ax.text(x, y, title, ha="center", va="center", fontsize=fs, weight="bold")


def actor(ax, x, y, name, s=0.32):
    ax.add_patch(plt.Circle((x, y + 1.6 * s), 0.35 * s, fc="white", ec="black", lw=1.2))
    ax.plot([x, x], [y + 1.25 * s, y + 0.3 * s], color="black", lw=1.2)
    ax.plot([x - 0.6 * s, x + 0.6 * s], [y + 1.0 * s, y + 1.0 * s], color="black", lw=1.2)
    ax.plot([x, x - 0.5 * s], [y + 0.3 * s, y - 0.4 * s], color="black", lw=1.2)
    ax.plot([x, x + 0.5 * s], [y + 0.3 * s, y - 0.4 * s], color="black", lw=1.2)
    ax.text(x, y - 0.6 * s, name, ha="center", va="top", fontsize=8, weight="bold")


def nhom(ax, x0, y0, x1, y1, nhan, color="#8899AA", fs=7.5, ls="--"):
    """Khung nhom (ranh gioi he thong hoac ranh gioi mot container)."""
    ax.add_patch(Rectangle((x0, y0), x1 - x0, y1 - y0, fc="none", ec=color, lw=1.0, ls=ls))
    ax.text(x0 + 0.12, y1 - 0.22, nhan, fontsize=fs, color=color, ha="left", va="center",
            style="italic")


print("Dang ve hinh Chuong 4...")

# ---------------------------------------------------------------- Hinh 4.1 Kien truc phan lop
fig, ax = new(9.4, 7.0, (0, 14), (0, 10.6))

rbox(ax, 3.6, 9.5, 5.6, 1.1, "BookStore.Web", "Blazor Web App (SSR + Interactive Server)\nGiao diện khách hàng", fc=GRN)
rbox(ax, 10.2, 9.5, 5.6, 1.1, "BookStore.Admin", "WinUI 3 + MVVM\nỨng dụng quản trị Desktop", fc=GRN)

rbox(ax, 10.2, 7.5, 5.6, 1.0, "BookStore.Api", "ASP.NET Core Web API\nXác thực JWT theo Role", fc=YEL)

rbox(ax, 6.9, 5.5, 9.0, 1.1, "BookStore.Services", "Nghiệp vụ, kiểm tra ràng buộc BR01–BR10, DTO\nIDonHangService, ISachService, IKhoService…", fc=BLUE)

rbox(ax, 6.9, 3.5, 9.0, 1.0, "BookStore.Data", "EF Core 10 Code First – BookStoreDbContext, Migrations", fc=BLUE)

rbox(ax, 6.9, 1.8, 9.0, 0.9, "BookStore.Domain", "19 lớp thực thể và các enum trạng thái", fc=BLUE)

rbox(ax, 12.6, 1.8, 2.2, 0.9, "SQL Server", "19 bảng", fc=GRAY)
rbox(ax, 1.3, 1.8, 2.4, 0.9, "Tests, UiTests", "xUnit, Playwright", fc=PINK)

arrow(ax, (3.6, 8.95), (5.2, 6.05), "gọi trực tiếp\nqua DI")
arrow(ax, (10.2, 8.95), (10.2, 8.0), "HTTPS / JSON")
arrow(ax, (10.2, 7.0), (8.6, 6.05))
arrow(ax, (6.9, 4.95), (6.9, 4.0))
arrow(ax, (6.9, 3.0), (6.9, 2.25))
arrow(ax, (11.4, 2.0), (11.5, 2.0), style="-")
ax.annotate("", xy=(11.5, 1.8), xytext=(11.4, 1.8),
            arrowprops=dict(arrowstyle="-|>", color="black", lw=1.0))
arrow(ax, (2.4, 2.0), (2.3, 2.0), style="-")
ax.annotate("", xy=(3.9, 3.2), xytext=(2.3, 2.1),
            arrowprops=dict(arrowstyle="-|>", color="#B04040", lw=1.0, linestyle="--"))
ax.text(2.0, 2.9, "kiểm thử", fontsize=7, color="#B04040", rotation=32)

ax.text(0.2, 10.3, "Tầng trình bày", fontsize=8, color="#555", style="italic")
ax.text(0.2, 5.5, "Tầng nghiệp vụ", fontsize=8, color="#555", style="italic", rotation=90,
        va="center")
ax.text(0.2, 2.9, "Tầng dữ liệu", fontsize=8, color="#555", style="italic")
save(fig, "kt_tongquan")

# ---------------------------------------------------------------- Hinh 4.2 C4 muc 1 - System Context
fig, ax = new(9.2, 6.4, (0, 13), (0, 9.2))

actor(ax, 2.0, 7.6, "Khách hàng")
actor(ax, 2.0, 2.2, "Quản lý")

rbox(ax, 6.7, 5.0, 4.6, 1.6, "Hệ thống quản lý và\nbán sách trực tuyến",
     "[Phần mềm]\nBán sách cho khách và quản lý\nkho, đơn hàng cho nhà sách",
     fc="#1F4E79", ec="#15395B", fs=9.5, subfs=7)
for t in ax.texts[-2:]:
    t.set_color("white")

rbox(ax, 11.3, 7.8, 3.0, 1.1, "Cổng VNPay", "[Hệ thống ngoài]\nThanh toán trực tuyến", fc=GRAY)
rbox(ax, 11.3, 5.0, 3.0, 1.1, "Dịch vụ Email", "[Hệ thống ngoài]\nGửi thư xác nhận", fc=GRAY)
rbox(ax, 11.3, 2.2, 3.0, 1.1, "Đơn vị vận chuyển", "[Hệ thống ngoài]\nGiao hàng cho khách", fc=GRAY)

arrow(ax, (2.6, 7.3), (4.5, 5.6), "Tìm sách, đặt hàng,\ntheo dõi đơn [HTTPS]", rad=0.0)
arrow(ax, (2.6, 2.6), (4.5, 4.4), "Xử lý đơn, quản lý sách,\nxem báo cáo [HTTPS]")
arrow(ax, (9.0, 5.5), (9.8, 7.4), "Chuyển hướng thanh toán,\nnhận kết quả [HTTPS]")
arrow(ax, (9.0, 5.0), (9.8, 5.0), "Gửi email [SMTP]")
arrow(ax, (9.0, 4.5), (9.8, 2.6), "Bàn giao đơn hàng")
save(fig, "c4_l1_context")

# ---------------------------------------------------------------- Hinh 4.3 C4 muc 2 - Container
fig, ax = new(10.0, 7.4, (0, 14.5), (0, 11.0))

actor(ax, 1.6, 8.6, "Khách hàng")
actor(ax, 1.6, 3.4, "Quản lý")

nhom(ax, 3.4, 0.9, 11.6, 10.4, "Hệ thống quản lý và bán sách trực tuyến")

rbox(ax, 5.9, 8.9, 3.9, 1.4, "Trang Web bán sách", "[Container: Blazor Server]\nTìm kiếm, giỏ hàng, đặt hàng,\nthanh toán, đơn của tôi", fc=GRN)
rbox(ax, 9.9, 8.9, 3.0, 1.4, "Ứng dụng quản trị", "[Container: WinUI 3]\nĐơn hàng, sách, kho,\nkhách hàng, báo cáo", fc=GRN)
rbox(ax, 9.9, 6.2, 3.0, 1.2, "Web API", "[Container: ASP.NET Core]\nREST + JWT", fc=YEL)
rbox(ax, 7.5, 4.0, 6.0, 1.2, "Thư viện nghiệp vụ", "[Container: .NET class library]\nBookStore.Services – nơi duy nhất cài đặt BR01–BR10", fc=BLUE)
rbox(ax, 7.5, 1.8, 6.0, 1.0, "Cơ sở dữ liệu", "[Container: SQL Server 2025] – 19 bảng", fc=GRAY)

rbox(ax, 13.1, 10.3, 2.2, 0.9, "Cổng VNPay", "[Hệ thống ngoài]", fc="#EDEDED")

arrow(ax, (2.3, 8.6), (3.9, 8.9), "HTTPS")
arrow(ax, (2.3, 3.8), (8.4, 8.4), "Mở ứng dụng")
arrow(ax, (5.9, 8.2), (6.4, 4.7), "gọi trực tiếp qua DI")
arrow(ax, (9.9, 8.2), (9.9, 6.85), "JSON/HTTPS\nkèm token JWT")
arrow(ax, (9.9, 5.6), (8.8, 4.65), "gọi qua DI")
arrow(ax, (7.5, 3.4), (7.5, 2.35), "EF Core / TDS")
arrow(ax, (6.9, 9.6), (12.0, 10.3), "Chuyển hướng thanh toán", fs=7.5)
save(fig, "c4_l2_container")

# ---------------------------------------------------------------- Hinh 4.4 C4 muc 3 - Component
fig, ax = new(10.4, 7.2, (0, 15), (0, 10.4))

rbox(ax, 4.0, 9.6, 6.4, 0.9, "BookStore.Web (Blazor)", "Trang Razor + endpoint nhận form, cổng thanh toán giả lập", fc=GRN, fs=8)
rbox(ax, 11.4, 9.6, 6.4, 0.9, "BookStore.Api", "SachController, DonHangController, QuanTriController, XacThucController", fc=YEL, fs=8)

nhom(ax, 0.4, 0.8, 14.6, 8.7, "BookStore.Services – các thành phần bên trong")

# Hang 1: dich vu ban hang va bao cao
cot = [(2.4, "TaiKhoanService", "Đăng nhập, đăng ký,\nkhóa, quên mật khẩu"),
       (5.2, "SachService", "Tìm kiếm, CRUD sách,\nBR09"),
       (8.0, "GioHangService", "Thêm, sửa, xóa\ndòng giỏ (BR01, BR02)"),
       (10.8, "DonHangService", "Đặt hàng, sơ đồ trạng thái\nBR02–BR07"),
       (13.4, "BaoCaoService", "Dashboard, doanh thu,\nnhập–xuất–tồn (BR10)")]
for x, ten, mo in cot:
    rbox(ax, x, 7.4, 2.5, 1.2, ten, mo, fs=7.4, subfs=6.0,
         fc="#CBDDF2" if ten == "DonHangService" else BLUE)

# Hang 2: phan he quan tri va tich hop he thong ngoai
cot2 = [(1.85, "DanhMucService", "Loại sách, tác giả,\nNXB, nhà cung cấp"),
        (4.15, "KhoService", "Tồn kho, phiếu nhập\n(BR08)"),
        (6.45, "KhachHangService", "Tra cứu, khóa\nkhách hàng"),
        (8.75, "ThanhToanService", "VNPay, ký\nHMAC-SHA512"),
        (11.05, "EmailService", "MailKit / tệp .eml"),
        (13.35, "XuatExcelService", "ClosedXML")]
for x, ten, mo in cot2:
    rbox(ax, x, 5.6, 2.15, 1.1, ten, mo, fs=7.0, subfs=6.0,
         fc="#E4EEF8" if ten in ("ThanhToanService", "EmailService", "XuatExcelService") else BLUE)

rbox(ax, 3.2, 3.7, 3.6, 0.9, "QuyTacNghiepVu", "hằng số BR05, BR07", fc=YEL, fs=7.6, subfs=6.2)
rbox(ax, 7.5, 3.7, 3.6, 0.9, "KetQua / KetQua<T>", "kiểu trả về chung", fc=YEL, fs=7.6, subfs=6.2)
rbox(ax, 11.8, 3.7, 3.6, 0.9, "Các lớp DTO", "dữ liệu trao đổi giữa các tầng", fc=YEL, fs=7.6, subfs=6.2)

rbox(ax, 7.5, 1.7, 7.0, 0.9, "BookStoreDbContext", "BookStore.Data – EF Core 10", fc=GRAY, fs=8, subfs=6.4)

# Trang Web goi cac dich vu ban hang, Web API goi cac dich vu quan tri
for x in (2.4, 5.2, 8.0, 10.8):
    arrow(ax, (4.0, 9.15), (x, 8.0), rad=0.0, lw=0.9)
for x in (5.2, 10.8, 13.4):
    arrow(ax, (11.4, 9.15), (x, 8.0), rad=0.0, lw=0.9, color="#8A6D1F")

# Moi dich vu deu dung DbContext
arrow(ax, (7.5, 3.25), (7.5, 2.15), "mọi dịch vụ truy cập dữ liệu qua DbContext", fs=6.6, rad=0.0, lw=0.8, color="#777")

ax.text(0.6, 10.15, "Mũi tên đen: trang Web gọi trực tiếp qua DI    ·    Mũi tên vàng: Web API gọi qua DI (Api còn gọi các dịch vụ ở hàng dưới)",
        fontsize=6.6, color="#555", style="italic")
save(fig, "c4_l3_component")

# ---------------------------------------------------------------- Hinh 4.5 C4 muc 4 - Code
def cls(ax, x, y, w, ten, thuoc_tinh, phuong_thuc, fc=BLUE, fs=6.8):
    n = len(thuoc_tinh) + len(phuong_thuc)
    h = 0.52 + 0.30 * n + 0.14
    ax.add_patch(Rectangle((x - w / 2, y - h / 2), w, h, fc=fc, ec=EDGE, lw=1.0))
    ax.add_patch(Rectangle((x - w / 2, y + h / 2 - 0.52), w, 0.52, fc="#C9D9EE", ec=EDGE, lw=1.0))
    ax.text(x, y + h / 2 - 0.26, ten, ha="center", va="center", fontsize=fs + 0.8, weight="bold")
    cy = y + h / 2 - 0.52
    for t in thuoc_tinh:
        cy -= 0.30
        ax.text(x - w / 2 + 0.15, cy + 0.03, t, ha="left", va="center", fontsize=fs)
    if thuoc_tinh and phuong_thuc:
        ax.plot([x - w / 2, x + w / 2], [cy - 0.08, cy - 0.08], color=EDGE, lw=0.9)
        cy -= 0.08
    for t in phuong_thuc:
        cy -= 0.30
        ax.text(x - w / 2 + 0.15, cy + 0.03, t, ha="left", va="center", fontsize=fs)
    return h


fig, ax = new(10.2, 6.8, (0, 15), (0, 10.0))

cls(ax, 3.4, 7.9, 6.0, "«interface» IDonHangService", [],
    ["+ DatHangAsync(maKH, dto) : KetQua<DonHangDto>",
     "+ ChuyenTrangThaiAsync(maDon, trangThaiMoi, …) : KetQua",
     "+ KhachHuyDonAsync(maDon, maKH, lyDo) : KetQua",
     "+ HuyDonQuaHanAsync() : int"], fc=YEL)

cls(ax, 3.4, 4.3, 6.0, "DonHangService",
    ["- _db : BookStoreDbContext",
     "- _log : ILogger<DonHangService>",
     "- BuocChuyenHopLe : Dictionary<TrangThai, TrangThai[]>"],
    ["+ DatHangAsync(…)  « transaction »",
     "+ ChuyenTrangThaiAsync(…)  « transaction »",
     "- HoanTonKhoAsync(don)   // BR03"])

cls(ax, 11.4, 8.3, 5.4, "DonHang",
    ["+ MaDonHang : int", "+ TrangThai : TrangThaiDonHang",
     "+ TamTinh, PhiVanChuyen, TongTien : decimal",
     "+ CTDonHangs : ICollection<CTDonHang>",
     "+ LichSuDonHangs : ICollection<LichSuDonHang>"], [], fc=GRN)

cls(ax, 11.4, 4.9, 5.4, "CTDonHang",
    ["+ MaSach : int", "+ TenSach : string", "+ SoLuong : int",
     "+ DonGia : decimal   // BR04"], ["+ ThanhTien : decimal"], fc=GRN)

cls(ax, 11.4, 2.0, 5.4, "«enumeration» TrangThaiDonHang",
    ["ChoThanhToan = 0", "ChoXacNhan = 1", "DaXacNhan = 2",
     "DangGiao = 3", "HoanThanh = 4", "DaHuy = 5"], [], fc=PURP)

cls(ax, 3.4, 1.3, 6.0, "QuyTacNghiepVu  «static»",
    ["+ PhiVanChuyen = 25.000", "+ NguongMienPhiVanChuyen = 300.000"],
    ["+ TinhPhiVanChuyen(tamTinh) : decimal   // BR07"], fc=YEL)

ax.annotate("", xy=(3.4, 6.65), xytext=(3.4, 5.75),
            arrowprops=dict(arrowstyle="-|>", color="black", lw=1.1, linestyle="--",
                            mutation_scale=16, fc="white"))
ax.text(3.6, 6.2, "hiện thực", fontsize=7, style="italic")

arrow(ax, (6.5, 4.8), (8.6, 7.4), "sử dụng")
arrow(ax, (6.5, 3.4), (8.6, 2.2), "sử dụng")
arrow(ax, (3.4, 2.85), (3.4, 2.25), "gọi")
arrow(ax, (11.4, 6.9), (11.4, 6.3), "1 .. *", style="-")
save(fig, "c4_l4_code")


# ---------------------------------------------------------------- Sequence diagram helper
def sequence(name, cols, steps, w=10.4, hstep=0.72, top_pad=1.5, note=None):
    """cols: [(nhan, dong_phu)]; steps: [(tu, den, nhan, loai)] loai: call|ret|self."""
    n = len(cols)
    gap = 14.0 / n
    xs = [gap * (i + 0.5) for i in range(n)]
    h = top_pad + hstep * len(steps) + 0.9
    fig, ax = new(w, max(3.0, h * 0.52), (0, 14), (0, h), equal=False)

    for x, (nhan, phu) in zip(xs, cols):
        ax.add_patch(FancyBboxPatch((x - gap * 0.44, h - 0.95), gap * 0.88, 0.80,
                                    boxstyle="round,pad=0.01,rounding_size=0.08",
                                    fc=BLUE, ec=EDGE, lw=1.0))
        ax.text(x, h - 0.44, nhan, ha="center", va="center", fontsize=7.6, weight="bold")
        if phu:
            ax.text(x, h - 0.73, phu, ha="center", va="center", fontsize=6.2, color="#444")
        ax.plot([x, x], [0.45, h - 0.95], color="#888", lw=0.9, ls=(0, (4, 3)))

    y = h - top_pad
    for tu, den, nhan, loai in steps:
        if loai == "self":
            x = xs[tu]
            ax.plot([x, x + 0.55, x + 0.55, x], [y, y, y - 0.28, y - 0.28], color="black", lw=1.0)
            ax.annotate("", xy=(x, y - 0.28), xytext=(x + 0.18, y - 0.28),
                        arrowprops=dict(arrowstyle="-|>", color="black", lw=1.0))
            ax.text(x + 0.72, y - 0.08, nhan, fontsize=6.8, ha="left", va="center")
        else:
            p, q = xs[tu], xs[den]
            ls = "--" if loai == "ret" else "-"
            ax.annotate("", xy=(q, y), xytext=(p, y),
                        arrowprops=dict(arrowstyle="-|>", color="black", lw=1.0, linestyle=ls))
            ax.text((p + q) / 2, y + 0.13, nhan, fontsize=6.8, ha="center", va="bottom")
        y -= hstep

    if note:
        ax.text(0.1, 0.18, note, fontsize=6.6, ha="left", va="center", style="italic",
                color="#555")
    save(fig, name)


sequence("seq_dathang",
         [("Khách hàng", "[Trình duyệt]"), ("BookStore.Web", "[Blazor]"),
          ("DonHangService", "[Services]"), ("BookStoreDbContext", "[EF Core]"),
          ("SQL Server", "[CSDL]")],
         [(0, 1, "POST /dat-hang (họ tên, địa chỉ, hình thức TT)", "call"),
          (1, 1, "kiểm tra cookie xác thực, lấy mã khách hàng", "self"),
          (1, 2, "DatHangAsync(maKhachHang, dto)", "call"),
          (2, 3, "BeginTransactionAsync()", "call"),
          (2, 3, "đọc giỏ hàng và danh sách sách", "call"),
          (3, 4, "SELECT GioHang, CTGioHang, Sach", "call"),
          (2, 2, "BR02: kiểm tra tồn kho từng dòng", "self"),
          (2, 2, "BR04: sao lưu đơn giá; BR07: tính phí vận chuyển", "self"),
          (2, 3, "thêm DonHang, CTDonHang, ThanhToan, LichSuDonHang", "call"),
          (2, 3, "trừ SoLuongTon, xóa dòng giỏ hàng", "call"),
          (3, 4, "INSERT / UPDATE trong cùng transaction", "call"),
          (2, 3, "CommitAsync()", "call"),
          (2, 1, "KetQua<DonHangDto> (thành công, mã đơn)", "ret"),
          (1, 0, "Redirect /ket-qua-dat-hang/{mã đơn}", "ret")],
         note="Nếu bất kỳ bước kiểm tra nào thất bại, transaction được hủy (rollback), tồn kho giữ nguyên "
              "và người dùng nhận thông báo lỗi nghiệp vụ.")

sequence("seq_xulydon",
         [("Quản lý", "[WinUI 3]"), ("DonHangViewModel", "[MVVM]"), ("ApiClient", "[HttpClient]"),
          ("DonHangController", "[Web API]"), ("DonHangService", "[Services]"), ("SQL Server", "")],
         [(0, 1, "bấm nút Xác nhận", "call"),
          (1, 2, "ChuyenTrangThaiDonAsync(maDon, 2, ghiChu)", "call"),
          (2, 2, "gắn header Authorization: Bearer <token>", "self"),
          (2, 3, "PUT /api/donhang/{maDon}/trangthai", "call"),
          (3, 3, "kiểm tra Role QUẢN LÝ", "self"),
          (3, 4, "ChuyenTrangThaiAsync(…)", "call"),
          (4, 4, "đối chiếu sơ đồ trạng thái hợp lệ", "self"),
          (4, 5, "UPDATE DonHang, INSERT LichSuDonHang", "call"),
          (4, 3, "KetQua (thành công)", "ret"),
          (3, 2, "200 OK { thongBao }", "ret"),
          (2, 1, "KetQuaApi<PhanHoi>", "ret"),
          (1, 0, "cập nhật danh sách và chi tiết đơn", "ret")],
         note="Bước chuyển không hợp lệ bị tầng Services từ chối và API trả về 400 kèm thông báo tiếng Việt.")

# ---------------------------------------------------------------- Hinh trien khai
fig, ax = new(9.6, 5.6, (0, 14), (0, 8.2))

nhom(ax, 0.4, 0.6, 6.2, 7.6, "Máy người dùng")
rbox(ax, 3.3, 6.3, 4.8, 1.0, "Trình duyệt web", "Chrome / Edge – khách hàng", fc=GRN)
rbox(ax, 3.3, 3.9, 4.8, 1.2, "BookStore.Admin.exe", "WinUI 3, triển khai dạng\nunpackaged self-contained", fc=GRN)
rbox(ax, 3.3, 1.7, 4.8, 0.9, ".NET 10 Desktop Runtime", "kèm sẵn trong gói self-contained", fc=GRAY, fs=7.5)

nhom(ax, 7.0, 0.6, 13.6, 7.6, "Máy chủ ứng dụng")
rbox(ax, 10.3, 6.3, 5.4, 1.0, "Kestrel – BookStore.Web", "cổng 5159 (HTTP) / 7132 (HTTPS)", fc=YEL)
rbox(ax, 10.3, 4.3, 5.4, 1.0, "Kestrel – BookStore.Api", "cổng 5171 (HTTP) / 7060 (HTTPS)", fc=YEL)
rbox(ax, 10.3, 2.0, 5.4, 1.1, "SQL Server 2025", "CSDL QuanLyBanSachTrucTuyen\n19 bảng", fc=GRAY)

arrow(ax, (5.8, 6.3), (7.6, 6.3), "HTTPS")
arrow(ax, (5.8, 4.1), (7.6, 4.3), "HTTPS + JWT")
arrow(ax, (10.3, 5.8), (10.3, 4.85), "")
arrow(ax, (9.2, 3.8), (9.2, 2.6), "EF Core")
arrow(ax, (11.4, 5.8), (11.4, 2.6), "EF Core", rad=0.18)
save(fig, "trienkhai")

# ---------------------------------------------------------------- Hinh log va xu ly loi
fig, ax = new(9.6, 5.4, (0, 14), (0, 7.6))

rbox(ax, 2.2, 6.4, 3.6, 0.9, "Yêu cầu của người dùng", fc=GRN, fs=8)
rbox(ax, 2.2, 4.3, 3.6, 1.0, "Tầng Services", "thực hiện nghiệp vụ", fc=BLUE)

rbox(ax, 7.2, 5.6, 4.0, 1.1, "Lỗi nghiệp vụ dự kiến", "trả về KetQua.Loi(thông báo)\nkhông ném ngoại lệ", fc=YEL)
rbox(ax, 7.2, 2.9, 4.0, 1.1, "Ngoại lệ ngoài dự kiến", "mất kết nối, lỗi lập trình", fc=PINK)

rbox(ax, 12.0, 5.6, 3.4, 1.0, "Thông báo tiếng Việt", "hiện ngay trên màn hình", fc=GRN)
rbox(ax, 12.0, 2.9, 3.4, 1.2, "Middleware xử lý lỗi", "ghi log kèm mã truy vết,\ntrả ProblemDetails / trang lỗi", fc=GRAY)

rbox(ax, 7.2, 0.8, 8.8, 0.8, "Nhật ký ứng dụng (ILogger) – Thông tin · Cảnh báo · Lỗi", fc=GRAY, fs=8)

arrow(ax, (2.2, 5.95), (2.2, 4.8))
arrow(ax, (4.0, 4.5), (5.2, 5.4))
arrow(ax, (4.0, 4.1), (5.2, 3.2))
arrow(ax, (9.2, 5.6), (10.3, 5.6))
arrow(ax, (9.2, 2.9), (10.3, 2.9))
arrow(ax, (7.2, 5.05), (7.2, 1.2), "ghi mức Cảnh báo", rad=-0.35)
arrow(ax, (12.0, 2.3), (9.0, 1.2), "ghi mức Lỗi")
arrow(ax, (2.2, 3.8), (3.0, 1.2), "ghi mức Thông tin\n(đăng nhập, tạo đơn,\nđổi trạng thái)", fs=6.8)
save(fig, "log_loi")

print("Xong, da ve 9 hinh cua Chuong 4.")
