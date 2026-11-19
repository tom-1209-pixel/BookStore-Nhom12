using System.Diagnostics;
using System.Net;
using System.Text;
using BookStore.Admin.Models;

namespace BookStore.Admin.Helpers;

/// <summary>Cac tien ich dung chung: mo tap tin, luu bao cao, in hoa don.</summary>
public static class TienIch
{
    /// <summary>
    /// Thu muc luu bao cao Excel va hoa don: Documents\BookStore. Neu khong tao duoc (Documents
    /// bi OneDrive chuyen huong) thi dung thu muc BaoCao canh tap tin BookStore.Admin.exe.
    /// </summary>
    public static string ThuMucLuu
    {
        get
        {
            try
            {
                var docs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BookStore");
                Directory.CreateDirectory(docs);
                return docs;
            }
            catch (Exception)
            {
                var duPhong = Path.Combine(AppContext.BaseDirectory, "BaoCao");
                Directory.CreateDirectory(duPhong);
                return duPhong;
            }
        }
    }

    /// <summary>Mo tap tin bang chuong trinh mac dinh cua Windows (Excel, trinh duyet...).</summary>
    public static void MoTep(string duongDan)
        => Process.Start(new ProcessStartInfo(duongDan) { UseShellExecute = true });

    public static string LuuTep(string tenTep, byte[] noiDung)
    {
        var duongDan = Path.Combine(ThuMucLuu, tenTep);
        File.WriteAllBytes(duongDan, noiDung);
        return duongDan;
    }

    /// <summary>
    /// In hoa don ban hang: sinh trang HTML co dinh dang in roi mo bang trinh duyet,
    /// trang tu goi lenh in khi mo. Cach nay khong can thu vien bao cao rieng.
    /// </summary>
    public static string TaoHoaDon(DonHang d)
    {
        static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        var dong = new StringBuilder();
        var stt = 0;
        foreach (var x in d.Dong)
            dong.Append($"<tr><td class='c'>{++stt}</td><td>{E(x.TenSach)}</td><td class='p'>{x.SoLuong}</td>" +
                        $"<td class='p'>{x.DonGia:#,##0}</td><td class='p'>{x.ThanhTien:#,##0}</td></tr>");

        var html = $$"""
            <!doctype html>
            <html lang="vi"><head><meta charset="utf-8"><title>Hóa đơn #{{d.MaDonHang}}</title>
            <style>
              body{font-family:"Times New Roman",serif;margin:32px;color:#000}
              h1{text-align:center;font-size:22px;margin:6px 0}
              .c{text-align:center} .p{text-align:right}
              table{width:100%;border-collapse:collapse;margin-top:12px}
              th,td{border:1px solid #444;padding:6px;font-size:14px}
              th{background:#eee}
              .ky{display:flex;justify-content:space-around;margin-top:40px;text-align:center}
              @media print{ button{display:none} }
            </style></head>
            <body onload="window.print()">
              <div class="c"><strong>NHÀ SÁCH TRỰC TUYẾN</strong><br/>273 An Dương Vương, Phường 3, Quận 5, TP. Hồ Chí Minh</div>
              <h1>HÓA ĐƠN BÁN HÀNG</h1>
              <div class="c">Số: {{d.MaDonHang:D6}} – Ngày đặt: {{d.NgayDat:dd/MM/yyyy HH:mm}}</div>
              <p>Người nhận: <strong>{{E(d.HoTenNguoiNhan)}}</strong> – Điện thoại: {{E(d.SoDienThoaiNhan)}}<br/>
                 Địa chỉ giao hàng: {{E(d.DiaChiGiao)}}<br/>
                 Hình thức thanh toán: {{E(d.TenHinhThucThanhToan)}}</p>
              <table>
                <tr><th>STT</th><th>Tên sách</th><th>SL</th><th>Đơn giá (đ)</th><th>Thành tiền (đ)</th></tr>
                {{dong}}
                <tr><td colspan="4" class="p">Tạm tính</td><td class="p">{{d.TamTinh:#,##0}}</td></tr>
                <tr><td colspan="4" class="p">Phí vận chuyển</td><td class="p">{{d.PhiVanChuyen:#,##0}}</td></tr>
                <tr><td colspan="4" class="p"><strong>Tổng cộng</strong></td><td class="p"><strong>{{d.TongTien:#,##0}}</strong></td></tr>
              </table>
              <div class="ky">
                <div>Người mua hàng<br/><i>(Ký, ghi rõ họ tên)</i></div>
                <div>Người lập hóa đơn<br/><i>(Ký, ghi rõ họ tên)</i><br/><br/><br/>{{E(d.TenQuanLyXuLy)}}</div>
              </div>
              <p class="c"><button onclick="window.print()">In hóa đơn</button></p>
            </body></html>
            """;

        var duongDan = Path.Combine(ThuMucLuu, $"HoaDon_{d.MaDonHang:D6}.html");
        File.WriteAllText(duongDan, html, Encoding.UTF8);
        return duongDan;
    }
}
