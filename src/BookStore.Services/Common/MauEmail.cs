using System.Net;
using BookStore.Domain.Enums;
using BookStore.Services.Dtos;

namespace BookStore.Services.Common;

/// <summary>Noi dung HTML cua cac email he thong gui cho khach hang.</summary>
public static class MauEmail
{
    private static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string Khung(string tieuDe, string than) => $"""
        <div style="font-family:Segoe UI,Arial,sans-serif;max-width:620px;margin:auto;color:#222">
          <div style="background:#1b6b73;color:#fff;padding:14px 20px;border-radius:8px 8px 0 0">
            <strong style="font-size:18px">Nhà sách trực tuyến</strong>
          </div>
          <div style="border:1px solid #ddd;border-top:none;padding:20px;border-radius:0 0 8px 8px">
            <h2 style="margin-top:0;font-size:18px">{E(tieuDe)}</h2>
            {than}
            <p style="color:#777;font-size:12px;margin-top:24px">
              Email này được gửi tự động, vui lòng không trả lời.
            </p>
          </div>
        </div>
        """;

    public static (string TieuDe, string NoiDung) XacNhanDonHang(DonHangDto d)
    {
        var dong = string.Join("", d.Dong.Select(x =>
            $"<tr><td style='padding:4px 0'>{E(x.TenSach)}</td><td align='center'>{x.SoLuong}</td>" +
            $"<td align='right'>{x.ThanhTien:#,##0} đ</td></tr>"));

        var loiNhac = d.TrangThai == TrangThaiDonHang.ChoThanhToan
            ? $"<p>Đơn hàng đang <strong>chờ thanh toán qua VNPay</strong>. Đơn sẽ tự hủy nếu chưa thanh toán sau {QuyTacNghiepVu.SoPhutHetHanThanhToan} phút.</p>"
            : "<p>Nhà sách sẽ xác nhận và giao hàng cho bạn trong thời gian sớm nhất.</p>";

        var than = $"""
            <p>Xin chào {E(d.HoTenNguoiNhan)},</p>
            <p>Cảm ơn bạn đã đặt hàng. Mã đơn hàng của bạn là <strong>#{d.MaDonHang}</strong>.</p>
            {loiNhac}
            <table style="width:100%;border-collapse:collapse;font-size:14px">
              <tr style="border-bottom:1px solid #ddd"><th align="left">Sách</th><th>SL</th><th align="right">Thành tiền</th></tr>
              {dong}
              <tr><td colspan="2" align="right">Tạm tính</td><td align="right">{d.TamTinh:#,##0} đ</td></tr>
              <tr><td colspan="2" align="right">Phí vận chuyển</td><td align="right">{d.PhiVanChuyen:#,##0} đ</td></tr>
              <tr><td colspan="2" align="right"><strong>Tổng cộng</strong></td><td align="right"><strong>{d.TongTien:#,##0} đ</strong></td></tr>
            </table>
            <p>Giao tới: {E(d.HoTenNguoiNhan)} – {E(d.SoDienThoaiNhan)}<br/>{E(d.DiaChiGiao)}</p>
            <p>Hình thức thanh toán: {E(d.TenHinhThucThanhToan)}</p>
            """;
        return ($"Xác nhận đơn hàng #{d.MaDonHang}", Khung($"Đặt hàng thành công – đơn #{d.MaDonHang}", than));
    }

    public static (string TieuDe, string NoiDung) CapNhatTrangThai(int maDonHang, string hoTen,
        TrangThaiDonHang trangThaiMoi, decimal tongTien, string? ghiChu)
    {
        var moTa = trangThaiMoi switch
        {
            TrangThaiDonHang.ChoXacNhan => "Nhà sách đã nhận được thanh toán của bạn, đơn hàng đang chờ xác nhận.",
            TrangThaiDonHang.DaXacNhan => "Đơn hàng đã được xác nhận và đang được đóng gói.",
            TrangThaiDonHang.DangGiao => "Đơn hàng đã được bàn giao cho đơn vị vận chuyển.",
            TrangThaiDonHang.HoanThanh => "Đơn hàng đã được giao thành công. Cảm ơn bạn đã mua sách!",
            TrangThaiDonHang.DaHuy => "Đơn hàng đã bị hủy. Số tiền đã thanh toán trực tuyến (nếu có) sẽ được hoàn lại.",
            _ => "Đơn hàng vừa được cập nhật trạng thái."
        };
        var than = $"""
            <p>Xin chào {E(hoTen)},</p>
            <p>Đơn hàng <strong>#{maDonHang}</strong> (tổng {tongTien:#,##0} đ) chuyển sang trạng thái
               <strong>{E(DonHangDto.MoTaTrangThai(trangThaiMoi))}</strong>.</p>
            <p>{moTa}</p>
            {(string.IsNullOrWhiteSpace(ghiChu) ? "" : $"<p>Ghi chú: {E(ghiChu)}</p>")}
            """;
        return ($"Đơn hàng #{maDonHang}: {DonHangDto.MoTaTrangThai(trangThaiMoi)}",
            Khung($"Cập nhật đơn hàng #{maDonHang}", than));
    }

    public static (string TieuDe, string NoiDung) DatLaiMatKhau(string hoTen, string lienKet, int soPhutHieuLuc)
    {
        var than = $"""
            <p>Xin chào {E(hoTen)},</p>
            <p>Bạn vừa yêu cầu đặt lại mật khẩu. Bấm vào liên kết dưới đây để đặt mật khẩu mới
               (liên kết có hiệu lực trong {soPhutHieuLuc} phút):</p>
            <p><a href="{E(lienKet)}" style="background:#1b6b73;color:#fff;padding:10px 16px;border-radius:6px;text-decoration:none">Đặt lại mật khẩu</a></p>
            <p style="font-size:12px;color:#555">Hoặc mở liên kết: {E(lienKet)}</p>
            <p>Nếu bạn không yêu cầu, hãy bỏ qua email này, mật khẩu của bạn không thay đổi.</p>
            """;
        return ("Đặt lại mật khẩu", Khung("Đặt lại mật khẩu", than));
    }
}
