// Nội dung Chương 4 (Thiết kế kiến trúc), Chương 5 (Triển khai, kiểm thử,
// đánh giá kết quả) và Chương 6 (Kết luận) của báo cáo giai đoạn 2.
//
// Tách thành file riêng để build.js không quá dài. Các hàm dựng đoạn văn,
// hình, bảng được build.js truyền vào, nhờ vậy số hiệu "Hình x.y" và
// "Bảng x.y" vẫn tiếp tục đúng thứ tự của cả báo cáo.

module.exports = (h) => {
  const { P, PL, H1, H2, H3, H4, bullet, bulletL, figure, figureOpt, table, CONTENT_W } = h;

  // Bảng hai cột "Thành phần – Mô tả" dùng lại nhiều lần trong chương 4
  const t2 = (c1, c2, rows, cap, w1 = 3200) =>
    table([c1, c2], rows, [w1, CONTENT_W - w1], cap);
  const t3 = (c1, c2, c3, rows, cap, w1 = 2400, w2 = 2600) =>
    table([c1, c2, c3], rows, [w1, w2, CONTENT_W - w1 - w2], cap);

  // ================================================================ CHƯƠNG 4
  const ch4 = [
    H1("THIẾT KẾ KIẾN TRÚC"),

    P("Chương 3 đã xác định hệ thống cần làm những gì. Chương này trả lời câu hỏi hệ thống được " +
      "dựng nên như thế nào: phần mềm được chia thành những khối nào, mỗi khối chịu trách nhiệm gì, " +
      "các khối trao đổi với nhau ra sao, dữ liệu được tổ chức thế nào và hệ thống ứng xử ra sao khi " +
      "có sự cố. Toàn bộ thiết kế trong chương này đã được cài đặt thật và là cơ sở để đọc mã nguồn " +
      "trình bày ở Chương 5."),

    // ---------------------------------------------------------- 4.1
    H2("4.1. Tổng quan về kiến trúc dự án"),

    H3("4.1.1. Yêu cầu chi phối kiến trúc"),
    P("Kiến trúc của hệ thống không được chọn tùy ý mà bị chi phối bởi các ràng buộc sau:"),
    bulletL("Hai thành phần, một cơ sở dữ liệu. ",
      "Đề bài bắt buộc hệ thống gồm một ứng dụng quản trị chạy trên Windows và một trang Web cho " +
      "người dùng, cả hai dùng chung một cơ sở dữ liệu Microsoft SQL Server. Kiến trúc vì vậy phải " +
      "có một nơi duy nhất định nghĩa mô hình dữ liệu."),
    bulletL("Không lặp lại nghiệp vụ. ",
      "Cùng một quy tắc nghiệp vụ, ví dụ trừ tồn kho khi tạo đơn, có thể bị kích hoạt từ trang Web " +
      "(khách đặt hàng) lẫn từ ứng dụng quản trị (quản lý hủy đơn). Nếu mỗi ứng dụng tự viết lấy " +
      "thì chỉ cần sửa quy tắc ở một nơi là hệ thống mâu thuẫn."),
    bulletL("Máy quản lý không giữ chuỗi kết nối. ",
      "Ứng dụng Desktop được cài trên nhiều máy trong nhà sách. Nếu mỗi máy nối thẳng vào SQL Server " +
      "thì chuỗi kết nối, kèm tài khoản truy cập cơ sở dữ liệu, nằm rải rác trên các máy đó."),
    bulletL("Vừa sức một nhóm ba sinh viên. ",
      "Kiến trúc phải đủ rõ ràng để cả ba thành viên hiểu và giải thích được, không chọn những " +
      "kiểu kiến trúc phức tạp như microservices."),

    H3("4.1.2. Kiểu kiến trúc được chọn"),
    P("Hệ thống được xây dựng theo kiểu kiến trúc đơn khối phân lớp (layered monolith): toàn bộ hệ " +
      "thống là một solution .NET 10 duy nhất, bên trong chia thành bảy project xếp theo ba tầng. " +
      "Project ở tầng trên được phép tham chiếu project ở tầng dưới, không có chiều ngược lại. " +
      "Nhờ ràng buộc một chiều này, tầng nghiệp vụ không biết gì về giao diện, nên cùng một tầng " +
      "nghiệp vụ phục vụ được cả trang Web lẫn ứng dụng quản trị."),
    figure("kt_tongquan", "Kiến trúc phân lớp của hệ thống và quan hệ phụ thuộc giữa bảy project"),

    P("Vai trò của từng project được trình bày trong Bảng 4.1."),
    t3("Project", "Loại", "Trách nhiệm", [
      ["BookStore.Domain", "Thư viện lớp", "19 lớp thực thể tương ứng 19 bảng và các kiểu liệt kê trạng thái. Không phụ thuộc bất kỳ project nào khác."],
      ["BookStore.Data", "Thư viện lớp", "BookStoreDbContext, cấu hình ánh xạ bằng Fluent API, Migrations và lớp sinh dữ liệu khởi tạo."],
      ["BookStore.Services", "Thư viện lớp", "Toàn bộ nghiệp vụ và quy tắc BR01–BR10, các DTO, các giao diện IDonHangService, ISachService, IGioHangService, ITaiKhoanService, IBaoCaoService."],
      ["BookStore.Api", "ASP.NET Core Web API", "Mở nghiệp vụ ra ngoài dưới dạng REST cho ứng dụng Desktop, xác thực bằng JWT và phân quyền theo Role."],
      ["BookStore.Web", "Blazor Web App", "Giao diện khách hàng. Gọi thẳng BookStore.Services qua Dependency Injection, không đi qua Web API."],
      ["BookStore.Admin", "WinUI 3 (Windows App SDK)", "Ứng dụng quản trị. Chỉ nói chuyện với BookStore.Api bằng HttpClient, không tham chiếu Services và không biết chuỗi kết nối."],
      ["BookStore.Tests", "xUnit", "Kiểm thử tầng nghiệp vụ trên cơ sở dữ liệu SQLite nằm trong bộ nhớ."],
    ], "Vai trò của bảy project trong solution", 2600, 2400),

    H3("4.1.3. Hai lối vào tầng nghiệp vụ"),
    P("Điểm cần giải thích rõ trong kiến trúc này là vì sao trang Web gọi thẳng tầng nghiệp vụ còn " +
      "ứng dụng Desktop lại phải đi vòng qua Web API."),
    P("Trang Web chạy ngay trên máy chủ, cùng tiến trình với tầng nghiệp vụ. Nếu bắt nó gọi qua Web " +
      "API thì mỗi thao tác phải thêm một lượt tuần tự hóa JSON và một vòng truyền mạng nội bộ, " +
      "trong khi không được lợi gì về bảo mật. Vì vậy trang Web nhận thẳng các interface của tầng " +
      "nghiệp vụ qua Dependency Injection."),
    P("Ứng dụng Desktop thì ngược lại: nó chạy trên máy của quản lý, nằm ngoài máy chủ. Cho nó nối " +
      "thẳng vào cơ sở dữ liệu đồng nghĩa với việc phát tán chuỗi kết nối và mất chỗ kiểm tra quyền " +
      "tập trung. Vì vậy mọi yêu cầu của nó đều đi qua Web API, nơi token JWT được kiểm tra trước " +
      "khi tầng nghiệp vụ được gọi."),
    P("Dù đi theo lối nào, điểm đến cuối cùng vẫn là cùng một lớp dịch vụ trong BookStore.Services. " +
      "Khi quy tắc nghiệp vụ thay đổi, chỉ cần sửa ở đó là cả hai ứng dụng cùng thay đổi theo."),

    // ---------------------------------------------------------- 4.2
    H2("4.2. Thiết kế thành phần theo mô hình C4"),
    P("Mô hình C4 mô tả kiến trúc phần mềm bằng bốn mức chi tiết tăng dần, mỗi mức dành cho một " +
      "nhóm người đọc khác nhau: mức 1 cho người dùng và giảng viên muốn nắm bối cảnh, mức 2 cho " +
      "người cần biết hệ thống gồm những khối chạy độc lập nào, mức 3 cho người sắp sửa mã nguồn " +
      "và mức 4 cho người cần hiểu một lớp cụ thể. Bốn mục dưới đây trình bày lần lượt bốn mức đó."),

    H3("4.2.1. Mức 1 – Bối cảnh hệ thống"),
    P("Ở mức này, toàn bộ hệ thống được coi là một hộp đen duy nhất. Điều quan trọng là ai dùng nó " +
      "và nó phải trao đổi với những hệ thống bên ngoài nào."),
    figure("c4_l1_context", "C4 mức 1 – Sơ đồ bối cảnh hệ thống"),
    t3("Thành phần ngoài", "Vai trò", "Hình thức trao đổi", [
      ["Khách hàng", "Người mua sách trên trang Web", "Trình duyệt, giao thức HTTPS"],
      ["Quản lý", "Xử lý đơn, quản lý sách và kho, xem báo cáo", "Ứng dụng Desktop gọi Web API qua HTTPS"],
      ["Cổng thanh toán VNPay", "Thu tiền với đơn chọn thanh toán trực tuyến", "Chuyển hướng trình duyệt và nhận kết quả trả về"],
      ["Dịch vụ Email", "Gửi thư xác nhận đơn hàng, thư đặt lại mật khẩu", "Giao thức SMTP"],
      ["Đơn vị vận chuyển", "Nhận hàng và giao đến người mua", "Bàn giao ngoài hệ thống, quản lý cập nhật trạng thái thủ công"],
    ], "Các tác nhân và hệ thống ngoài ở mức bối cảnh", 2800, 3000),

    H3("4.2.2. Mức 2 – Các container"),
    P("Mở hộp đen ở mức 1 ra, hệ thống gồm năm khối có thể chạy hoặc triển khai độc lập, gọi là " +
      "container theo thuật ngữ của C4. Lưu ý container ở đây không phải là Docker container mà là " +
      "một tiến trình, một kho dữ liệu hoặc một thư viện có ranh giới triển khai riêng."),
    figure("c4_l2_container", "C4 mức 2 – Sơ đồ container"),
    t3("Container", "Công nghệ", "Trách nhiệm", [
      ["Trang Web bán sách", "Blazor Web App (Interactive Server), Bootstrap 5", "Toàn bộ giao diện khách hàng: trang chủ, tìm kiếm, chi tiết sách, giỏ hàng, đặt hàng, đơn hàng của tôi."],
      ["Ứng dụng quản trị", "WinUI 3, MVVM, CommunityToolkit.Mvvm", "Giao diện quản lý: tổng quan, xử lý đơn hàng, quản lý sách."],
      ["Web API", "ASP.NET Core Web API, JWT, Swagger", "Cửa ngõ duy nhất của ứng dụng Desktop vào hệ thống. Xác thực, phân quyền theo Role, chuyển tiếp xuống tầng nghiệp vụ."],
      ["Thư viện nghiệp vụ", ".NET class library", "Nơi duy nhất cài đặt BR01–BR10. Được cả trang Web và Web API nạp vào cùng tiến trình qua DI."],
      ["Cơ sở dữ liệu", "SQL Server 2025", "19 bảng quan hệ, giữ toàn bộ dữ liệu lâu dài của hệ thống."],
    ], "Các container của hệ thống", 2600, 2600),

    H3("4.2.3. Mức 3 – Các thành phần bên trong tầng nghiệp vụ"),
    P("Container đáng quan tâm nhất là thư viện nghiệp vụ, vì đó là nơi tập trung toàn bộ quy tắc. " +
      "Thư viện này được chia thành năm lớp dịch vụ theo đúng các phân hệ đã phân tích ở mục 3.2.2, " +
      "kèm ba nhóm lớp dùng chung."),
    figure("c4_l3_component", "C4 mức 3 – Các thành phần bên trong BookStore.Services"),
    t3("Thành phần", "Giao diện", "Nhiệm vụ chính", [
      ["TaiKhoanService", "ITaiKhoanService", "Đăng nhập cho cả hai loại ứng dụng, đăng ký khách hàng, đổi mật khẩu, đếm số lần đăng nhập sai và tạm khóa tài khoản."],
      ["SachService", "ISachService", "Tìm kiếm có lọc và phân trang, thêm, sửa, xóa sách; áp dụng BR07 khi sách đã phát sinh giao dịch."],
      ["GioHangService", "IGioHangService", "Thêm, sửa số lượng, xóa dòng giỏ hàng; kiểm tra tồn kho ngay khi khách thêm sách."],
      ["DonHangService", "IDonHangService", "Đặt hàng trong một giao dịch, chuyển trạng thái theo sơ đồ, hủy đơn và hoàn tồn kho, tự hủy đơn quá hạn thanh toán."],
      ["BaoCaoService", "IBaoCaoService", "Số liệu tổng quan cho dashboard, doanh thu theo khoảng ngày, danh sách sách bán chạy."],
      ["QuyTacNghiepVu", "lớp tĩnh", "Giữ các hằng số nghiệp vụ: phí vận chuyển, ngưỡng miễn phí, số phút hết hạn thanh toán, số lần đăng nhập sai tối đa."],
      ["KetQua và KetQua<T>", "lớp dùng chung", "Kiểu trả về thống nhất gồm cờ thành công, thông báo tiếng Việt và mã quy tắc bị vi phạm."],
      ["Các lớp DTO", "lớp dùng chung", "Dữ liệu trao đổi giữa tầng nghiệp vụ và tầng giao diện, tránh để thực thể của EF Core lọt ra ngoài."],
    ], "Các thành phần trong tầng nghiệp vụ", 2600, 2200),

    P("Việc mọi dịch vụ đều trả về KetQua thay vì ném ngoại lệ là một quyết định thiết kế có chủ ý. " +
      "Lỗi nghiệp vụ, ví dụ khách đặt nhiều hơn số lượng tồn, là tình huống bình thường chứ không " +
      "phải sự cố; tầng giao diện chỉ cần đọc thuộc tính ThongBao và hiển thị, không phải bắt " +
      "ngoại lệ ở từng chỗ gọi."),

    H3("4.2.4. Mức 4 – Thiết kế mã nguồn của nghiệp vụ đơn hàng"),
    P("Mức cuối cùng đi vào bên trong một thành phần. Chọn DonHangService vì đây là thành phần phức " +
      "tạp nhất, nơi cài đặt sáu trong mười quy tắc nghiệp vụ."),
    figure("c4_l4_code", "C4 mức 4 – Lược đồ lớp của nghiệp vụ đơn hàng"),
    P("Ba điểm đáng chú ý trong thiết kế này:"),
    bulletL("Sơ đồ trạng thái được mã hóa thành dữ liệu. ",
      "Thay vì rải các câu lệnh if kiểm tra trạng thái khắp nơi, lớp DonHangService khai báo một từ " +
      "điển BuocChuyenHopLe ánh xạ mỗi trạng thái sang danh sách trạng thái kế tiếp được phép. " +
      "Hàm ChuyenTrangThaiAsync chỉ cần tra từ điển này. Khi quy trình nghiệp vụ thay đổi, chỉ sửa " +
      "một khai báo thay vì dò lại toàn bộ hàm."),
    bulletL("Giá bán được sao lưu vào chi tiết đơn. ",
      "Lớp CTDonHang có hai trường TenSach và DonGia tuy đã có khóa ngoại tới bảng Sach. Đây là dữ " +
      "liệu lịch sử tại thời điểm đặt hàng theo BR02, không phải dư thừa vi phạm chuẩn 3NF."),
    bulletL("Hằng số nghiệp vụ tách khỏi thuật toán. ",
      "Phí vận chuyển và ngưỡng miễn phí nằm trong lớp tĩnh QuyTacNghiepVu, nhờ đó lớp kiểm thử " +
      "cũng đọc được cùng hằng số và không phải chép lại con số vào trong kịch bản kiểm thử."),

    // ---------------------------------------------------------- 4.3
    H2("4.3. Thiết kế giao tiếp"),

    H3("4.3.1. Các kênh giao tiếp trong hệ thống"),
    P("Hệ thống có bốn kênh trao đổi khác nhau về giao thức và cách xác thực, được tổng hợp trong " +
      "Bảng 4.5."),
    table(["Kênh", "Hai đầu", "Giao thức, định dạng", "Cách xác thực"], [
      ["Khách hàng – Trang Web", "Trình duyệt ↔ Blazor Server", "HTTPS; HTML cho trang tĩnh, WebSocket cho phần tương tác", "Cookie xác thực, chống giả mạo biểu mẫu bằng antiforgery token"],
      ["Ứng dụng quản trị – Web API", "WinUI 3 ↔ ASP.NET Core", "HTTPS, JSON (REST)", "Token JWT đặt trong tiêu đề Authorization"],
      ["Tầng giao diện – Tầng nghiệp vụ", "Trong cùng tiến trình", "Gọi phương thức C# qua Dependency Injection", "Không cần; quyền đã được kiểm tra ở lớp ngoài"],
      ["Tầng nghiệp vụ – Cơ sở dữ liệu", "EF Core ↔ SQL Server", "TDS, lệnh SQL do EF Core sinh ra", "Windows Authentication theo chuỗi kết nối"],
    ], [2100, 2100, 2500, CONTENT_W - 6700], "Các kênh giao tiếp của hệ thống"),

    H3("4.3.2. Hợp đồng của Web API"),
    P("Web API được thiết kế theo phong cách REST, đường dẫn đặt bằng tiếng Việt không dấu cho " +
      "thống nhất với tên bảng và tên lớp. Mọi phản hồi lỗi đều có dạng JSON chứa trường thongBao " +
      "bằng tiếng Việt để ứng dụng Desktop hiển thị thẳng cho quản lý."),
    table(["Phương thức", "Đường dẫn", "Chức năng", "Quyền"], [
      ["POST", "/api/xacthuc/dangnhap", "Đăng nhập, trả về token JWT và thông tin người dùng", "Ẩn danh"],
      ["GET", "/api/xacthuc/toi", "Lấy thông tin người đang đăng nhập, dùng để kiểm tra token", "Đã đăng nhập"],
      ["POST", "/api/xacthuc/doimatkhau", "Đổi mật khẩu", "Đã đăng nhập"],
      ["GET", "/api/sach", "Tìm kiếm, lọc, phân trang danh sách sách", "Quản lý"],
      ["GET", "/api/sach/{maSach}", "Chi tiết một quyển sách", "Quản lý"],
      ["POST", "/api/sach", "Thêm sách mới", "Quản lý"],
      ["PUT", "/api/sach/{maSach}", "Cập nhật sách", "Quản lý"],
      ["DELETE", "/api/sach/{maSach}", "Xóa sách, hoặc ngừng kinh doanh nếu đã có giao dịch", "Quản lý"],
      ["GET", "/api/sach/danhmuc", "Danh sách loại sách kèm số đầu sách", "Quản lý"],
      ["GET", "/api/donhang", "Danh sách đơn hàng có lọc theo trạng thái, ngày, từ khóa", "Quản lý"],
      ["GET", "/api/donhang/{maDonHang}", "Chi tiết đơn kèm các dòng hàng và lịch sử xử lý", "Quản lý"],
      ["PUT", "/api/donhang/{maDonHang}/trangthai", "Chuyển trạng thái đơn hàng", "Quản lý"],
      ["POST", "/api/donhang/huydonquahan", "Hủy các đơn VNPay quá hạn thanh toán (dự phòng, trang Web đã có tác vụ nền tự chạy mỗi phút)", "Quản lý"],
      ["GET", "/api/danhmuc/{loai}", "Danh sách loại sách, tác giả, nhà xuất bản hoặc nhà cung cấp, có tìm theo tên", "Quản lý"],
      ["POST", "/api/danhmuc/{loai}", "Thêm một mục danh mục", "Quản lý"],
      ["PUT", "/api/danhmuc/{loai}/{ma}", "Cập nhật một mục danh mục", "Quản lý"],
      ["DELETE", "/api/danhmuc/{loai}/{ma}", "Xóa mục danh mục chưa được sách hoặc phiếu nhập sử dụng", "Quản lý"],
      ["GET", "/api/kho/tonkho", "Tồn kho từng sách, lọc sách sắp hết hàng", "Quản lý"],
      ["PUT", "/api/kho/tonkho/{maSach}/nguong", "Đặt ngưỡng cảnh báo hết hàng của một sách", "Quản lý"],
      ["GET", "/api/kho/phieunhap", "Danh sách phiếu nhập, lọc theo trạng thái", "Quản lý"],
      ["GET", "/api/kho/phieunhap/{ma}", "Chi tiết phiếu nhập kèm các dòng sách", "Quản lý"],
      ["POST", "/api/kho/phieunhap", "Lập phiếu nhập ở trạng thái Chờ nhập", "Quản lý"],
      ["PUT", "/api/kho/phieunhap/{ma}", "Sửa phiếu nhập chưa hoàn thành", "Quản lý"],
      ["POST", "/api/kho/phieunhap/{ma}/hoanthanh", "Hoàn thành phiếu nhập và cộng tồn kho (BR06)", "Quản lý"],
      ["DELETE", "/api/kho/phieunhap/{ma}", "Xóa phiếu nhập chưa hoàn thành", "Quản lý"],
      ["GET", "/api/khachhang", "Tra cứu khách hàng theo từ khóa và trạng thái tài khoản", "Quản lý"],
      ["GET", "/api/khachhang/{ma}", "Thông tin khách hàng kèm số đơn và tổng chi tiêu", "Quản lý"],
      ["PUT", "/api/khachhang/{ma}/trangthai", "Khóa hoặc mở khóa tài khoản khách hàng", "Quản lý"],
      ["GET", "/api/baocao/thongke", "Số liệu tổng quan cho màn hình Tổng quan", "Quản lý"],
      ["GET", "/api/baocao/doanhthu", "Doanh thu và sách bán chạy theo khoảng ngày", "Quản lý"],
      ["GET", "/api/baocao/doanhthu/chitiet", "Doanh thu từng ngày, số đơn, số sách bán, giá trị trung bình mỗi đơn", "Quản lý"],
      ["GET", "/api/baocao/nhapxuatton", "Báo cáo nhập – xuất – tồn của từng sách trong kỳ", "Quản lý"],
      ["GET", "/api/baocao/dontheotrangthai", "Số đơn và tổng tiền theo từng trạng thái đơn", "Quản lý"],
      ["GET", "/api/baocao/xuatexcel/doanhthu", "Tải báo cáo doanh thu dạng tập tin Excel", "Quản lý"],
      ["GET", "/api/baocao/xuatexcel/nhapxuatton", "Tải báo cáo nhập – xuất – tồn dạng tập tin Excel", "Quản lý"],
    ], [1400, 3000, CONTENT_W - 6500, 2100], "Danh sách điểm cuối của Web API"),

    H3("4.3.3. Xác thực và phân quyền"),
    P("Hai loại ứng dụng dùng hai cơ chế xác thực khác nhau vì bản chất của chúng khác nhau. Trang " +
      "Web chạy trong trình duyệt nên dùng cookie: trình duyệt tự gửi kèm cookie ở mọi yêu cầu và " +
      "cookie được đánh dấu HttpOnly nên mã JavaScript trên trang không đọc được. Ứng dụng Desktop " +
      "không có trình duyệt nên dùng token JWT: sau khi đăng nhập, token được giữ trong bộ nhớ của " +
      "tiến trình và được lớp ApiClient tự gắn vào tiêu đề Authorization của mọi lời gọi."),
    P("Dù khác cơ chế, cả hai đều tra cùng bảng TaiKhoan và bảng VaiTro. Nhờ vậy việc khóa một tài " +
      "khoản có hiệu lực tức thì ở cả hai phía. Ba vai trò trong hệ thống được phân tách như sau:"),
    bulletL("KHÁCH HÀNG: ", "chỉ đăng nhập được trang Web. Nếu nhập tài khoản này vào ứng dụng quản trị, " +
      "tầng nghiệp vụ trả về thông báo từ chối ngay cả khi mật khẩu đúng."),
    bulletL("QUẢN LÝ: ", "đăng nhập ứng dụng quản trị, xử lý đơn hàng, quản lý sách, kho và xem báo cáo doanh thu."),
    P("Trên Web API, phân quyền được khai báo bằng thuộc tính [Authorize(Roles = …)] đặt tại lớp " +
      "controller hoặc tại từng hành động, nên đọc mã nguồn là thấy ngay ai được gọi điểm cuối nào."),

    H3("4.3.4. Hai kịch bản giao tiếp tiêu biểu"),
    P("Hình 4.6 mô tả đường đi của một yêu cầu đặt hàng từ trình duyệt của khách tới cơ sở dữ liệu. " +
      "Đây là kịch bản đi theo lối gọi trực tiếp, không qua Web API."),
    figure("seq_dathang", "Lược đồ tuần tự – Khách hàng đặt hàng trên trang Web"),

    P("Hình 4.7 mô tả kịch bản còn lại: quản lý dùng ứng dụng Desktop để xác nhận chính đơn hàng " +
      "vừa được tạo ở Hình 4.6. Lần này yêu cầu đi qua Web API và phải vượt qua hai lớp kiểm tra: " +
      "kiểm tra quyền ở controller và kiểm tra tính hợp lệ của bước chuyển trạng thái ở tầng nghiệp vụ."),
    figure("seq_xulydon", "Lược đồ tuần tự – Quản lý xác nhận đơn hàng trên ứng dụng quản trị"),

    // ---------------------------------------------------------- 4.4
    H2("4.4. Thiết kế dữ liệu"),

    H3("4.4.1. Từ lược đồ vật lý sang mã nguồn"),
    P("Lược đồ cơ sở dữ liệu mức vật lý đã được thiết kế ở mục 3.3 với 19 bảng. Ở bước cài đặt, " +
      "nhóm chọn hướng Code First của EF Core: mỗi bảng được mô tả bằng một lớp C# trong project " +
      "BookStore.Domain, còn cách ánh xạ lớp sang bảng được khai báo tập trung bằng Fluent API trong " +
      "phương thức OnModelCreating của BookStoreDbContext. Lệnh dotnet ef migrations sinh ra kịch " +
      "bản tạo bảng từ các khai báo đó."),
    P("Hướng Code First được chọn thay vì Database First vì ba lý do: lược đồ nằm trong mã nguồn nên " +
      "được quản lý phiên bản cùng với Git; mỗi lần đổi thiết kế đều để lại một migration có thể " +
      "đọc và xem lại; và cả ba thành viên chỉ cần chạy một lệnh là có cơ sở dữ liệu giống hệt nhau " +
      "trên máy mình."),

    H3("4.4.2. Quy ước ánh xạ"),
    P("Để lược đồ sinh ra khớp với thiết kế ở Chương 3, nhóm áp dụng thống nhất các quy ước sau."),
    t2("Quy ước", "Cách áp dụng", [
      ["Tên bảng và tên cột", "Giữ nguyên tiếng Việt không dấu kiểu PascalCase như trong thiết kế, khai báo bằng ToTable và không để EF Core tự đặt tên số nhiều."],
      ["Khóa chính", "Mỗi bảng một khóa chính tự tăng, trừ bốn bảng quan hệ nhiều – nhiều và bảng chi tiết dùng khóa chính ghép: Sach_TacGia, CTPhieuNhap, CTGioHang, CTDonHang."],
      ["Kiểu tiền tệ", "Mọi cột tiền dùng decimal(18,2) thay vì float, tránh sai số khi cộng dồn thành tiền."],
      ["Kiểu trạng thái", "Các cột trạng thái dùng TINYINT và được ánh xạ sang kiểu liệt kê trong C#, nhờ đó mã nguồn viết TrangThaiDonHang.DangGiao thay vì con số 3."],
      ["Ràng buộc kiểm tra", "Năm ràng buộc CHECK ở mức cơ sở dữ liệu: giá bán và số lượng tồn không âm, số lượng trong chi tiết đơn, chi tiết phiếu nhập và dòng giỏ hàng phải lớn hơn 0."],
      ["Ràng buộc duy nhất", "Tên đăng nhập và email trong bảng TaiKhoan; mỗi tài khoản chỉ gắn với một khách hàng hoặc một quản lý; mỗi khách hàng chỉ có một giỏ hàng."],
      ["Chỉ mục", "Thêm chỉ mục trên Sach.TenSach để phục vụ tìm kiếm, trên DonHang.NgayDat và DonHang.TrangThai để phục vụ lọc đơn và thống kê."],
      ["Hành vi xóa", "Chi tiết đơn, lịch sử và thanh toán xóa theo đơn (Cascade). Ngược lại, không cho xóa sách, loại sách hay khách hàng khi còn bản ghi tham chiếu (Restrict), nhằm giữ toàn vẹn dữ liệu lịch sử."],
    ], "Quy ước ánh xạ giữa lớp C# và bảng cơ sở dữ liệu", 2800),

    H3("4.4.3. Migration và dữ liệu khởi tạo"),
    P("Toàn bộ lược đồ được tạo bằng một migration duy nhất tên KhoiTaoCSDL. Khi ứng dụng khởi " +
      "động ở môi trường phát triển, lớp SeedData gọi Database.MigrateAsync để áp dụng những " +
      "migration còn thiếu, sau đó kiểm tra bảng VaiTro: nếu bảng còn rỗng thì sinh dữ liệu mẫu, " +
      "nếu đã có dữ liệu thì bỏ qua. Cách này cho phép chạy lại ứng dụng nhiều lần mà không nhân " +
      "đôi dữ liệu."),
    P("Dữ liệu khởi tạo gồm hai vai trò, hai tài khoản mẫu (một quản lý, một khách " +
      "hàng), một hồ sơ quản lý, một hồ sơ khách hàng kèm địa chỉ giao hàng, sáu loại sách, bốn " +
      "nhà xuất bản, sáu tác giả, hai nhà cung cấp và mười hai quyển sách. Mật khẩu của các tài " +
      "khoản mẫu được băm bằng lớp PasswordHasher của ASP.NET Core Identity chứ không lưu dạng " +
      "chữ thường."),

    H3("4.4.4. Giao dịch và toàn vẹn dữ liệu"),
    P("Hai thao tác trong hệ thống đụng tới nhiều bảng cùng lúc và không được phép hoàn thành một " +
      "nửa, đó là tạo đơn hàng và chuyển trạng thái đơn hàng. Khi tạo đơn, hệ thống phải ghi bản " +
      "ghi đơn, ghi các dòng chi tiết, ghi bản ghi thanh toán, ghi lịch sử, trừ tồn kho và xóa giỏ " +
      "hàng. Nếu dừng giữa chừng, tồn kho sẽ bị trừ cho một đơn không tồn tại."),
    P("Vì vậy cả hai thao tác đều được bọc trong một transaction tường minh. Chuỗi kết nối của hệ " +
      "thống bật chế độ thử lại khi lỗi tạm thời (EnableRetryOnFailure), mà EF Core không cho phép " +
      "mở transaction thủ công bên trong chế độ này nếu không khai báo rõ. Do đó transaction được " +
      "đặt bên trong một execution strategy lấy từ Database.CreateExecutionStrategy, để nếu lần " +
      "thử lại xảy ra thì cả khối thao tác được chạy lại như một đơn vị, không phải chỉ một câu lệnh."),
    P("Ngoài transaction, tính toàn vẹn còn được bảo vệ ở hai mức nữa: các ràng buộc CHECK và khóa " +
      "ngoại ở mức cơ sở dữ liệu, và các phép kiểm tra nghiệp vụ ở tầng Services. Hai lớp này bổ " +
      "sung cho nhau: tầng nghiệp vụ cho thông báo dễ hiểu bằng tiếng Việt, còn cơ sở dữ liệu là " +
      "chốt chặn cuối cùng kể cả khi có ai đó sửa dữ liệu trực tiếp."),

    // ---------------------------------------------------------- 4.5
    H2("4.5. Thiết kế thích ứng – ghi nhật ký và xử lý lỗi"),

    H3("4.5.1. Phân loại tình huống bất thường"),
    P("Không phải lỗi nào cũng được xử lý giống nhau. Hệ thống phân biệt ba nhóm, mỗi nhóm có cách " +
      "ứng xử riêng."),
    t3("Nhóm", "Ví dụ", "Cách xử lý", [
      ["Lỗi nhập liệu", "Bỏ trống địa chỉ giao hàng, mật khẩu xác nhận không khớp, giá bán âm", "Kiểm tra ngay tại biểu mẫu và kiểm tra lại ở tầng nghiệp vụ; trả về KetQua.Loi kèm thông báo; không ghi log."],
      ["Lỗi nghiệp vụ", "Đặt nhiều hơn tồn kho (BR01), khách hủy đơn đã xác nhận (BR05), chuyển trạng thái sai sơ đồ", "Trả về KetQua.Loi kèm mã quy tắc; ghi log mức Cảnh báo để còn truy lại được."],
      ["Sự cố kỹ thuật", "Mất kết nối cơ sở dữ liệu, Web API không chạy, lỗi lập trình ngoài dự kiến", "Ghi log mức Lỗi kèm toàn bộ ngăn xếp và mã truy vết; hiển thị cho người dùng một thông báo chung, không lộ chi tiết kỹ thuật."],
    ], "Phân loại tình huống bất thường và cách xử lý", 2200, 3200),

    figure("log_loi", "Luồng xử lý lỗi và ghi nhật ký trong hệ thống"),

    H3("4.5.2. Ghi nhật ký"),
    P("Cả ba ứng dụng đều dùng chung hạ tầng ILogger của .NET, nhận qua Dependency Injection. Mức " +
      "ghi log được cấu hình trong appsettings.json, riêng không gian tên BookStore đặt ở mức " +
      "Information còn các thư viện của Microsoft đặt ở mức Warning để nhật ký không bị trôi."),
    P("Những sự kiện sau luôn được ghi lại: đăng nhập thành công và thất bại kèm tên tài khoản, " +
      "tạm khóa tài khoản, tạo đơn hàng kèm mã đơn và tổng tiền, mọi lần chuyển trạng thái đơn kèm " +
      "trạng thái cũ, trạng thái mới và người thực hiện, thêm – sửa – xóa sách, và mọi lời gọi Web " +
      "API trả về mã lỗi. Thông điệp log dùng cú pháp tham số có tên của ILogger, ví dụ ghi " +
      "\"Đơn {MaDonHang}: {Cu} -> {Moi}\" thay vì nối chuỗi, để sau này có thể lọc theo từng tham số."),
    P("Ngoài nhật ký kỹ thuật, hệ thống còn có một dạng nhật ký nghiệp vụ lưu trong bảng " +
      "LichSuDonHang. Khác với log, bảng này được hiển thị cho cả khách hàng xem trên trang Web và " +
      "cho quản lý xem trong ứng dụng quản trị, trả lời câu hỏi ai đã đổi trạng thái đơn, vào lúc " +
      "nào và vì lý do gì."),

    H3("4.5.3. Xử lý lỗi theo từng tầng"),
    bulletL("Tầng nghiệp vụ. ",
      "Mỗi phương thức có khả năng thất bại đều trả về KetQua hoặc KetQua<T>. Hai thao tác có " +
      "transaction còn bọc thêm khối try–catch: khi có ngoại lệ, transaction được hủy để dữ liệu " +
      "trở về nguyên trạng, ngoại lệ được ghi log rồi chuyển thành một thông báo lỗi dễ hiểu."),
    bulletL("Web API. ",
      "Lớp XuLyLoiMiddleware đặt ở đầu đường ống bắt mọi ngoại lệ chưa được xử lý, ghi log kèm mã " +
      "truy vết của yêu cầu rồi trả về phản hồi theo chuẩn ProblemDetails của RFC 7807. Middleware " +
      "này cũng đo thời gian xử lý và ghi cảnh báo với những yêu cầu chạy quá một giây."),
    bulletL("Trang Web. ",
      "Ở môi trường thật, UseExceptionHandler chuyển hướng người dùng sang trang lỗi riêng hiển thị " +
      "mã truy vết để báo cho bộ phận kỹ thuật. Các lỗi nghiệp vụ không đi qua đường này mà hiện " +
      "ngay tại chỗ dưới dạng dải thông báo màu đỏ."),
    bulletL("Ứng dụng quản trị. ",
      "Lớp ApiClient bắt riêng ba tình huống thường gặp nhất: không kết nối được máy chủ, máy chủ " +
      "phản hồi quá chậm và token hết hạn, mỗi tình huống có một câu hướng dẫn cụ thể thay vì một " +
      "thông báo chung chung. Sự kiện UnhandledException của lớp App ghi log mọi ngoại lệ còn lại " +
      "và giữ cho ứng dụng không bị đóng đột ngột."),

    H3("4.5.4. Khả năng chịu lỗi và phục hồi"),
    bulletL("Thử lại khi lỗi tạm thời. ",
      "EF Core được cấu hình thử lại tối đa ba lần với các lỗi tạm thời của SQL Server, giúp hệ " +
      "thống vượt qua những lần mất kết nối ngắn mà không làm người dùng thấy lỗi."),
    bulletL("Hoàn tác tự động. ",
      "Mọi thất bại trong lúc tạo đơn hoặc chuyển trạng thái đều kéo theo rollback, nên không bao " +
      "giờ có chuyện tồn kho bị trừ mà đơn hàng không được tạo."),
    bulletL("Tự dọn đơn treo. ",
      "Theo BR04, các đơn chọn thanh toán VNPay mà quá 30 phút chưa thanh toán sẽ bị hủy và hoàn " +
      "lại tồn kho. Việc này được gói trong phương thức HuyDonQuaHanAsync và mở ra một điểm cuối " +
      "Web API để quản lý kích hoạt."),
    bulletL("Chống dò mật khẩu. ",
      "Sau năm lần đăng nhập sai liên tiếp, tài khoản bị tạm khóa 15 phút. Thông báo trả về không " +
      "cho biết sai tên đăng nhập hay sai mật khẩu, nhằm không giúp người dò đoán được tài khoản nào có thật."),

    // ---------------------------------------------------------- 4.6
    H2("4.6. Thiết kế triển khai"),
    P("Hình 4.9 mô tả cách các thành phần được đặt lên máy thật. Trong phạm vi đồ án, máy chủ ứng " +
      "dụng và máy chủ cơ sở dữ liệu là cùng một máy; khi triển khai thật có thể tách hai máy mà " +
      "không phải sửa mã nguồn, chỉ cần đổi chuỗi kết nối."),
    figure("trienkhai", "Sơ đồ triển khai hệ thống"),
    P("Ứng dụng quản trị được đóng gói ở dạng unpackaged self-contained: toàn bộ .NET runtime và " +
      "Windows App SDK được chép kèm trong thư mục xuất bản. Nhờ vậy, máy của quản lý chỉ cần " +
      "chép thư mục về và chạy tập tin BookStore.Admin.exe, không phải cài thêm runtime, đây là " +
      "điểm thuận lợi khi demo trên máy của giảng viên."),
  ];

  // ================================================================ CHƯƠNG 5
  const ch5 = [
    H1("TRIỂN KHAI, KIỂM THỬ VÀ ĐÁNH GIÁ KẾT QUẢ"),

    P("Chương này trình bày phần mềm đã được cài đặt thực tế từ thiết kế ở Chương 4: mã nguồn được " +
      "tổ chức ra sao, chạy hệ thống thế nào, kiểm thử bằng cách nào và kết quả thu được tới đâu so " +
      "với yêu cầu của đề bài."),

    // ---------------------------------------------------------- 5.1
    H2("5.1. Triển khai phần mềm"),

    H3("5.1.1. Môi trường và công cụ"),
    t2("Thành phần", "Phiên bản sử dụng", [
      [".NET SDK", "10.0.401, các project đều nhắm net10.0 (riêng ứng dụng quản trị nhắm net10.0-windows10.0.19041.0)"],
      ["Hệ quản trị cơ sở dữ liệu", "Microsoft SQL Server 2025 (phiên bản 17.0), Developer Edition"],
      ["Entity Framework Core", "10.0.12, nhà cung cấp Microsoft.EntityFrameworkCore.SqlServer"],
      ["Windows App SDK", "1.8.250916003, dùng cho giao diện WinUI 3"],
      ["Thư viện MVVM", "CommunityToolkit.Mvvm 8.4.0"],
      ["Xác thực", "Microsoft.AspNetCore.Authentication.JwtBearer 10.0.12 cho Web API; cookie authentication và lớp PasswordHasher của ASP.NET Core Identity cho trang Web"],
      ["Tài liệu API", "Swashbuckle.AspNetCore 9.0.6 (giao diện Swagger UI)"],
      ["Kiểm thử", "xUnit, chạy trên SQLite in-memory qua Microsoft.EntityFrameworkCore.Sqlite 10.0.12"],
      ["Giao diện Web", "Bootstrap 5 kèm tệp định kiểu riêng của nhóm"],
      ["Hệ điều hành phát triển", "Windows 11"],
    ], "Môi trường và thư viện sử dụng", 3400),

    P("So với công nghệ đã khảo sát ở Chương 2, chỉ có một thay đổi: báo cáo giai đoạn 1 dự kiến " +
      "dùng .NET 8, nhưng máy cài đặt chỉ có .NET 10 nên toàn bộ solution được nâng lên nền tảng " +
      "này. Đây là thay đổi về phiên bản, không ảnh hưởng tới kiến trúc hay cách tổ chức mã nguồn."),

    H3("5.1.2. Khối lượng mã nguồn"),
    P("Bảng 5.2 thống kê số tệp mã nguồn và số dòng của từng project, không tính tệp sinh tự động " +
      "trong thư mục Migrations, obj và bin."),
    t3("Project", "Số tệp", "Số dòng", [
      ["BookStore.Domain", "21", "363"],
      ["BookStore.Data", "3", "460"],
      ["BookStore.Services", "20", "3.065"],
      ["BookStore.Api", "8", "645"],
      ["BookStore.Web", "27", "2.007"],
      ["BookStore.Admin", "33", "3.653"],
      ["BookStore.Tests", "8", "1.076"],
      ["Tổng cộng", "120", "11.269"],
    ], "Khối lượng mã nguồn theo từng project", 4000, 2000),

    H3("5.1.3. Cài đặt và chạy hệ thống"),
    P("Các bước để chạy hệ thống trên một máy mới:"),
    bullet("Bước 1. Cài .NET SDK 10 và SQL Server, bảo đảm dịch vụ SQL Server đang chạy."),
    bullet("Bước 2. Mở thư mục mã nguồn, chạy lệnh dotnet build để khôi phục gói và biên dịch cả bảy project."),
    bullet("Bước 3. Chỉnh chuỗi kết nối trong tệp appsettings.json của BookStore.Api và BookStore.Web nếu tên máy chủ cơ sở dữ liệu khác localhost."),
    bullet("Bước 4. Chạy BookStore.Api. Lần chạy đầu tiên, lớp SeedData sẽ tự tạo cơ sở dữ liệu QuanLyBanSachTrucTuyen gồm 19 bảng và nạp dữ liệu mẫu."),
    bullet("Bước 5. Chạy BookStore.Web rồi mở trình duyệt vào địa chỉ của trang Web để dùng phần dành cho khách hàng."),
    bullet("Bước 6. Chạy BookStore.Admin.exe để mở ứng dụng quản trị. Ứng dụng cần Web API đang chạy vì mọi dữ liệu đều lấy qua đó."),
    P("Hai tài khoản mẫu dùng để thử nghiệm, cả hai có mật khẩu 123456: quanly (vai trò QUẢN LÝ) " +
      "và khachhang (vai trò KHÁCH HÀNG). Tài khoản đầu chỉ dùng " +
      "được trên ứng dụng quản trị, tài khoản thứ hai chỉ dùng được trên trang Web."),
    P("Thanh toán VNPay và gửi email chạy được ngay mà không cần cấu hình gì thêm. Khi chưa có mã " +
      "TmnCode và khóa HashSecret của VNPay sandbox, khách hàng được chuyển sang cổng thanh toán giả " +
      "lập của hệ thống; khi chưa có máy chủ SMTP, email được ghi ra tập tin .eml trong thư mục " +
      "EmailGuiDi để xem lại nội dung. Muốn dùng dịch vụ thật, chỉ cần khai báo các khóa bằng lệnh " +
      "dotnet user-secrets (mục VnPay:TmnCode, VnPay:HashSecret, Email:MayChuSmtp, Email:TenDangNhap, " +
      "Email:MatKhau) mà không phải sửa mã nguồn; các khóa này nằm ngoài thư mục dự án nên không bị " +
      "đưa lên GitHub."),

    H3("5.1.4. Quy trình nghiệp vụ hoàn chỉnh đã cài đặt"),
    P("Đề bài yêu cầu cài đặt trọn vẹn ít nhất một quy trình nghiệp vụ có liên quan tới cả trang " +
      "người dùng lẫn phần mềm quản trị. Nhóm chọn quy trình đặt hàng và xử lý đơn hàng, vì đây là " +
      "quy trình trung tâm của hệ thống, đi qua cả hai ứng dụng và vận dụng phần lớn các quy tắc " +
      "nghiệp vụ đã phân tích."),
    P("Quy trình gồm chín bước, trong đó bốn bước đầu diễn ra trên trang Web và năm bước sau diễn " +
      "ra trên ứng dụng quản trị:"),
    t3("Bước", "Nơi thực hiện", "Nội dung và quy tắc áp dụng", [
      ["1", "Trang Web", "Khách hàng đăng nhập. Tài khoản quản lý bị từ chối ở bước này."],
      ["2", "Trang Web", "Khách tìm sách theo từ khóa hoặc lọc theo danh mục và khoảng giá, xem chi tiết rồi thêm vào giỏ. Hệ thống kiểm tra số lượng tồn ngay khi thêm (BR01)."],
      ["3", "Trang Web", "Khách mở giỏ hàng, tăng giảm số lượng hoặc xóa dòng. Phí vận chuyển được tính lại tức thì theo BR10 và hiển thị số tiền còn thiếu để được miễn phí."],
      ["4", "Trang Web", "Khách nhập thông tin người nhận, chọn hình thức thanh toán rồi bấm Đặt hàng. Hệ thống mở một transaction, kiểm tra lại tồn kho, sao lưu đơn giá (BR02), trừ tồn kho (BR01), ghi đơn hàng, chi tiết, bản ghi thanh toán và lịch sử, cuối cùng xóa giỏ hàng. Đơn COD vào trạng thái Chờ xác nhận, đơn VNPay vào trạng thái Chờ thanh toán."],
      ["5", "Ứng dụng quản trị", "Quản lý đăng nhập, màn hình Tổng quan hiển thị ngay số đơn đang chờ xác nhận."],
      ["6", "Ứng dụng quản trị", "Quản lý mở màn hình Quản lý đơn hàng, lọc theo trạng thái Chờ xác nhận, chọn đơn và xem chi tiết kèm lịch sử."],
      ["7", "Ứng dụng quản trị", "Quản lý bấm Xác nhận. Các nút thao tác chỉ bật khi bước chuyển hợp lệ theo sơ đồ trạng thái; mọi lần chuyển đều ghi thêm một dòng lịch sử."],
      ["8", "Ứng dụng quản trị", "Quản lý chuyển đơn sang Đang giao, rồi sang Hoàn thành khi giao xong. Ở bước Hoàn thành, bản ghi thanh toán COD chuyển sang trạng thái Thành công và doanh thu được ghi nhận (BR09)."],
      ["9", "Trang Web", "Khách mở mục Đơn hàng của tôi và thấy trạng thái cùng lịch sử xử lý được cập nhật. Khi đơn đã qua bước xác nhận, nút Hủy đơn không còn hiện ra nữa (BR05)."],
    ], "Các bước của quy trình đặt hàng và xử lý đơn đã cài đặt", 900, 2800),

    P("Nhánh rẽ cũng đã được cài đặt: nếu quản lý hoặc khách hàng hủy đơn, hệ thống hoàn lại đúng " +
      "số lượng đã trừ vào tồn kho và chuyển bản ghi thanh toán sang trạng thái Thất bại (BR03). " +
      "Trường hợp giao hàng không thành công được ghi nhận bằng chính trạng thái Đã hủy kèm lý do " +
      "trong lịch sử, đúng như đã chốt ở phần phân tích."),
    P("Với hình thức VNPay, sau bước 4 khách được chuyển sang cổng thanh toán. Yêu cầu thanh toán " +
      "được ký HMAC-SHA512 theo đặc tả VNPay 2.1.0; khi cổng chuyển trình duyệt về, hệ thống kiểm tra " +
      "lại chữ ký và số tiền, ghi mã giao dịch vào bảng ThanhToan rồi chuyển đơn từ Chờ thanh toán " +
      "sang Chờ xác nhận, sau đó quy trình tiếp tục từ bước 5. Nếu khách hủy giao dịch, đơn vẫn ở " +
      "Chờ thanh toán và có nút Thanh toán lại; quá 30 phút chưa thanh toán thì tác vụ nền tự hủy " +
      "đơn và hoàn tồn kho (BR04). Ở mỗi lần đặt hàng và đổi trạng thái, khách hàng nhận một email " +
      "thông báo."),

    // ---------------------------------------------------------- 5.2
    H2("5.2. Kiểm thử phần mềm"),

    H3("5.2.1. Chiến lược kiểm thử"),
    P("Hệ thống được kiểm thử ở hai mức. Mức đơn vị tập trung vào tầng nghiệp vụ, vì đây là nơi " +
      "chứa các quy tắc dễ sai và khó kiểm bằng mắt. Mức hệ thống chạy thử trọn vẹn các kịch bản " +
      "người dùng trên cả trang Web và ứng dụng quản trị, trên cùng một cơ sở dữ liệu thật."),
    P("Giao diện trang Web được kiểm thử tự động bằng Playwright (mục 5.2.3). Giao diện ứng dụng quản trị " +
      "WinUI 3 chưa có kiểm thử tự động vì công cụ tự động hóa cho WinUI 3 kém ổn định; các màn hình này " +
      "được kiểm tra thủ công theo danh sách kịch bản cũng ở mục 5.2.4."),

    H3("5.2.2. Kiểm thử mức đơn vị"),
    P("Project BookStore.Tests dùng xUnit, gồm 58 phương thức kiểm thử, trong đó hai phương thức " +
      "chạy theo nhiều bộ dữ liệu nên tổng cộng có 63 trường hợp kiểm thử được thực thi. Mỗi lớp " +
      "kiểm thử chạy trên một cơ sở dữ liệu SQLite nằm hoàn toàn trong bộ nhớ, được tạo mới và nạp " +
      "dữ liệu mẫu trước mỗi trường hợp, nhờ vậy các trường hợp không ảnh hưởng lẫn nhau và không " +
      "cần cài SQL Server để chạy kiểm thử."),
    t3("Lớp kiểm thử", "Số trường hợp", "Nội dung kiểm thử", [
      ["QuyTacNghiepVuTests", "6", "Công thức tính phí vận chuyển ở các mức dưới, đúng bằng và trên ngưỡng miễn phí; giá trị các hằng số nghiệp vụ."],
      ["DatHangTests", "10", "Trừ tồn kho và tính tổng tiền; miễn phí vận chuyển; từ chối khi vượt tồn kho; giỏ hàng trống; thiếu địa chỉ; trạng thái khởi tạo của đơn COD và đơn VNPay; sao lưu đơn giá; xóa giỏ sau khi đặt; phát sinh bản ghi thanh toán và lịch sử."],
      ["XuLyDonHangTests", "12", "Chuỗi chuyển trạng thái hợp lệ; từ chối bước nhảy cóc; đơn đã hoàn thành không đổi được; hoàn tồn kho khi hủy; ghi nhận giao thất bại; khách hủy đơn đúng và sai thời điểm; hủy đơn quá hạn thanh toán; doanh thu chỉ tính đơn hoàn thành; ghi nhận thanh toán COD."],
      ["TaiKhoanVaSachTests", "15", "Đăng nhập đúng và sai mật khẩu; chặn khách hàng vào ứng dụng quản trị và chặn quản lý vào trang Web; tạm khóa sau nhiều lần sai; đăng ký trùng tên, sai xác nhận mật khẩu, đăng ký hợp lệ và băm mật khẩu; kiểm tra dữ liệu khi thêm sách; xóa sách chưa và đã có giao dịch; tìm kiếm theo từ khóa, lọc theo giá và ẩn sách ngừng kinh doanh."],
      ["KhoVaDanhMucTests", "10", "Lập phiếu nhập chưa cộng tồn; hoàn thành phiếu thì cộng tồn đúng một lần và không sửa, xóa được nữa (BR06); dữ liệu phiếu không hợp lệ; xóa phiếu chưa hoàn thành; cảnh báo theo ngưỡng riêng của từng sách; thêm, sửa, xóa danh mục và chặn xóa khi đang được dùng; khóa và mở khóa tài khoản khách hàng; tra cứu khách hàng kèm số đơn, chi tiêu."],
      ["ThanhToanVaBaoCaoTests", "10", "Chữ ký VNPay đúng đặc tả và phát hiện dữ liệu bị sửa; thanh toán thành công chuyển đơn sang Chờ xác nhận và chỉ ghi nhận một lần; khách hủy giao dịch rồi thanh toán lại; đơn COD không tạo được yêu cầu thanh toán; email khi đặt hàng và đổi trạng thái; quên mật khẩu với token dùng một lần; báo cáo nhập – xuất – tồn; báo cáo doanh thu và tập tin Excel đọc lại được."],
      ["Tổng cộng", "63", "Toàn bộ đều đạt"],
    ], "Các lớp kiểm thử đơn vị và phạm vi kiểm thử", 3000, 1500),

    P("Kết quả chạy lệnh dotnet test: 63 trường hợp, 63 đạt, 0 thất bại, thời gian khoảng 8 giây. " +
      "Mười quy tắc nghiệp vụ BR01–BR10 đều có ít nhất một trường hợp kiểm thử tương ứng."),

    H3("5.2.3. Kiểm thử giao diện Web tự động"),
    P("Project BookStore.UiTests dùng xUnit kết hợp Playwright, điều khiển trình duyệt Microsoft Edge thật " +
      "để thao tác trên trang Web như một khách hàng. Trước khi chạy, bộ kiểm thử tự tạo một cơ sở dữ liệu " +
      "riêng (không đụng vào dữ liệu demo), khởi động trang Web trên cổng 5199 bằng một tiến trình riêng và " +
      "xóa cơ sở dữ liệu này khi kết thúc. Mỗi test dùng một phiên trình duyệt mới và một khách hàng đăng ký " +
      "mới nên các test không ảnh hưởng nhau. Gồm 24 trường hợp, chạy khoảng 14 giây:"),
    bullet("Trang sách (5): trang chủ, tìm kiếm có và không có kết quả, chi tiết sách, lọc theo giá."),
    bullet("Tài khoản (8): đăng nhập sai và đúng, đăng xuất, tài khoản Quản lý bị từ chối trên Web, đăng ký trùng tên và hợp lệ, hai trang yêu cầu đăng nhập, thông báo quên mật khẩu không lộ email có tồn tại hay không."),
    bullet("Giỏ hàng và đặt hàng (7): thêm vào giỏ khi chưa đăng nhập bị chuyển sang đăng nhập (BR01), tổng tiền khi đổi số lượng, miễn phí vận chuyển từ 300.000đ, đặt hàng COD và trừ tồn kho (BR02), thanh toán VNPay giả lập thành công và bị hủy, giỏ trống không đặt được."),
    bullet("Responsive (4): khung điện thoại 375 px không cuộn ngang ở ba trang, vẫn tìm kiếm và mở chi tiết sách được."),
    P("Bộ kiểm thử đã phát hiện một lỗi thật ngay lần chạy đầu: trang Web chưa nạp JavaScript của Bootstrap " +
      "nên menu người dùng (Đơn hàng của tôi, Đăng xuất) không mở được. Lỗi đã được sửa trong tệp App.razor."),

    H3("5.2.4. Kiểm thử mức hệ thống"),
    P("Kiểm thử mức hệ thống được thực hiện trên cơ sở dữ liệu SQL Server thật, với cả ba ứng dụng " +
      "cùng chạy. Mỗi kịch bản được thực hiện từ giao diện người dùng, sau đó kiểm tra lại dữ liệu " +
      "trong cơ sở dữ liệu để chắc chắn hệ thống không chỉ hiển thị đúng mà còn ghi đúng."),
    table(["STT", "Kịch bản", "Kết quả mong đợi", "Kết quả"], [
      ["1", "Đăng nhập trang Web bằng tài khoản khách hàng", "Vào được trang chủ, thanh điều hướng hiển thị tên khách", "Đạt"],
      ["2", "Đăng nhập trang Web bằng tài khoản quản lý", "Bị từ chối kèm thông báo dùng ứng dụng quản trị", "Đạt"],
      ["3", "Đăng nhập ứng dụng quản trị bằng tài khoản khách hàng", "Bị từ chối, thông báo không có quyền", "Đạt"],
      ["4", "Tìm sách theo từ khóa và lọc theo danh mục, khoảng giá", "Danh sách đúng điều kiện, có phân trang", "Đạt"],
      ["5", "Thêm sách vào giỏ với số lượng vượt tồn kho", "Bị từ chối, nêu rõ số lượng còn lại", "Đạt"],
      ["6", "Đặt hàng tổng dưới 300.000đ", "Cộng thêm phí vận chuyển 25.000đ", "Đạt"],
      ["7", "Đặt hàng tổng từ 300.000đ trở lên", "Miễn phí vận chuyển", "Đạt"],
      ["8", "Đặt hàng thành công bằng COD", "Đơn ở trạng thái Chờ xác nhận; tồn kho giảm đúng số lượng; giỏ hàng rỗng; phát sinh bản ghi thanh toán và một dòng lịch sử", "Đạt"],
      ["9", "Quản lý chuyển đơn Chờ xác nhận thẳng sang Hoàn thành", "Bị từ chối, nêu rõ không chuyển được giữa hai trạng thái", "Đạt"],
      ["10", "Quản lý lần lượt xác nhận, giao hàng, hoàn thành đơn", "Trạng thái đổi đúng thứ tự, mỗi bước thêm một dòng lịch sử kèm tên người thực hiện", "Đạt"],
      ["11", "Sửa đơn đã hoàn thành", "Bị từ chối", "Đạt"],
      ["12", "Xem màn hình Tổng quan sau khi hoàn thành đơn", "Doanh thu tháng và số đơn hoàn thành tăng tương ứng; danh sách sách bán chạy cập nhật", "Đạt"],
      ["13", "Khách mở Đơn hàng của tôi", "Thấy đúng trạng thái và lịch sử; đơn đã xác nhận không còn nút Hủy đơn", "Đạt"],
      ["14", "Gọi điểm cuối Web API khi không có token", "Máy chủ trả mã 401", "Đạt"],
      ["15", "Xóa sách đã phát sinh đơn hàng", "Không xóa, chuyển sang trạng thái Ngừng kinh doanh và sách biến mất khỏi trang Web", "Đạt"],
      ["16", "Đặt hàng bằng VNPay khi chưa cấu hình khóa sandbox", "Chuyển sang cổng giả lập với đúng số tiền; xác nhận thanh toán thì quay về trang kết quả, đơn ở Chờ xác nhận và có mã giao dịch", "Đạt"],
      ["17", "Quên mật khẩu với email đã đăng ký", "Email chứa liên kết được ghi ra tập tin; đặt lại mật khẩu thành công; dùng lại liên kết lần hai bị từ chối", "Đạt"],
      ["18", "Lập phiếu nhập rồi bấm Hoàn thành", "Tồn kho của sách tăng đúng số lượng nhập, báo cáo nhập – xuất – tồn khớp", "Đạt"],
      ["19", "Xóa phiếu nhập đã hoàn thành", "Bị từ chối theo BR06", "Đạt"],
      ["20", "Thêm rồi xóa một tác giả chưa gán cho sách", "Thêm và xóa thành công", "Đạt"],
      ["21", "Xuất báo cáo nhập – xuất – tồn ra Excel", "Tải về tập tin .xlsx mở được bằng Excel", "Đạt"],
      ["22", "Xem báo cáo với ngày bắt đầu sau ngày kết thúc", "Bị từ chối kèm thông báo", "Đạt"],
    ], [700, 3000, CONTENT_W - 5100, 1400], "Kết quả kiểm thử mức hệ thống"),

    H3("5.2.5. Lỗi phát hiện trong quá trình kiểm thử"),
    P("Quá trình kiểm thử giúp phát hiện và sửa ba lỗi đáng chú ý, đều thuộc loại khó nhận ra nếu " +
      "chỉ đọc mã nguồn:"),
    bulletL("Sai thứ tự middleware trên trang Web. ",
      "Ban đầu middleware chống giả mạo biểu mẫu được đặt trước middleware xác thực, nên token sinh " +
      "cho người đã đăng nhập lại bị đối chiếu với người dùng ẩn danh và mọi biểu mẫu đều trả về " +
      "lỗi 400. Sửa bằng cách gọi UseAuthentication và UseAuthorization trước UseAntiforgery."),
    bulletL("Tranh chấp DbContext trong Blazor Server. ",
      "Khi bố cục trang và nội dung trang cùng truy vấn dữ liệu, hai thành phần chạy đồng thời trên " +
      "cùng một DbContext và gây lỗi đóng kết nối. Sửa bằng cách đăng ký DbContext cùng các dịch vụ " +
      "theo vòng đời Transient để mỗi điểm tiêm phụ thuộc nhận một thể hiện riêng."),
    bulletL("Transaction xung đột với cơ chế thử lại của EF Core. ",
      "Chuỗi kết nối bật EnableRetryOnFailure khiến EF Core từ chối transaction mở thủ công. Sửa " +
      "bằng cách bọc transaction trong execution strategy như đã trình bày ở mục 4.4.4."),
    bulletL("Thư mục Documents không tồn tại. ",
      "Trên máy cài đặt, thư mục Documents bị OneDrive chuyển sang một đường dẫn không có thật nên " +
      "việc ghi email dự phòng thất bại. Nhờ thiết kế thích ứng, lỗi này chỉ được ghi log, đơn hàng " +
      "vẫn tạo bình thường. Sửa bằng cách thử Documents trước, không được thì ghi vào thư mục cạnh " +
      "tập tin chạy của ứng dụng; ứng dụng quản trị áp dụng cùng cách cho báo cáo Excel và hóa đơn."),

    // ---------------------------------------------------------- 5.3
    H2("5.3. Kết quả đạt được"),

    H3("5.3.1. Giao diện trang Web dành cho khách hàng"),
    P("Trang Web được dựng bằng Blazor Web App ở chế độ Interactive Server, giao diện dùng Bootstrap " +
      "5 nên tự co giãn theo chiều rộng màn hình. Các hình dưới đây là ảnh chụp màn hình thật của " +
      "hệ thống đang chạy."),
    figureOpt("kq_w01_trangchu", "Trang chủ với danh mục và danh sách sách mới cập nhật"),
    figureOpt("kq_w02_timkiem", "Trang kết quả tìm kiếm kèm bộ lọc theo danh mục, khoảng giá và sắp xếp"),
    figureOpt("kq_w03_chitietsach", "Trang chi tiết sách và chức năng thêm vào giỏ hàng"),
    figureOpt("kq_w04_giohang", "Giỏ hàng – cập nhật số lượng không tải lại trang, tính phí vận chuyển theo BR10"),
    figureOpt("kq_w05_thanhtoan", "Trang thanh toán với địa chỉ giao hàng được điền sẵn và hai hình thức thanh toán"),
    figureOpt("kq_w06_ketqua", "Trang kết quả đặt hàng kèm lịch sử xử lý đơn"),
    figureOpt("kq_w07_donhangcuatoi", "Mục Đơn hàng của tôi – đơn đã xác nhận không còn nút hủy theo BR05"),
    figureOpt("kq_w08_dangnhap", "Màn hình đăng nhập của khách hàng"),

    H3("5.3.2. Giao diện ứng dụng quản trị"),
    P("Ứng dụng quản trị dùng WinUI 3 theo mẫu MVVM: mỗi màn hình gồm một tệp XAML mô tả giao diện " +
      "và một lớp ViewModel giữ dữ liệu cùng các lệnh, hai bên nối với nhau bằng data binding. " +
      "Khung điều hướng là một NavigationView bên trái với bảy mục: Tổng quan, Quản lý đơn hàng, " +
      "Quản lý sách, Danh mục, Kho và nhập hàng, Khách hàng, Báo cáo thống kê."),
    bulletL("Màn hình Đăng nhập. ",
      "Nhập tên đăng nhập và mật khẩu, gọi điểm cuối /api/xacthuc/dangnhap và giữ token JWT trong " +
      "bộ nhớ của phiên làm việc. Nhấn phím Enter cũng gửi biểu mẫu; trong lúc chờ phản hồi, nút " +
      "đăng nhập bị tắt và vòng xoay tiến trình hiện lên."),
    bulletL("Màn hình Tổng quan. ",
      "Năm thẻ số liệu: đơn chờ xác nhận, đơn đang giao, đơn hoàn thành trong tháng, doanh thu " +
      "tháng và số sách sắp hết hàng; bên dưới là bảng sách bán chạy."),
    bulletL("Màn hình Quản lý đơn hàng. ",
      "Bên trái là danh sách đơn có ô tìm kiếm và bộ lọc theo trạng thái, mỗi dòng gắn một nhãn màu " +
      "theo trạng thái. Bên phải là chi tiết đơn gồm thông tin người nhận, các dòng hàng, lịch sử " +
      "xử lý, ô ghi chú và bốn nút chuyển trạng thái. Bốn nút này chỉ bật khi bước chuyển hợp lệ, " +
      "nên quản lý không thể bấm nhầm. Nút In hóa đơn sinh hóa đơn bán hàng và mở hộp thoại in."),
    bulletL("Màn hình Quản lý sách. ",
      "Danh sách sách kèm ô tìm kiếm ở bên trái, biểu mẫu thêm hoặc sửa ở bên phải. Chọn một dòng " +
      "trong danh sách thì biểu mẫu tự điền; biểu mẫu cho chọn loại sách, nhà xuất bản, nhiều tác " +
      "giả và ngưỡng cảnh báo. Nút Xóa chỉ bật khi đang sửa một quyển sách có sẵn."),
    bulletL("Màn hình Danh mục. ",
      "Một màn hình dùng chung cho bốn danh mục: loại sách, tác giả, nhà xuất bản, nhà cung cấp. " +
      "Biểu mẫu tự hiện đúng các trường của danh mục đang chọn; mục đang được sách hoặc phiếu nhập " +
      "sử dụng thì không xóa được."),
    bulletL("Màn hình Kho và nhập hàng. ",
      "Thẻ Phiếu nhập cho lập, sửa, hoàn thành và xóa phiếu; thẻ Tồn kho liệt kê tồn kho, đánh dấu " +
      "sách sắp hết và cho đặt ngưỡng cảnh báo của từng sách."),
    bulletL("Màn hình Khách hàng. ",
      "Tra cứu khách hàng, xem thông tin, số đơn và tổng chi tiêu, khóa hoặc mở khóa tài khoản kèm lý do."),
    bulletL("Màn hình Báo cáo thống kê. ",
      "Chọn khoảng thời gian để xem doanh thu theo ngày dưới dạng biểu đồ thanh, sách bán chạy, đơn " +
      "theo trạng thái và bảng nhập – xuất – tồn; hai nút xuất báo cáo ra tập tin Excel."),
    figureOpt("kq_d01_dangnhap", "Màn hình đăng nhập của ứng dụng quản trị"),
    figureOpt("kq_d02_tongquan", "Màn hình Tổng quan của ứng dụng quản trị"),
    figureOpt("kq_d03_donhang", "Màn hình Quản lý đơn hàng – chi tiết đơn và các nút chuyển trạng thái"),
    figureOpt("kq_d04_sach", "Màn hình Quản lý sách – danh sách và biểu mẫu thêm, sửa"),
    figureOpt("kq_d05_danhmuc", "Màn hình Danh mục – loại sách, tác giả, nhà xuất bản, nhà cung cấp"),
    figureOpt("kq_d06_kho", "Màn hình Kho và nhập hàng – phiếu nhập đã hoàn thành không sửa, xóa được (BR06)"),
    figureOpt("kq_d07_khachhang", "Màn hình Quản lý khách hàng"),
    figureOpt("kq_d08_baocao", "Màn hình Báo cáo thống kê – doanh thu theo ngày, sách bán chạy, đơn theo trạng thái"),

    H3("5.3.3. Web API"),
    P("Web API cung cấp 35 điểm cuối như đã liệt kê ở Bảng 4.6, kèm giao diện Swagger UI để tra cứu " +
      "và thử trực tiếp. Giao diện này cũng là công cụ hữu ích khi kiểm thử, vì cho phép gọi từng " +
      "điểm cuối mà không cần mở ứng dụng Desktop."),
    figureOpt("kq_api_swagger", "Giao diện Swagger UI của Web API"),

    H3("5.3.4. Đối chiếu với yêu cầu của đề bài"),
    table(["Yêu cầu của đề bài", "Mức độ đáp ứng"], [
      ["Ứng dụng quản trị chạy trên Windows bằng C#, có đăng nhập và phân quyền", "Đạt. Ứng dụng WinUI 3, đăng nhập bằng JWT, phân quyền theo hai vai trò, tài khoản khách hàng bị từ chối."],
      ["Màn hình thêm, xóa, sửa, xem, tìm kiếm cho danh mục nghiệp vụ", "Đạt. Sách, loại sách, tác giả, nhà xuất bản, nhà cung cấp, phiếu nhập và khách hàng đều có màn hình thêm, sửa, xóa (hoặc khóa), xem và tìm kiếm."],
      ["Trang Web người dùng bằng C#, có giao diện chính, menu, danh mục, tìm kiếm, chi tiết và chức năng nghiệp vụ, responsive", "Đạt. Trang Web viết hoàn toàn bằng C# với Blazor, đủ các thành phần nêu trên, bố cục co giãn nhờ Bootstrap 5."],
      ["Hai thành phần dùng chung một cơ sở dữ liệu SQL Server", "Đạt. Cùng một cơ sở dữ liệu QuanLyBanSachTrucTuyen; đơn khách đặt trên Web hiện ngay trong ứng dụng quản trị."],
      ["Cơ sở dữ liệu đạt chuẩn 3NF, có khóa chính và khóa ngoại hợp lý", "Đạt. 19 bảng, đã kiểm tra 1NF, 2NF, 3NF ở mục 3.3; khóa ngoại và ràng buộc kiểm tra được khai báo đầy đủ."],
      ["Hệ thống chạy và demo được", "Đạt. Cả ba ứng dụng chạy được, quy trình đặt hàng và xử lý đơn hoàn chỉnh từ đầu đến cuối."],
      ["Điểm sáng tạo: WinUI 3, Blazor, EF Core, Web API, DI, Unit Test, dashboard, xuất báo cáo", "Đạt: WinUI 3, Blazor, EF Core Code First, Web API có JWT và Swagger, Dependency Injection ở cả ba ứng dụng, 63 trường hợp kiểm thử đơn vị, 24 trường hợp kiểm thử giao diện Web bằng Playwright, màn hình dashboard và báo cáo, xuất báo cáo ra Excel bằng ClosedXML; thêm thanh toán VNPay và gửi email bằng MailKit."],
    ], [4200, CONTENT_W - 4200], "Đối chiếu kết quả với yêu cầu của đề bài"),

    H3("5.3.5. Bình luận"),
    P("Về ưu điểm, điểm nhóm hài lòng nhất là việc tách tầng nghiệp vụ thành một project riêng đã " +
      "chứng minh được giá trị trong thực tế chứ không chỉ trên sơ đồ. Quy tắc trừ tồn kho và hoàn " +
      "tồn kho chỉ được viết một lần nhưng có hiệu lực ở cả ba đường vào hệ thống: khách đặt hàng " +
      "trên Web, khách tự hủy đơn trên Web và quản lý hủy đơn trên ứng dụng quản trị. Việc mã hóa " +
      "sơ đồ trạng thái thành một từ điển cũng khiến phần kiểm tra bước chuyển gọn và dễ kiểm thử " +
      "hơn hẳn so với cách viết nhiều lệnh điều kiện."),
    P("Việc có bộ kiểm thử đơn vị cũng tỏ ra hữu ích hơn nhóm dự đoán. Khi sửa lỗi transaction xung " +
      "đột với cơ chế thử lại, nhóm phải đổi cấu trúc hai phương thức dài; bộ kiểm thử chạy lại " +
      "trong vài giây và khẳng định ngay rằng hành vi nghiệp vụ không đổi."),
    P("Về hạn chế, thanh toán VNPay đã được cài đặt đúng đặc tả nhưng mới kiểm thử với cổng giả lập " +
      "của hệ thống, vì cần mã merchant sandbox do VNPay cấp mới gọi được cổng thật. Hệ thống cũng " +
      "chỉ xử lý kết quả qua địa chỉ trả về trên trình duyệt (Return URL), chưa xử lý thông báo từ " +
      "máy chủ VNPay gửi sang (IPN) vì cơ chế này đòi hỏi máy chủ có địa chỉ công khai trên Internet. " +
      "Tương tự, email mới được kiểm thử ở chế độ ghi ra tập tin; gửi qua SMTP thật cần tài khoản " +
      "và mật khẩu ứng dụng của nhà cung cấp email."),
    P("Một hạn chế nữa nằm ở phần xác thực của trang Web. Báo cáo giai đoạn 1 dự kiến dùng ASP.NET " +
      "Core Identity với bộ lưu trữ tùy biến trỏ vào bảng TaiKhoan. Khi cài đặt, nhóm nhận thấy " +
      "việc hiện thực đầy đủ các giao diện IUserStore và IRoleStore tốn nhiều công hơn giá trị thu " +
      "được, nên chuyển sang dùng cookie authentication của ASP.NET Core kết hợp lớp PasswordHasher " +
      "của Identity để băm mật khẩu. Kết quả về mặt bảo mật mật khẩu là tương đương, nhưng hệ thống " +
      "chưa có sẵn các tính năng kèm theo của Identity như xác thực hai lớp hay xác nhận email."),
    P("Cuối cùng, ứng dụng quản trị WinUI 3 chưa có kiểm thử giao diện tự động và vẫn được kiểm tra thủ công. " +
      "Nếu dự án tiếp tục phát triển, đây là mảng cần bổ sung, vì số màn hình sẽ tăng nhanh hơn khả năng kiểm tra bằng tay."),
  ];

  // ================================================================ CHƯƠNG 6
  const ch6 = [
    H1("KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN"),

    H2("6.1. Kết luận"),
    P("Đồ án đã đi trọn một vòng từ khảo sát, phân tích, thiết kế tới cài đặt và kiểm thử một hệ " +
      "thống quản lý và bán sách trực tuyến. Sản phẩm cuối cùng là một solution .NET 10 gồm bảy " +
      "project với khoảng 11.300 dòng mã do nhóm tự viết, chạy trên cơ sở dữ liệu SQL Server với 19 " +
      "bảng đã chuẩn hóa tới dạng chuẩn 3."),
    P("Hệ thống đáp ứng các yêu cầu bắt buộc của đề bài: có đủ hai thành phần là ứng dụng quản trị " +
      "trên Windows và trang Web cho khách hàng, cả hai dùng chung một cơ sở dữ liệu, chạy và demo " +
      "được. Quy trình nghiệp vụ trung tâm là đặt hàng và xử lý đơn đã được cài đặt trọn vẹn, đi " +
      "liền mạch từ lúc khách tìm sách trên trang Web cho tới khi quản lý hoàn tất đơn trên ứng " +
      "dụng quản trị và doanh thu được ghi nhận. Toàn bộ các phân hệ đã phân tích ở Chương 3 đều đã " +
      "có mã nguồn: danh mục, kho và phiếu nhập, khách hàng, báo cáo, in hóa đơn, xuất Excel, thanh " +
      "toán VNPay và gửi email."),
    P("Về mặt kỹ thuật, nhóm đã vận dụng được các công nghệ nằm trong nhóm điểm sáng tạo mà đề bài " +
      "khuyến khích: WinUI 3 với mẫu MVVM, Blazor, Entity Framework Core theo hướng Code First, " +
      "Web API có xác thực JWT và tài liệu Swagger, Dependency Injection ở cả ba ứng dụng, 63 " +
      "trường hợp kiểm thử đơn vị với xUnit, màn hình dashboard và xuất báo cáo ra Excel."),

    H2("6.2. Những điều rút ra được"),
    bulletL("Kiến trúc chỉ chứng minh được giá trị khi bắt đầu viết mã. ",
      "Trên sơ đồ, việc tách tầng nghiệp vụ thành project riêng trông giống một quy ước hình thức. " +
      "Chỉ khi cài đặt chức năng hủy đơn ở hai nơi khác nhau, nhóm mới thấy rõ nếu không tách thì " +
      "sẽ phải chép cùng một đoạn xử lý tồn kho sang hai chỗ."),
    bulletL("Lỗi khó nhất thường nằm ở chỗ ghép nối chứ không ở thuật toán. ",
      "Ba lỗi mất nhiều thời gian nhất của nhóm đều là lỗi về cách các thành phần hạ tầng phối hợp " +
      "với nhau: thứ tự middleware, vòng đời đối tượng trong DI và sự xung đột giữa transaction với " +
      "cơ chế thử lại, chứ không phải lỗi nghiệp vụ."),
    bulletL("Kiểm thử đơn vị tiết kiệm thời gian ngay trong cùng một kỳ. ",
      "Với một đồ án ngắn, dễ nghĩ rằng viết kiểm thử là mất thời gian. Thực tế bộ kiểm thử đã " +
      "giúp nhóm đổi cấu trúc mã nguồn một cách yên tâm và phát hiện sớm vài sai sót về tính tiền."),
    bulletL("Quy tắc nghiệp vụ nên được viết thành dữ liệu hoặc hằng số. ",
      "Đưa phí vận chuyển, ngưỡng miễn phí và bảng chuyển trạng thái ra thành khai báo riêng khiến " +
      "mã nguồn ngắn hơn, đồng thời cho phép lớp kiểm thử dùng lại chính những khai báo đó."),

    H2("6.3. Hạn chế của đề tài"),
    bullet("Thanh toán VNPay mới kiểm thử với cổng giả lập, chưa chạy với mã merchant sandbox thật; chưa xử lý thông báo IPN từ máy chủ VNPay và chưa có chức năng hoàn tiền."),
    bullet("Email mới kiểm thử ở chế độ ghi ra tập tin, chưa gửi qua máy chủ SMTP thật; email được gửi ngay trong yêu cầu nên nếu máy chủ SMTP chậm, thao tác của người dùng cũng chậm theo."),
    bullet("Báo cáo nhập – xuất – tồn suy ngược từ tồn kho hiện tại; nếu quản lý sửa trực tiếp số lượng tồn trong màn hình Quản lý sách thì báo cáo của các kỳ trước sẽ lệch tương ứng."),
    bullet("Ứng dụng quản trị chưa có kiểm thử giao diện tự động (trang Web đã có 24 test Playwright) và chưa đo được khả năng chịu tải của hệ thống."),
    bullet("Hệ thống mới chạy trên môi trường máy cá nhân, chưa triển khai lên máy chủ thật và chưa cấu hình chứng chỉ HTTPS chính thức."),

    H2("6.4. Hướng phát triển"),
    P("Nếu đề tài được tiếp tục, nhóm dự kiến phát triển theo bốn hướng, xếp theo thứ tự ưu tiên."),
    t3("Thứ tự", "Hướng phát triển", "Nội dung cụ thể", [
      ["1", "Đưa thanh toán và email vào môi trường thật", "Đăng ký mã merchant VNPay sandbox, bổ sung xử lý IPN và đối soát giao dịch, chức năng hoàn tiền; cấu hình máy chủ SMTP thật và chuyển việc gửi email sang hàng đợi chạy nền để không làm chậm thao tác."],
      ["2", "Đóng gói và triển khai", "Đóng gói trang Web, Web API và SQL Server bằng Docker để chạy được trên máy mới bằng một lệnh; triển khai lên máy chủ có chứng chỉ HTTPS; ghi nhận biến động tồn kho thành bảng riêng để báo cáo nhập – xuất – tồn chính xác cả khi sửa tồn thủ công."],
      ["3", "Nâng chất lượng và trải nghiệm", "Bổ sung kiểm thử tự động cho giao diện ứng dụng quản trị; thêm biểu đồ doanh thu trên dashboard bằng LiveCharts2; thêm bộ nhớ đệm cho danh mục và trang chủ; bổ sung phân trang phía máy chủ cho các danh sách dài trong ứng dụng quản trị."],
      ["4", "Mở rộng phạm vi nghiệp vụ", "Đưa trở lại hai chức năng đã tạm gác là khuyến mãi và đánh giá sách; thêm gợi ý sách theo lịch sử mua hàng; hỗ trợ nhiều kho hàng; xây dựng ứng dụng di động dùng chung Web API hiện có."],
    ], "Hướng phát triển của đề tài", 1100, 3400),

    P("Trong bốn hướng trên, hướng thứ nhất và thứ hai giúp hệ thống đã cài đặt chạy được trong môi " +
      "trường thật, nên cần làm trước. Hai hướng còn lại mở rộng ra ngoài phạm vi ban đầu và chỉ nên " +
      "thực hiện khi phần lõi đã ổn định."),
  ];

  return { ch4, ch5, ch6 };
};
