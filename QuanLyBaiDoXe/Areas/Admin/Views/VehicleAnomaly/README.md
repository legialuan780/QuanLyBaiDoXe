# Module Quản lý Sự cố (VehicleAnomaly)

## Tổng quan
Module quản lý sự cố được thiết kế để theo dõi, phân loại và xử lý các sự cố xảy ra trong hệ thống quản lý bãi đỗ xe.

## Các chức năng chính

### 1. Quản lý Sự cố (VehicleAnomalyController)
**Đường dẫn:** `/Admin/VehicleAnomaly/Index`

#### Tính năng:
- **Xem danh sách sự cố**: Hiển thị tất cả sự cố theo thời gian thực
- **Phân loại sự cố**: 
  - Xe mất thẻ
  - Lỗi barrier
  - Lỗi camera
  - Tranh chấp phí
  - Khẩn cấp
  - Khác

- **Trạng thái xử lý**:
  - 0: Chưa xử lý (màu đỏ)
  - 1: Đang xử lý (màu vàng)
  - 2: Đã xử lý (màu xanh)

- **Bộ lọc nâng cao**:
  - Theo ngày
  - Theo loại sự cố
  - Theo trạng thái
  - Tìm kiếm theo mã thẻ/mô tả

#### API Endpoints:
```csharp
GET  /Admin/VehicleAnomaly/Index              // Danh sách sự cố
GET  /Admin/VehicleAnomaly/Details/{id}       // Chi tiết sự cố
POST /Admin/VehicleAnomaly/UpdateStatus       // Cập nhật trạng thái
POST /Admin/VehicleAnomaly/AssignStaff        // Điều nhân viên
GET  /Admin/VehicleAnomaly/GetStaffList       // Danh sách nhân viên
```

### 2. Giám sát Realtime (AnomalyMonitorController)
**Đường dẫn:** `/Admin/AnomalyMonitor/RealTimeMonitor`

#### Tính năng:
- **Màn hình giám sát trực tiếp**:
  - Cập nhật tự động mỗi 5 giây
  - Cảnh báo âm thanh khi có sự cố mới
  - Badge LIVE hiển thị trạng thái

- **Thống kê tổng quan**:
  - Số sự cố khẩn cấp
  - Số sự cố chờ xử lý
  - Số sự cố đang xử lý
  - Thời gian xử lý trung bình

- **Bản đồ nhiệt**:
  - Hiển thị vị trí sự cố trên sơ đồ bãi xe
  - Mã màu theo mức độ nghiêm trọng
  - Hiệu ứng pulse cho sự cố mới

- **Phân tích theo loại**:
  - Biểu đồ thanh ngang
  - Số lượng và tỷ lệ đã xử lý

#### API Endpoints:
```csharp
GET /Admin/AnomalyMonitor/GetRealtimeAnomalies    // Lấy sự cố realtime
GET /Admin/AnomalyMonitor/GetStatisticsByTime     // Thống kê theo thời gian
GET /Admin/AnomalyMonitor/GetStatisticsByType     // Thống kê theo loại
```

### 3. Chi tiết Sự cố (_AnomalyDetailsModal)
**View:** Partial view hiển thị trong modal

#### Thông tin hiển thị:
- Thời gian ghi nhận
- Loại sự cố với icon và màu sắc
- Mã thẻ/Biển số xe
- Vị trí xảy ra sự cố
- Trạng thái xử lý
- Nhân viên được gán
- Mức độ ưu tiên
- Mô tả chi tiết
- Video camera (nếu có)
- Hình ảnh bằng chứng (nếu có)
- Lịch sử xử lý với timeline

#### Actions:
- Xem chi tiết đầy đủ
- Điều nhân viên xử lý
- Đánh dấu hoàn thành
- Tải lên video/hình ảnh

## ViewModels

### VehicleAnomalyViewModel
```csharp
- MaSuCo: int                   // Mã sự cố
- ThoiGianGhiNhan: DateTime?    // Thời gian phát hiện
- MaNhanVien: int?              // Mã nhân viên xử lý
- TenNhanVien: string?          // Tên nhân viên
- LoaiSuCo: string?             // Loại sự cố
- MaThe: string?                // Mã thẻ xe
- MaViTri: int?                 // Vị trí trong bãi
- MoTaChiTiet: string?          // Mô tả chi tiết
- TrangThaiXuLy: int?           // 0=Chưa, 1=Đang, 2=Đã
- TrangThaiXuLyText: string?    // Text hiển thị
- LinkVideo: string?            // Link video camera
- LinkHinhAnh: string?          // Link hình ảnh
```

### AnomalyStatisticsViewModel
```csharp
- TongSuCo: int                 // Tổng số sự cố
- ChuaXuLy: int                 // Số chưa xử lý
- DangXuLy: int                 // Số đang xử lý
- DaXuLy: int                   // Số đã xử lý
- KhanCap: int                  // Số khẩn cấp
- ThoiGianXuLyTrungBinh: double // Thời gian TB (phút)
```

## Database Schema

### Bảng SuCo
```sql
MaSuCo (PK)              - int
ThoiGianGhiNhan          - datetime
MaNhanVien (FK)          - int (nullable)
LoaiSuCo                 - nvarchar(50)
MaThe                    - varchar(20)
MaViTri                  - int
MoTaChiTiet              - nvarchar(500)
TrangThaiXuLy            - int (0, 1, 2)
```

## Quy trình xử lý sự cố

### 1. Phát hiện sự cố
- Hệ thống tự động ghi nhận hoặc nhân viên nhập thủ công
- Phân loại sự cố theo các tiêu chí
- Ghi lại thời gian, vị trí, thông tin xe

### 2. Cảnh báo
- Hiển thị trên màn hình giám sát
- Phát âm thanh cảnh báo (nếu được bật)
- Gửi thông báo đến các màn hình liên quan

### 3. Điều nhân viên
- Admin/Quản lý chọn nhân viên phù hợp
- Cập nhật trạng thái: Đang xử lý
- Ghi nhận thời gian bắt đầu xử lý

### 4. Xử lý
- Nhân viên đến hiện trường
- Thực hiện các biện pháp xử lý
- Ghi chú quá trình xử lý
- Tải lên bằng chứng (video/hình ảnh)

### 5. Hoàn thành
- Cập nhật trạng thái: Đã xử lý
- Ghi nhận thời gian hoàn thành
- Lưu trữ toàn bộ thông tin vào database

## Tính năng nâng cao

### Hệ thống cảnh báo
- **Realtime notification**: Cập nhật tự động mỗi 5 giây
- **Sound alert**: Âm thanh cảnh báo khi có sự cố mới
- **Visual indicator**: Badge LIVE nhấp nháy, hiệu ứng pulse

### Lưu giữ bằng chứng
- **Video camera**: Tích hợp với hệ thống camera bãi xe
- **Hình ảnh**: Chụp ảnh hiện trường
- **Timeline**: Ghi lại toàn bộ quá trình xử lý

### Quản lý từ xa
- **Màn hình giám sát**: Theo dõi tất cả sự cố từ xa
- **Bản đồ nhiệt**: Hiển thị vị trí trên sơ đồ
- **Điều động**: Gán nhân viên từ xa

### Thống kê & Báo cáo
- **Theo thời gian**: Biểu đồ xu hướng
- **Theo loại**: Phân tích tần suất
- **Hiệu suất**: Thời gian xử lý trung bình
- **Xuất Excel**: Export dữ liệu để phân tích

## Responsive Design
- Mobile-friendly interface
- Touch-optimized controls
- Adaptive layout cho mọi kích thước màn hình

## Performance
- AJAX loading để tránh reload page
- Lazy loading cho video/hình ảnh
- Caching thông tin nhân viên
- Debounce cho search/filter

## Security
- Authentication required (Area="Admin")
- Role-based access control
- Input validation
- SQL injection prevention

## Browser Support
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Dependencies
- ASP.NET Core 8.0
- Entity Framework Core
- Bootstrap 5
- Font Awesome 6
- jQuery 3.6+

## Future Enhancements
- [ ] Tích hợp AI phát hiện sự cố tự động
- [ ] Push notification mobile app
- [ ] Chatbot hỗ trợ xử lý
- [ ] Báo cáo tự động gửi email
- [ ] Integration với third-party services
- [ ] Machine learning để dự đoán sự cố

## Liên hệ
Để biết thêm thông tin, vui lòng liên hệ team phát triển.
