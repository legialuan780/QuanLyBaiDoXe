# Module Quản Lý Ca Làm Việc (Vehicle Shift Management)

## Tổng quan
Module quản lý ca làm việc và nhân viên cho hệ thống quản lý bãi đỗ xe, bao gồm các chức năng:
- Quản lý danh sách ca làm việc
- Lịch làm việc tuần/tháng
- Bảng chấm công và tính giờ công
- Danh sách nhân viên

## Cấu trúc Files

### Controller
- **VehicleShiftController.cs**: Controller chính xử lý các chức năng
  - `Index()`: Danh sách ca làm việc
  - `Schedule()`: Lịch làm việc theo tuần
  - `TimeSheet()`: Bảng chấm công theo tháng
  - `EmployeeList()`: Danh sách nhân viên
  - `GetShiftStats()`: API lấy thống kê ca
  - `GetShiftDetail()`: API lấy chi tiết ca

### ViewModels
- **VehicleShiftViewModel.cs**: Chứa các ViewModel
  - `ShiftViewModel`: Thông tin ca làm việc
  - `ScheduleViewModel`: Lịch làm việc
  - `EmployeeTimeSheetViewModel`: Bảng chấm công
  - `EmployeeViewModel`: Thông tin nhân viên

### Views
1. **Index.cshtml**: Trang danh sách ca làm việc
   - Thống kê tổng quan (tổng ca, ca đang làm, đã hoàn thành, doanh thu)
   - Bảng danh sách ca với các thông tin chi tiết
   - Lọc theo ngày và trạng thái
   - Xem chi tiết ca, kết thúc ca, in báo cáo

2. **Schedule.cshtml**: Trang lịch làm việc
   - Xem lịch làm việc theo tuần
   - Hiển thị dạng lưới (grid) với nhân viên và 7 ngày trong tuần
   - Phân biệt 3 ca: Ca sáng, Ca chiều, Ca đêm
   - Thêm/xóa lịch làm việc
   - Xuất Excel và in lịch

3. **TimeSheet.cshtml**: Trang bảng chấm công
   - Thống kê theo tháng/năm
   - Tính tổng giờ làm, tăng ca
   - Doanh thu và hiệu suất từng nhân viên
   - Biểu đồ hiệu suất
   - Xuất bảng công Excel

4. **EmployeeList.cshtml**: Trang danh sách nhân viên
   - Hiển thị danh sách nhân viên dạng card
   - Thông tin: Họ tên, chức vụ, tuổi, ngày vào làm, trạng thái
   - Lọc theo chức vụ và trạng thái
   - Tìm kiếm theo tên, số điện thoại
   - Xem chi tiết và lịch làm việc nhân viên

### CSS
- **vehicle-shift.css**: Style riêng cho module
  - Employee avatar
  - Schedule grid với 3 loại ca (màu sắc khác nhau)
  - Employee cards
  - TimeSheet table
  - Responsive design

## Các chức năng chính

### 1. Quản lý Ca Làm Việc
- **Mở ca mới**: Tạo ca làm việc mới cho nhân viên
- **Theo dõi ca**: Xem trạng thái ca (Chưa bắt đầu, Đang làm việc, Đã kết thúc)
- **Kết thúc ca**: Đóng ca và tính toán doanh thu
- **Chi tiết ca**: Xem số xe vào/ra, tiền đầu ca, tổng thu, tiền cuối ca

### 2. Lịch Làm Việc
- **Xem theo tuần**: Hiển thị lịch 7 ngày
- **Phân chia 3 ca**:
  - Ca 1 - Ca sáng (6h-14h): Màu vàng
  - Ca 2 - Ca chiều (14h-22h): Màu xanh lá
  - Ca 3 - Ca đêm (22h-6h): Màu xám
- **Quản lý lịch**: Thêm, sửa, xóa lịch làm việc
- **Điều hướng**: Chuyển tuần trước/sau, chọn ngày cụ thể

### 3. Bảng Chấm Công
- **Tính giờ công**: Tự động tính tổng giờ làm từ các ca
- **Giờ chuẩn vs Tăng ca**: Phân biệt giờ làm chuẩn (8h/ca) và tăng ca
- **Hiệu suất**: Tính doanh thu trung bình/ca
- **Biểu đồ**: Trực quan hóa hiệu suất nhân viên
- **Xuất báo cáo**: Export Excel theo tháng

### 4. Quản Lý Nhân Viên
- **Danh sách nhân viên**: Hiển thị dạng card với đầy đủ thông tin
- **Chức vụ**: Bảo vệ, Thu ngân, Giám sát, Quản lý
- **Trạng thái**: Đang làm việc / Đã nghỉ việc
- **Lọc và tìm kiếm**: Tìm nhanh theo nhiều tiêu chí
- **Xem lịch**: Truy cập nhanh lịch làm việc của nhân viên

## Database

Module sử dụng các bảng sau:
- **CaLamViec**: Lưu thông tin ca làm việc
- **LichLamViec**: Lưu lịch làm việc
- **NhanVien**: Thông tin nhân viên
- **LuotGui**: Liên kết với lượt xe vào/ra

## Cách sử dụng

### 1. Truy cập module
- Từ sidebar Admin, click "Ca làm việc"
- Hoặc truy cập trực tiếp:
  - `/Admin/VehicleShift/Index` - Danh sách ca
  - `/Admin/VehicleShift/Schedule` - Lịch làm việc
  - `/Admin/VehicleShift/TimeSheet` - Bảng chấm công
  - `/Admin/VehicleShift/EmployeeList` - Danh sách nhân viên

### 2. Quy trình quản lý ca
1. **Tạo lịch làm việc**: Vào Schedule → Thêm lịch cho nhân viên
2. **Mở ca**: Nhân viên nhận ca tại Index → Mở ca mới
3. **Theo dõi**: Xem trạng thái ca đang làm việc
4. **Kết thúc ca**: Giao ca và đối soát tiền
5. **Chấm công**: Xem tổng hợp tại TimeSheet

### 3. Tính năng nâng cao
- **API endpoints**: Có thể tích hợp với mobile app
- **Real-time**: Có thể mở rộng với SignalR để cập nhật real-time
- **Export**: Xuất Excel cho báo cáo
- **Print**: In lịch và bảng công

## Tính năng mở rộng (có thể phát triển thêm)

1. **Giao/Nhận ca tự động**
   - QR code check-in/check-out
   - GPS tracking để xác nhận vị trí

2. **Thông báo**
   - Nhắc nhở nhân viên về ca làm việc sắp tới
   - Cảnh báo ca chưa có người

3. **Quản lý nghỉ phép**
   - Đăng ký nghỉ phép
   - Duyệt nghỉ phép
   - Tính lương theo nghỉ phép

4. **Đánh giá hiệu suất**
   - KPI theo ca làm việc
   - Xếp hạng nhân viên
   - Bonus/Penalty

5. **Tích hợp lương**
   - Tính lương theo giờ công
   - Tính tăng ca
   - Phụ cấp theo ca (đêm, lễ)

6. **Dashboard analytics**
   - Biểu đồ xu hướng
   - So sánh theo kỳ
   - Dự báo nhân lực

## Responsive Design
- ✅ Desktop: Hiển thị đầy đủ tính năng
- ✅ Tablet: Grid layout tối ưu
- ✅ Mobile: Stack layout, menu responsive

## Browser Support
- Chrome (recommended)
- Firefox
- Edge
- Safari

## Dependencies
- Bootstrap 5
- Font Awesome 6
- jQuery
- Chart.js (cho biểu đồ)

## Ghi chú
- Module này tương thích với .NET 8 và C# 12
- Sử dụng Entity Framework Core
- Tuân theo pattern MVC của ASP.NET Core
