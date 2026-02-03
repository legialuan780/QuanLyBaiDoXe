# 🚗 Hệ Thống Quản Lý Bãi Đỗ Xe

Hệ thống quản lý bãi đỗ xe thông minh được xây dựng bằng **ASP.NET Core MVC** với **.NET 8**, hỗ trợ quản lý xe ra vào, thẻ xe, nhân viên, và các tính năng nâng cao.

## 📋 Tính Năng Chính

### 🔐 Quản Lý Người Dùng
- Đăng ký/Đăng nhập tài khoản
- Phân quyền (Admin, Nhân viên, Khách hàng)
- Quên mật khẩu & Reset password qua email
- Xác thực Cookie-based Authentication

### 🚘 Quản Lý Xe & Thẻ Xe
- **Luồng ra/vào xe**: Ghi nhận thời gian, biển số, loại xe
- **Thẻ tháng**: Đăng ký, gia hạn, theo dõi lịch sử
- **Thẻ lượt**: Tính phí theo giờ/ngày
- **AI Vision**: Nhận diện biển số xe tự động (Vehicle Vision)

### 🏢 Quản Lý Bãi Đỗ
- **Khu vực & Vị trí đỗ**: Phân bổ chỗ đỗ xe theo khu vực
- **Đặt chỗ trước**: Khách hàng đặt vị trí đỗ xe
- **Cấu hình giá**: Thiết lập giá theo loại xe, thời gian
- **Báo cáo sự cố**: Ghi nhận và xử lý sự cố

### 👥 Quản Lý Nhân Viên
- **Ca làm việc**: Đăng ký và sắp xếp ca
- **Lịch làm việc**: Quản lý lịch theo tuần/tháng
- **Giao ca**: Ghi chú và xác nhận giao ca

## 🛠️ Công Nghệ Sử Dụng

- **Framework**: ASP.NET Core MVC (.NET 8)
- **Database**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core
- **Authentication**: Cookie Authentication
- **Frontend**: Razor Views, JavaScript, Bootstrap
- **Email Service**: SMTP Integration
- **AI/ML**: Vehicle Vision (OCR biển số xe)

## 📦 Cài Đặt & Chạy Dự Án

### Yêu Cầu Hệ Thống
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server hoặc SQL Server LocalDB
- Visual Studio 2022+ (khuyến nghị) hoặc VS Code

### Các Bước Cài Đặt

1. **Clone repository**
2. **Cấu hình Database**
- Mở file `appsettings.json` vàcập nhật connection string:
3. **Tạo Database**

Hoặc dùng Migration (nếu có):

4. **Chạy ứng dụng**
Hoặc nhấn **F5** trong Visual Studio

5. **Truy cập ứng dụng**

## 📂 Cấu Trúc Dự Án

## 🔧 Cấu Hình Quan Trọng

### Email Service (appsettings.json)

### Encoding UTF-8 (Tiếng Việt)
Dự án đã được cấu hình UTF-8 để hỗ trợ tiếng Việt. Xem thêm: `ENCODING_FIX_GUIDE.md`

## 👨‍💻 Nhóm Phát Triển

- **Repository**: [github.com/legialuan780/QuanLyBaiDoXe](https://github.com/legialuan780/QuanLyBaiDoXe)
- **Branch chính**: `main`
- **Branch phát triển**: `khoi_code`

## 📝 Ghi Chú

- Đảm bảo SQL Server đang chạy trước khi khởi động ứng dụng
- Tài khoản admin mặc định (nếu có) xem trong Database script
- Đối với tính năng Vehicle Vision, cần cấu hình thêm AI model

## 📄 License

[Thêm thông tin license của bạn ở đây]

---

**Phát triển bởi**: Nhóm QuanLyBaiDoXe  
**Năm**: 2026