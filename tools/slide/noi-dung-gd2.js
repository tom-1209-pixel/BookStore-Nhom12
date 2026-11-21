// Cac slide rieng cua giai doan 2: Chuong 4 (Thiet ke kien truc),
// Chuong 5 (Trien khai, kiem thu, ket qua), Chuong 6 (Ket luan) va slide ket.
//
// build-slide-gd2.js goi ham nay va truyen vao cac tien ich dung chung
// (pres, card, tx, list, fitImage, hang so bo cuc) de giu nguyen phong cach
// thiet ke cua bo slide giai doan 1.

module.exports = function themSlideGD2(h) {
  const { pres, C, THEME, M, H, card, tx, list, fitImage } = h;

  /* ============================================ Chuong 4 */
  pres.addSection({ title: "Chương 4 – Thiết kế" });
  {
    const s = pres.addSlide({ masterName: "DIVIDER", sectionTitle: "Chương 4 – Thiết kế" });
    tx(s, "CHƯƠNG 4", { placeholder: "eyebrow" });
    tx(s, "Thiết kế kiến trúc", { placeholder: "title" });
    tx(s, "Kiến trúc tổng quan · Mô hình C4 bốn mức · Giao tiếp · Dữ liệu · Log và lỗi",
      { placeholder: "body" });
    s.addNotes("Chuong 4 tra loi cau hoi he thong duoc dung nen nhu the nao.");
  }

  {
    const s = pres.addSlide({ masterName: "BASE", sectionTitle: "Chương 4 – Thiết kế" });
    tx(s, "Kiến trúc đơn khối phân lớp – 7 project", { placeholder: "title" });
    fitImage(s, "kt_tongquan", 1.34, { x: 0.7, y: 1.45, w: 7.0, h: 4.9 });
    tx(s, "Nguyên tắc phụ thuộc một chiều", { x: 8.0, y: 1.6, w: 4.6, h: 0.4, fontSize: 17, bold: true, color: C.accent1 });
    list(s, [
      "Tầng trên tham chiếu tầng dưới, không có chiều ngược lại",
      "BookStore.Services là nơi duy nhất cài đặt BR01–BR10",
      "Trang Web gọi thẳng Services qua DI cho nhanh",
      "Ứng dụng Desktop đi qua Web API nên máy quản lý không giữ chuỗi kết nối",
      "Đổi quy tắc nghiệp vụ chỉ sửa một nơi, cả hai ứng dụng cùng đổi",
    ], { x: 8.0, y: 2.1, w: 4.6, h: 4.2, fontSize: 13, paraSpaceAfter: 12 });
    s.addNotes("Day la ly do chinh vi sao tach tang nghiep vu thanh mot project rieng.");
  }

  {
    const s = pres.addSlide({ masterName: "TINT", sectionTitle: "Chương 4 – Thiết kế" });
    tx(s, "Mô hình C4 – bốn mức chi tiết", { placeholder: "title" });
    const muc = [
      ["Mức 1", "Bối cảnh", "Hệ thống là một hộp đen, xung quanh là khách hàng, quản lý, VNPay, email, đơn vị vận chuyển"],
      ["Mức 2", "Container", "5 khối chạy độc lập: trang Web, ứng dụng quản trị, Web API, thư viện nghiệp vụ, cơ sở dữ liệu"],
      ["Mức 3", "Thành phần", "5 lớp dịch vụ trong tầng nghiệp vụ, kèm lớp hằng số quy tắc, kiểu KetQua và các DTO"],
      ["Mức 4", "Mã nguồn", "Lược đồ lớp của nghiệp vụ đơn hàng: DonHangService, DonHang, CTDonHang, enum trạng thái"],
    ];
    muc.forEach(([so, ten, mo], i) => {
      const y = 1.6 + i * 1.22;
      card(s, { x: M, y, w: 12, h: 1.05, fill: C.background1, name: "c4-" + i });
      tx(s, so, { x: M + 0.35, y: y + 0.18, w: 1.1, h: 0.7, fontSize: 18, bold: true, color: C.accent3, valign: "middle" });
      tx(s, ten, { x: M + 1.5, y: y + 0.18, w: 2.4, h: 0.7, fontSize: 16, bold: true, color: C.accent1, valign: "middle" });
      tx(s, mo, { x: M + 4.0, y: y + 0.18, w: 8.3, h: 0.7, fontSize: 12.5, color: C.text1, valign: "middle" });
    });
    tx(s, "Mỗi mức dành cho một nhóm người đọc khác nhau, chi tiết tăng dần từ mức 1 đến mức 4.",
      { x: M, y: 6.5, w: 12, h: 0.4, fontSize: 12.5, italic: true, color: C.accent5 });
    s.addNotes("C4 giup trinh bay kien truc cho nhieu doi tuong nguoi doc ma khong bi roi.");
  }

  {
    const s = pres.addSlide({ masterName: "BASE", sectionTitle: "Chương 4 – Thiết kế" });
    tx(s, "C4 mức 2 – Sơ đồ container", { placeholder: "title" });
    fitImage(s, "c4_l2_container", 1.35, { x: 1.4, y: 1.45, w: 10.5, h: 4.7 });
    tx(s, "Hai lối vào tầng nghiệp vụ: trang Web gọi trực tiếp, ứng dụng Desktop đi qua Web API với token JWT.",
      { x: M, y: 6.3, w: 12, h: 0.4, fontSize: 13, italic: true, color: C.accent5, align: "center" });
    s.addNotes("Luu y ca hai loi vao deu ket thuc o cung mot tang nghiep vu.");
  }

  {
    const s = pres.addSlide({ masterName: "TINT", sectionTitle: "Chương 4 – Thiết kế" });
    tx(s, "Thiết kế giao tiếp", { placeholder: "title" });
    const kenh = [
      ["Khách hàng ↔ Trang Web", "HTTPS, cookie xác thực, chống giả mạo biểu mẫu"],
      ["Desktop ↔ Web API", "REST JSON, token JWT trong tiêu đề Authorization"],
      ["Giao diện ↔ Nghiệp vụ", "Gọi phương thức C# trong cùng tiến trình qua DI"],
      ["Nghiệp vụ ↔ CSDL", "EF Core sinh SQL, Windows Authentication"],
    ];
    kenh.forEach(([a, b], i) => {
      const y = 1.6 + i * 0.95;
      card(s, { x: M, y, w: 6.0, h: 0.8, fill: C.background1, name: "kenh" + i });
      tx(s, a, { x: M + 0.25, y: y + 0.05, w: 5.5, h: 0.32, fontSize: 13, bold: true, color: C.accent1 });
      tx(s, b, { x: M + 0.25, y: y + 0.38, w: 5.5, h: 0.36, fontSize: 11.5, color: C.accent5 });
    });
    tx(s, "Web API – 35 điểm cuối", { x: M + 6.5, y: 1.6, w: 5.5, h: 0.4, fontSize: 17, bold: true, color: C.accent1 });
    list(s, [
      "api/xacthuc – đăng nhập, thông tin tài khoản, đổi mật khẩu",
      "api/sach – tìm kiếm, chi tiết, thêm, sửa, xóa, danh mục",
      "api/donhang – danh sách, chi tiết, chuyển trạng thái, hủy đơn quá hạn",
      "api/danhmuc, api/kho, api/khachhang – danh mục, tồn kho, phiếu nhập, khách hàng",
      "api/baocao – dashboard, doanh thu, nhập – xuất – tồn, xuất Excel",
      "Phân quyền khai báo ngay tại controller bằng [Authorize(Roles = …)]",
    ], { x: M + 6.5, y: 2.1, w: 5.5, h: 4.0, fontSize: 12.5, paraSpaceAfter: 12 });
    s.addNotes("Duong dan API dat bang tieng Viet khong dau cho thong nhat voi ten bang.");
  }

  {
    const s = pres.addSlide({ masterName: "BASE", sectionTitle: "Chương 4 – Thiết kế" });
    tx(s, "Kịch bản đặt hàng – lược đồ tuần tự", { placeholder: "title" });
    fitImage(s, "seq_dathang", 1.73, { x: 0.8, y: 1.4, w: 11.7, h: 4.3 });
    card(s, { x: M, y: 5.95, w: 12, h: 0.85, fill: C.accent6, line: C.accent6 });
    tx(s, [
      { text: "Một transaction duy nhất: ", options: { bold: true } },
      { text: "kiểm tra tồn kho (BR01) → sao lưu đơn giá (BR02) → tính phí vận chuyển (BR10) → ghi đơn, chi tiết, thanh toán, lịch sử → trừ tồn kho → xóa giỏ. Thất bại ở bất kỳ bước nào đều hoàn tác toàn bộ." },
    ], { x: M + 0.35, y: 5.95, w: 11.3, h: 0.85, valign: "middle", fontSize: 12, color: C.text1 });
    s.addNotes("Day la cho the hien ro nhat vi sao can transaction.");
  }

  {
    const s = pres.addSlide({ masterName: "TINT", sectionTitle: "Chương 4 – Thiết kế" });
    tx(s, "Thiết kế thích ứng – log và xử lý lỗi", { placeholder: "title" });
    fitImage(s, "log_loi", 1.78, { x: 0.7, y: 1.5, w: 7.2, h: 3.6 });
    const nhom = [
      ["Lỗi nhập liệu", "Kiểm tra tại biểu mẫu và ở tầng nghiệp vụ, không ghi log"],
      ["Lỗi nghiệp vụ", "Trả về KetQua.Loi kèm mã quy tắc, ghi log mức Cảnh báo"],
      ["Sự cố kỹ thuật", "Ghi log mức Lỗi kèm mã truy vết, người dùng chỉ thấy thông báo chung"],
    ];
    nhom.forEach(([a, b], i) => {
      const y = 1.6 + i * 1.1;
      tx(s, a, { x: 8.2, y, w: 4.4, h: 0.32, fontSize: 14.5, bold: true, color: C.accent1 });
      tx(s, b, { x: 8.2, y: y + 0.34, w: 4.4, h: 0.65, fontSize: 12, color: C.text1, valign: "top" });
    });
    tx(s, "Chịu lỗi: EF Core thử lại 3 lần khi lỗi tạm thời · transaction tự hoàn tác · tự hủy đơn quá hạn thanh toán · tạm khóa tài khoản sau 5 lần đăng nhập sai",
      { x: M, y: 5.45, w: 12, h: 0.9, fontSize: 12.5, color: C.accent5, valign: "top" });
    s.addNotes("Phan biet ro loi nghiep vu va su co ky thuat la y chinh cua muc nay.");
  }

  /* ============================================ Chuong 5 */
  pres.addSection({ title: "Chương 5 – Kết quả" });
  {
    const s = pres.addSlide({ masterName: "DIVIDER", sectionTitle: "Chương 5 – Kết quả" });
    tx(s, "CHƯƠNG 5", { placeholder: "eyebrow" });
    tx(s, "Triển khai, kiểm thử và đánh giá kết quả", { placeholder: "title" });
    tx(s, "11.269 dòng mã · 7 project · 19 bảng · 63 trường hợp kiểm thử", { placeholder: "body" });
    s.addNotes("Chuong 5 cho thay san pham that chu khong con la thiet ke.");
  }

  {
    const s = pres.addSlide({ masterName: "BASE", sectionTitle: "Chương 5 – Kết quả" });
    tx(s, "Khối lượng đã triển khai", { placeholder: "title" });
    const so = [["7", "project"], ["120", "tệp mã nguồn"], ["11.269", "dòng mã"], ["19", "bảng CSDL"], ["35", "điểm cuối API"], ["63", "test case"]];
    so.forEach(([a, b], i) => {
      const x = M + (i % 3) * 4.05, y = 1.6 + Math.floor(i / 3) * 1.5;
      card(s, { x, y, w: 3.8, h: 1.25, fill: C.background2, name: "so" + i });
      tx(s, a, { x, y: y + 0.12, w: 3.8, h: 0.6, align: "center", fontSize: 28, bold: true, color: C.accent1 });
      tx(s, b, { x, y: y + 0.75, w: 3.8, h: 0.35, align: "center", fontSize: 13, color: C.accent5 });
    });
    tx(s, "Môi trường: .NET 10 · SQL Server 2025 · EF Core 10 · Windows App SDK 1.8 · Bootstrap 5",
      { x: M, y: 4.8, w: 12, h: 0.4, fontSize: 13.5, bold: true, color: C.accent4 });
    tx(s, "So với giai đoạn 1, chỉ thay đổi nền tảng từ .NET 8 sang .NET 10 cho khớp với phiên bản cài trên máy. Kiến trúc và cách tổ chức mã nguồn giữ nguyên.",
      { x: M, y: 5.25, w: 12, h: 0.8, fontSize: 12.5, color: C.text1, valign: "top" });
    s.addNotes("Nhan manh day la ma nguon nhom tu viet, khong ke tep sinh tu dong.");
  }

  {
    const s = pres.addSlide({ masterName: "TINT", sectionTitle: "Chương 5 – Kết quả" });
    tx(s, "Quy trình nghiệp vụ hoàn chỉnh: đặt hàng và xử lý đơn", { placeholder: "title" });
    const web = ["Khách đăng nhập trang Web", "Tìm sách, xem chi tiết, thêm vào giỏ (BR01)",
      "Cập nhật giỏ, phí vận chuyển tính lại theo BR10", "Nhập địa chỉ, chọn COD hoặc VNPay, đặt hàng"];
    const dk = ["Quản lý đăng nhập ứng dụng quản trị", "Lọc đơn Chờ xác nhận, xem chi tiết và lịch sử",
      "Xác nhận → Đang giao → Hoàn thành", "Doanh thu được ghi nhận (BR09)"];
    card(s, { x: M, y: 1.6, w: 5.85, h: 4.3, fill: C.background1 });
    tx(s, "Trang Web – khách hàng", { x: M + 0.35, y: 1.85, w: 5.15, h: 0.4, fontSize: 17, bold: true, color: C.accent1 });
    list(s, web, { x: M + 0.35, y: 2.4, w: 5.15, h: 3.2, fontSize: 13, paraSpaceAfter: 14 });
    card(s, { x: M + 6.1, y: 1.6, w: 5.85, h: 4.3, fill: C.background1 });
    tx(s, "Ứng dụng quản trị – quản lý", { x: M + 6.45, y: 1.85, w: 5.15, h: 0.4, fontSize: 17, bold: true, color: C.accent4 });
    list(s, dk, { x: M + 6.45, y: 2.4, w: 5.15, h: 3.2, fontSize: 13, paraSpaceAfter: 14 });
    tx(s, "Nhánh rẽ cũng đã cài đặt: hủy đơn hoàn lại đúng số lượng tồn kho (BR03); khách chỉ hủy được khi đơn chưa xác nhận (BR05).",
      { x: M, y: 6.1, w: 12, h: 0.5, fontSize: 12.5, italic: true, color: C.accent5 });
    s.addNotes("Day chinh la yeu cau it nhat mot quy trinh nghiep vu lien quan ca hai thanh phan.");
  }

  {
    const s = pres.addSlide({ masterName: "BASE", sectionTitle: "Chương 5 – Kết quả" });
    tx(s, "Giao diện trang Web", { placeholder: "title" });
    const anh = [
      { n: "kq_w01_trangchu", t: "Trang chủ", x: M },
      { n: "kq_w04_giohang", t: "Giỏ hàng – cập nhật không tải lại trang", x: M + 6.1 },
    ];
    anh.forEach((a) => {
      card(s, { x: a.x, y: 1.55, w: 5.85, h: 4.1, fill: C.background2 });
      fitImage(s, a.n, 1.41, { x: a.x + 0.25, y: 1.8, w: 5.35, h: 3.35 });
      tx(s, a.t, { x: a.x, y: 5.2, w: 5.85, h: 0.35, align: "center", fontSize: 13, bold: true, color: C.accent1 });
    });
    tx(s, "Blazor Web App ở chế độ Interactive Server, giao diện Bootstrap 5 tự co giãn theo chiều rộng màn hình.",
      { x: M, y: 5.9, w: 11.95, h: 0.5, fontSize: 13, italic: true, color: C.accent5 });
    s.addNotes("Anh chup that cua he thong dang chay, khong phai wireframe.");
  }

  {
    const s = pres.addSlide({ masterName: "TINT", sectionTitle: "Chương 5 – Kết quả" });
    tx(s, "Giao diện ứng dụng quản trị WinUI 3", { placeholder: "title" });
    const anh = [
      { n: "kq_d02_tongquan", t: "Tổng quan – số liệu lấy từ Web API", x: M },
      { n: "kq_d03_donhang", t: "Quản lý đơn hàng – nút chỉ bật khi bước chuyển hợp lệ", x: M + 6.1 },
    ];
    anh.forEach((a) => {
      card(s, { x: a.x, y: 1.55, w: 5.85, h: 4.1, fill: C.background1 });
      fitImage(s, a.n, 1.64, { x: a.x + 0.25, y: 1.8, w: 5.35, h: 3.35 });
      tx(s, a.t, { x: a.x, y: 5.2, w: 5.85, h: 0.35, align: "center", fontSize: 12.5, bold: true, color: C.accent1 });
    });
    tx(s, "MVVM với CommunityToolkit.Mvvm, DI qua Generic Host, triển khai dạng unpackaged self-contained nên máy đích không cần cài runtime.",
      { x: M, y: 5.9, w: 11.95, h: 0.5, fontSize: 12.5, italic: true, color: C.accent5 });
    s.addNotes("Ung dung chay duoc ngay tren may khac chi bang cach chep thu muc.");
  }

  {
    const s = pres.addSlide({ masterName: "BASE", sectionTitle: "Chương 5 – Kết quả" });
    tx(s, "Kiểm thử phần mềm", { placeholder: "title" });
    card(s, { x: M, y: 1.55, w: 5.85, h: 4.5, fill: C.background2 });
    tx(s, "Mức đơn vị – 63 trường hợp", { x: M + 0.35, y: 1.8, w: 5.15, h: 0.4, fontSize: 17, bold: true, color: C.accent1 });
    list(s, [
      "xUnit, chạy trên SQLite in-memory nên không cần SQL Server",
      "Mỗi trường hợp có cơ sở dữ liệu sạch riêng",
      "Mười quy tắc BR01–BR10 đều có kiểm thử tương ứng",
      "Kết quả: 63 đạt, 0 thất bại, khoảng 8 giây",
    ], { x: M + 0.35, y: 2.35, w: 5.15, h: 3.4, fontSize: 13, paraSpaceAfter: 14 });

    card(s, { x: M + 6.1, y: 1.55, w: 5.85, h: 4.5, fill: C.background2 });
    tx(s, "Mức hệ thống – 15 kịch bản", { x: M + 6.45, y: 1.8, w: 5.15, h: 0.4, fontSize: 17, bold: true, color: C.accent4 });
    list(s, [
      "Chạy thật trên SQL Server với cả ba ứng dụng",
      "Thực hiện từ giao diện rồi kiểm tra lại dữ liệu trong CSDL",
      "Bao gồm cả các trường hợp phải bị từ chối",
      "Kết quả: 15/15 đạt",
    ], { x: M + 6.45, y: 2.35, w: 5.15, h: 3.4, fontSize: 13, paraSpaceAfter: 14 });

    tx(s, "Ba lỗi đáng nhớ đều nằm ở chỗ ghép nối hạ tầng: thứ tự middleware, vòng đời DbContext trong Blazor Server, transaction xung đột với cơ chế thử lại của EF Core.",
      { x: M, y: 6.2, w: 12, h: 0.5, fontSize: 12.5, italic: true, color: C.accent5 });
    s.addNotes("Nhan manh bo kiem thu da giup nhom sua cau truc ma nguon ma van yen tam.");
  }

  {
    const s = pres.addSlide({ masterName: "TINT", sectionTitle: "Chương 5 – Kết quả" });
    tx(s, "Đối chiếu với yêu cầu của đề bài", { placeholder: "title" });
    const yc = [
      ["Ứng dụng quản trị Windows có đăng nhập, phân quyền", "Đạt"],
      ["Trang Web người dùng viết bằng C#, responsive", "Đạt"],
      ["Hai thành phần dùng chung một CSDL SQL Server", "Đạt"],
      ["Cơ sở dữ liệu đạt 3NF, khóa chính và khóa ngoại hợp lý", "Đạt"],
      ["Hệ thống chạy và demo được", "Đạt"],
      ["Màn hình CRUD cho danh mục nghiệp vụ", "Đạt với sách"],
      ["Điểm sáng tạo: WinUI 3, Blazor, EF Core, Web API, DI, Unit Test, dashboard", "Đạt"],
      ["Xuất báo cáo ra Excel", "Chưa làm"],
    ];
    yc.forEach(([a, b], i) => {
      const y = 1.55 + i * 0.63;
      const dat = b === "Chưa làm";
      card(s, { x: M, y, w: 12, h: 0.53, fill: C.background1, name: "yc" + i });
      tx(s, a, { x: M + 0.3, y, w: 9.4, h: 0.53, valign: "middle", fontSize: 12.5, color: C.text1 });
      tx(s, b, { x: M + 9.9, y, w: 1.9, h: 0.53, valign: "middle", align: "right", fontSize: 12.5,
        bold: true, color: dat ? "B04040" : C.accent1 });
    });
    s.addNotes("Nhom neu ro ca phan chua lam de thay thay duoc pham vi that.");
  }

  /* ============================================ Chuong 6 */
  pres.addSection({ title: "Chương 6 – Kết luận" });
  {
    const s = pres.addSlide({ masterName: "DIVIDER", sectionTitle: "Chương 6 – Kết luận" });
    tx(s, "CHƯƠNG 6", { placeholder: "eyebrow" });
    tx(s, "Kết luận và hướng phát triển", { placeholder: "title" });
    s.addNotes("Phan cuoi cua bao cao.");
  }

  {
    const s = pres.addSlide({ masterName: "BASE", sectionTitle: "Chương 6 – Kết luận" });
    tx(s, "Kết luận và những điều rút ra", { placeholder: "title" });
    card(s, { x: M, y: 1.55, w: 5.85, h: 4.6, fill: C.background2 });
    tx(s, "Đã đạt được", { x: M + 0.35, y: 1.8, w: 5.15, h: 0.4, fontSize: 18, bold: true, color: C.accent1 });
    list(s, [
      "Đi trọn vòng từ khảo sát tới cài đặt và kiểm thử",
      "Đủ hai thành phần dùng chung một cơ sở dữ liệu",
      "Quy trình đặt hàng – xử lý đơn hoàn chỉnh hai đầu",
      "Vận dụng được các công nghệ thuộc nhóm điểm sáng tạo",
    ], { x: M + 0.35, y: 2.35, w: 5.15, h: 3.5, fontSize: 13, paraSpaceAfter: 14 });

    card(s, { x: M + 6.1, y: 1.55, w: 5.85, h: 4.6, fill: C.background2 });
    tx(s, "Bài học", { x: M + 6.45, y: 1.8, w: 5.15, h: 0.4, fontSize: 18, bold: true, color: C.accent4 });
    list(s, [
      "Giá trị của kiến trúc chỉ thấy rõ khi bắt đầu viết mã",
      "Lỗi khó nhất nằm ở chỗ ghép nối, không ở thuật toán",
      "Kiểm thử đơn vị tiết kiệm thời gian ngay trong cùng một kỳ",
      "Quy tắc nghiệp vụ nên viết thành hằng số hoặc dữ liệu",
    ], { x: M + 6.45, y: 2.35, w: 5.15, h: 3.5, fontSize: 13, paraSpaceAfter: 14 });
    s.addNotes("Phan bai hoc la nhung gi nhom thuc su rut ra trong qua trinh lam.");
  }

  {
    const s = pres.addSlide({ masterName: "TINT", sectionTitle: "Chương 6 – Kết luận" });
    tx(s, "Hạn chế và hướng phát triển", { placeholder: "title" });
    tx(s, "Hạn chế", { x: M, y: 1.55, w: 5.85, h: 0.4, fontSize: 18, bold: true, color: "B04040" });
    list(s, [
      "VNPay mới chạy với cổng giả lập, chưa có mã merchant sandbox, chưa xử lý IPN",
      "Email mới ghi ra tập tin, chưa gửi qua SMTP thật",
      "Báo cáo nhập – xuất – tồn lệch nếu sửa tồn kho thủ công",
      "Chưa kiểm thử tự động cho giao diện",
      "Chưa đóng gói Docker để chạy nhanh trên máy mới",
    ], { x: M, y: 2.1, w: 5.85, h: 3.6, fontSize: 13, paraSpaceAfter: 12 });

    tx(s, "Hướng phát triển", { x: M + 6.1, y: 1.55, w: 5.85, h: 0.4, fontSize: 18, bold: true, color: C.accent1 });
    const huong = [
      ["1", "VNPay sandbox thật, IPN, hoàn tiền; SMTP thật qua hàng đợi"],
      ["2", "Đóng gói Docker, triển khai máy chủ có HTTPS"],
      ["3", "Kiểm thử giao diện, biểu đồ doanh thu, bộ nhớ đệm"],
      ["4", "Khuyến mãi, đánh giá sách, gợi ý sách, ứng dụng di động"],
    ];
    huong.forEach(([so, t], i) => {
      const y = 2.15 + i * 0.92;
      s.addShape(pres.ShapeType.ellipse, { x: M + 6.1, y: y + 0.05, w: 0.34, h: 0.34,
        fill: { color: C.accent1 }, line: { color: C.accent1, width: 0 }, objectName: "huong" + so });
      tx(s, so, { x: M + 6.1, y: y + 0.05, w: 0.34, h: 0.34, align: "center", valign: "middle",
        fontSize: 11, bold: true, color: "FFFFFF" });
      tx(s, t, { x: M + 6.6, y, w: 5.3, h: 0.7, fontSize: 12.5, color: C.text1, valign: "top" });
    });
    tx(s, "Hai hướng đầu bù phần còn thiếu so với thiết kế, hai hướng sau mở rộng ra ngoài phạm vi ban đầu.",
      { x: M, y: 6.1, w: 12, h: 0.4, fontSize: 12.5, italic: true, color: C.accent5 });
    s.addNotes("Uu tien hoan thien phan loi truoc khi mo rong pham vi.");
  }

  {
    const s = pres.addSlide({ masterName: "TITLE", sectionTitle: "Chương 6 – Kết luận" });
    tx(s, "NHÓM 12 · ĐỒ ÁN NGÔN NGỮ LẬP TRÌNH C#", { placeholder: "eyebrow" });
    tx(s, "Cảm ơn thầy và các bạn đã lắng nghe", { placeholder: "title" });
    tx(s, [
      { text: "Nhóm xin demo hệ thống và tiếp nhận câu hỏi.", options: { fontSize: 17, breakLine: true } },
      { text: " ", options: { breakLine: true, fontSize: 10 } },
      { text: "Phạm Hoàng Sang · Võ Trần Hải Đăng · Ngô Hồ Đăng Khoa" },
    ], { placeholder: "body" });
    s.addNotes("Ket thuc phan trinh bay, chuyen sang demo truc tiep he thong.");
  }
};
