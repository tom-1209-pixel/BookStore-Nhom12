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

- **Trang Web bán sách** cho khách hàng – Blazor Web App (Interactive Server), thanh toán COD hoặc VNPay.
- **Ứng dụng quản trị Desktop** cho Quản lý nhà sách – WinUI 3, gọi Web API (sách, danh mục, đơn hàng, kho và nhập hàng, khách hàng, báo cáo thống kê, xuất Excel).

Có hai vai trò: **Khách hàng** (Web) và **Quản lý** (Desktop). Tài khoản Quản lý không đăng nhập được trên Web.

## Công nghệ

- .NET 10, C#
- Blazor Web App + Bootstrap 5, xác thực cookie (Web) và JWT (Web API)
- WinUI 3 (Windows App SDK) + MVVM (CommunityToolkit.Mvvm)
- ASP.NET Core Web API + Swagger
- Entity Framework Core (Code First, Migrations) + Microsoft SQL Server
- xUnit (tầng Services) và Playwright (giao diện Web)
- Tích hợp: VNPay (kèm cổng giả lập), MailKit (email), ClosedXML (xuất Excel)

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
tools/           Script sinh báo cáo, sơ đồ, slide
```

## Chạy thử

Yêu cầu: Windows 10/11, .NET SDK 10, SQL Server (bản Developer hoặc Express, chạy ở `localhost`).

```bash
dotnet build
dotnet run --project src/BookStore.Api --launch-profile http      # cổng 5171, Swagger ở /swagger
dotnet run --project src/BookStore.Web --launch-profile http      # cổng 5159
src/BookStore.Admin/bin/Debug/net10.0-windows10.0.19041.0/win-x64/BookStore.Admin.exe   # cần Web API đang chạy
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
Muốn dùng VNPay sandbox hoặc SMTP thật, đặt khóa bằng user-secrets (không đưa lên GitHub):

```bash
dotnet user-secrets set "VnPay:TmnCode" "<ma>" --project src/BookStore.Web
dotnet user-secrets set "VnPay:HashSecret" "<khoa>" --project src/BookStore.Web
```

## Quy trình làm việc

Mỗi thay đổi làm trên một nhánh riêng, mở Pull Request vào `main`, một bạn khác trong nhóm xem rồi mới merge.
Mẫu mô tả PR nằm ở `.github/pull_request_template.md`.
