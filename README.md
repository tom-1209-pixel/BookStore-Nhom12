# Hệ thống quản lý và bán sách trực tuyến

Đồ án môn **Ngôn ngữ lập trình C#** – Trường Đại học Sài Gòn, Khoa Công nghệ Thông tin.
Giảng viên giảng dạy: **TS. Đỗ Như Tài**. Nhóm **12**.

## Thành viên

| STT | Họ và tên | MSSV | GitHub |
|-----|-----------|------|--------|
| 1 | Phạm Hoàng Sang | 3124411256 | [`tom-1209-pixel`](https://github.com/tom-1209-pixel) |
| 2 | Võ Trần Hải Đăng | 3124411069 | [`dangvotranhai-dot`](https://github.com/dangvotranhai-dot) |
| 3 | Ngô Hồ Đăng Khoa | 3124411130 | [`khoango6688-collab`](https://github.com/khoango6688-collab) |

## Giới thiệu

Hệ thống gồm hai thành phần dùng chung một cơ sở dữ liệu Microsoft SQL Server:

- **Trang Web bán sách** cho khách hàng – Blazor Web App (trang render tĩnh, giỏ hàng Interactive Server): tìm kiếm, giỏ hàng, đặt hàng, thanh toán COD hoặc VNPay, tài khoản và sổ địa chỉ, quên mật khẩu.
- **Ứng dụng quản trị Desktop** cho Quản lý nhà sách – WinUI 3, gọi Web API (sách kèm ảnh bìa, danh mục, đơn hàng, kho và nhập hàng, khách hàng, báo cáo thống kê, xuất Excel).

Có hai vai trò: **Khách hàng** (Web) và **Quản lý** (Desktop). Tài khoản Quản lý không đăng nhập được trên Web.

## Công nghệ

- .NET 10, C#
- Blazor Web App + Bootstrap 5, xác thực cookie (Web) và JWT (Web API)
- WinUI 3 (Windows App SDK) + MVVM (CommunityToolkit.Mvvm)
- ASP.NET Core Web API + Swagger
- Entity Framework Core (Code First, Migrations) + Microsoft SQL Server
- xUnit (tầng Services) và Playwright (giao diện Web)
- Tích hợp: VNPay (kèm cổng giả lập có mã QR, thẻ và OTP), MailKit (email), ClosedXML (xuất Excel), QRCoder

## Kiến trúc

Solution `BookStore.slnx` gồm các project trong `src/`:

| Project | Vai trò |
|---|---|
| `BookStore.Domain` | Entity, enum trạng thái |
| `BookStore.Data` | DbContext, Migrations, dữ liệu mẫu |
| `BookStore.Services` | Nghiệp vụ, validation, DTO; đăng ký qua DI |
| `BookStore.Api` | Web API cho ứng dụng quản trị (JWT, Swagger) |
| `BookStore.Web` | Trang Web khách hàng; gọi thẳng `BookStore.Services` |
| `BookStore.Admin` | Ứng dụng quản trị WinUI 3; gọi `BookStore.Api` bằng `HttpClient` |
| `BookStore.Tests` | xUnit cho tầng Services (SQLite in-memory) |
| `BookStore.UiTests` | Playwright kiểm thử giao diện trang Web |

## Cấu trúc thư mục

```
BaoCao/          Báo cáo đồ án (.docx chia theo chương)
DacTaUseCase/    Đặc tả chi tiết use case
src/             Mã nguồn
src/AnhBia/      Ảnh bìa sách (dùng chung giữa Web và Web API)
tools/           Script sinh báo cáo, sơ đồ, slide, ảnh bìa mẫu
```

## Chạy nhanh bằng Docker

Cần Docker Desktop. Một lệnh dựng SQL Server, Web API và trang Web:

```bash
docker compose up --build
```

- Trang Web: http://localhost:5159 (tài khoản `khachhang` / `123456`)
- Web API (Swagger): http://localhost:5171/swagger (đăng nhập `quanly` / `123456`)
- Ứng dụng quản trị Desktop chạy trên máy (không nằm trong Docker), trỏ tới `http://localhost:5171`.

Lần đầu Docker kéo khoảng 1,5 GB image và CSDL được tạo kèm dữ liệu mẫu. Đổi mật khẩu SA hoặc khóa JWT bằng cách
sao chép `.env.example` thành `.env`. Dừng và xóa dữ liệu: `docker compose down -v`.

## Chạy thử không dùng Docker

Yêu cầu: Windows 10/11, .NET SDK 10, SQL Server (bản Developer hoặc Express, chạy ở `localhost`).

```bash
dotnet build
dotnet run --project src/BookStore.Api --launch-profile http      # cổng 5171, Swagger ở /swagger
dotnet run --project src/BookStore.Web --launch-profile http      # cổng 5159
dotnet run --project src/BookStore.Admin                          # ứng dụng quản trị, cần Web API đang chạy
```

Lần chạy đầu, CSDL `QuanLyBanSachTrucTuyen` được tạo và nạp dữ liệu mẫu.
Tài khoản mẫu (mật khẩu `123456`): `khachhang` (Web), `quanly` (ứng dụng quản trị).

Chuỗi kết nối mặc định: `Server=localhost;Database=QuanLyBanSachTrucTuyen;Trusted_Connection=True;TrustServerCertificate=True`
(sửa trong `appsettings.json` của `BookStore.Api` và `BookStore.Web`).

### Kiểm thử

```bash
dotnet test src/BookStore.Tests      # không cần SQL Server
dotnet test src/BookStore.UiTests    # cần SQL Server và Microsoft Edge; tắt Web đang chạy ở cổng 5199
```

### VNPay và email (tùy chọn)

Mặc định hệ thống dùng cổng thanh toán giả lập (`/vnpay-gia-lap`) và ghi email ra tập tin `.eml`.
Thẻ thử của cổng giả lập (giống VNPay sandbox): ngân hàng NCB, số thẻ `9704198526191432198`, tên `NGUYEN VAN A`,
ngày phát hành `07/15`, OTP `123456`.

Ảnh bìa lưu ở `src/AnhBia` (đổi bằng khóa cấu hình `AnhBia:ThuMuc`; khi chạy Docker là volume `anh-bia`).
Muốn dùng VNPay sandbox hoặc SMTP thật, đặt khóa bằng user-secrets (không đưa lên GitHub):

```bash
dotnet user-secrets set "VnPay:TmnCode" "<ma>" --project src/BookStore.Web
dotnet user-secrets set "VnPay:HashSecret" "<khoa>" --project src/BookStore.Web
```

## Quy trình làm việc

Mỗi thay đổi làm trên một nhánh riêng, mở Pull Request vào `main`, một bạn khác trong nhóm xem rồi mới merge.
Mẫu mô tả PR nằm ở `.github/pull_request_template.md`.
