using BookStore.Services.Dtos;
using BookStore.Services.Interfaces;
using ClosedXML.Excel;

namespace BookStore.Services.Implementations;

/// <summary>
/// Xuat bao cao ra tap tin Excel bang thu vien ClosedXML (ma nguon mo, giay phep MIT).
/// Tra ve mang byte de Web API gui ve ung dung quan tri dang tap tin .xlsx.
/// </summary>
public class XuatExcelService : IXuatExcelService
{
    private readonly IBaoCaoService _baoCao;

    public XuatExcelService(IBaoCaoService baoCao) => _baoCao = baoCao;

    private const string DinhDangTien = "#,##0 \"đ\"";

    public async Task<byte[]> XuatDoanhThuAsync(DateTime tuNgay, DateTime denNgay)
    {
        var bc = await _baoCao.LayBaoCaoDoanhThuAsync(tuNgay, denNgay);

        using var wb = new XLWorkbook();

        // Trang 1: tong hop va doanh thu theo ngay
        var ws = wb.Worksheets.Add("Doanh thu");
        TieuDe(ws, "BÁO CÁO DOANH THU", $"Từ ngày {bc.TuNgay:dd/MM/yyyy} đến ngày {bc.DenNgay:dd/MM/yyyy}", 3);

        ws.Cell(4, 1).Value = "Tổng doanh thu";
        ws.Cell(4, 2).Value = bc.TongDoanhThu;
        ws.Cell(4, 2).Style.NumberFormat.Format = DinhDangTien;
        ws.Cell(5, 1).Value = "Số đơn hoàn thành";
        ws.Cell(5, 2).Value = bc.SoDonHoanThanh;
        ws.Cell(6, 1).Value = "Số sách bán ra";
        ws.Cell(6, 2).Value = bc.SoSachBanRa;
        ws.Cell(7, 1).Value = "Giá trị trung bình mỗi đơn";
        ws.Cell(7, 2).Value = bc.GiaTriTrungBinhDon;
        ws.Cell(7, 2).Style.NumberFormat.Format = DinhDangTien;
        ws.Range(4, 1, 7, 1).Style.Font.Bold = true;

        var dong = 9;
        TieuDeBang(ws, dong, "Ngày", "Số đơn", "Doanh thu");
        foreach (var n in bc.TheoNgay)
        {
            dong++;
            ws.Cell(dong, 1).Value = n.Ngay;
            ws.Cell(dong, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(dong, 2).Value = n.SoDon;
            ws.Cell(dong, 3).Value = n.DoanhThu;
            ws.Cell(dong, 3).Style.NumberFormat.Format = DinhDangTien;
        }
        KeBang(ws, 9, dong, 3);
        ws.Columns().AdjustToContents();

        // Trang 2: sach ban chay
        var ws2 = wb.Worksheets.Add("Sách bán chạy");
        TieuDe(ws2, "SÁCH BÁN CHẠY", $"Từ ngày {bc.TuNgay:dd/MM/yyyy} đến ngày {bc.DenNgay:dd/MM/yyyy}", 4);
        dong = 4;
        TieuDeBang(ws2, dong, "STT", "Tên sách", "Số lượng bán", "Doanh thu");
        var stt = 0;
        foreach (var s in bc.TopSachBanChay)
        {
            dong++;
            ws2.Cell(dong, 1).Value = ++stt;
            ws2.Cell(dong, 2).Value = s.TenSach;
            ws2.Cell(dong, 3).Value = s.SoLuongBan;
            ws2.Cell(dong, 4).Value = s.DoanhThu;
            ws2.Cell(dong, 4).Style.NumberFormat.Format = DinhDangTien;
        }
        KeBang(ws2, 4, dong, 4);
        ws2.Columns().AdjustToContents();

        return LuuThanhByte(wb);
    }

    public async Task<byte[]> XuatNhapXuatTonAsync(DateTime tuNgay, DateTime denNgay)
    {
        var ds = await _baoCao.LayNhapXuatTonAsync(tuNgay, denNgay);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Nhập xuất tồn");
        TieuDe(ws, "BÁO CÁO NHẬP – XUẤT – TỒN", $"Từ ngày {tuNgay:dd/MM/yyyy} đến ngày {denNgay:dd/MM/yyyy}", 6);

        var dong = 4;
        TieuDeBang(ws, dong, "Mã sách", "Tên sách", "Tồn đầu kỳ", "Nhập trong kỳ", "Xuất trong kỳ", "Tồn cuối kỳ");
        foreach (var x in ds)
        {
            dong++;
            ws.Cell(dong, 1).Value = x.MaSach;
            ws.Cell(dong, 2).Value = x.TenSach;
            ws.Cell(dong, 3).Value = x.TonDauKy;
            ws.Cell(dong, 4).Value = x.NhapTrongKy;
            ws.Cell(dong, 5).Value = x.XuatTrongKy;
            ws.Cell(dong, 6).Value = x.TonCuoiKy;
        }

        // Dong tong cong dung cong thuc Excel de nguoi dung kiem tra lai duoc
        dong++;
        ws.Cell(dong, 2).Value = "Tổng cộng";
        for (var c = 3; c <= 6; c++)
            ws.Cell(dong, c).FormulaA1 = $"SUM({ws.Cell(5, c).Address}:{ws.Cell(dong - 1, c).Address})";
        ws.Row(dong).Style.Font.Bold = true;

        KeBang(ws, 4, dong, 6);
        ws.Columns().AdjustToContents();
        return LuuThanhByte(wb);
    }

    private static void TieuDe(IXLWorksheet ws, string tieuDe, string phuDe, int soCot)
    {
        ws.Cell(1, 1).Value = tieuDe;
        ws.Range(1, 1, 1, soCot).Merge().Style.Font.SetBold().Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(2, 1).Value = phuDe;
        ws.Range(2, 1, 2, soCot).Merge().Style.Font.SetItalic()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    }

    private static void TieuDeBang(IXLWorksheet ws, int dong, params string[] cot)
    {
        for (var i = 0; i < cot.Length; i++) ws.Cell(dong, i + 1).Value = cot[i];
        var r = ws.Range(dong, 1, dong, cot.Length);
        r.Style.Font.Bold = true;
        r.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E2F3");
    }

    private static void KeBang(IXLWorksheet ws, int dongDau, int dongCuoi, int soCot)
    {
        var r = ws.Range(dongDau, 1, Math.Max(dongDau, dongCuoi), soCot);
        r.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        r.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    }

    private static byte[] LuuThanhByte(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
