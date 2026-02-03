# Hướng dẫn sử dụng Module Quản lý Sự cố

## 🚀 Tính năng mới

### 1. Tạo Dữ liệu Mẫu
**Nút:** `Tạo dữ liệu mẫu` (màu xanh lá)

**Chức năng:** 
- Tạo tự động 3 sự cố mẫu để test hệ thống
- Các sự cố mẫu bao gồm:
  1. **Khẩn cấp**: Barrier bị kẹt ở cổng vào
  2. **Xe mất thẻ**: Khách hàng báo mất thẻ gửi xe
  3. **Lỗi camera**: Camera khu vực A3 mất tín hiệu

**Cách sử dụng:**
1. Click nút "Tạo dữ liệu mẫu"
2. Xác nhận trong popup
3. Trang sẽ tự động reload với 3 sự cố mới

### 2. Giao Sự cố cho Nhân viên (Single)
**Nút:** Icon `👤+` trong cột Thao tác

**Chức năng:** 
- Giao một sự cố cụ thể cho nhân viên
- Tự động cập nhật trạng thái thành "Đang xử lý"

**Cách sử dụng:**
1. Click icon nhân viên (👤+) trên dòng sự cố
2. Chọn nhân viên từ dropdown
3. Nhập ghi chú (tùy chọn)
4. Click "Xác nhận"

### 3. Giao Nhiều Sự cố (Bulk Assign) ⭐ MỚI
**Nút:** `Giao sự cố` (màu vàng, header)

**Chức năng:**
- Giao nhiều sự cố cùng lúc cho một nhân viên
- Tiết kiệm thời gian khi có nhiều sự cố cần xử lý
- Hiển thị preview tất cả sự cố được chọn

**Cách sử dụng:**

#### Bước 1: Chọn sự cố
- **Cách 1:** Click vào checkbox đầu mỗi dòng sự cố
- **Cách 2:** Click nút "Chọn tất cả" để chọn tất cả sự cố
- **Cách 3:** Click vào checkbox ở header bảng

#### Bước 2: Mở modal giao sự cố
- Click nút "Giao sự cố" màu vàng ở góc trên phải
- Hệ thống sẽ kiểm tra: phải chọn ít nhất 1 sự cố

#### Bước 3: Chọn nhân viên
- Dropdown hiển thị tất cả nhân viên đang làm việc
- Format: `Tên nhân viên - Số điện thoại`
- Chọn nhân viên phù hợp để xử lý

#### Bước 4: Xem preview
- Danh sách sự cố được chọn hiển thị với:
  - Mã sự cố
  - Loại sự cố (badge màu)
  - Mô tả ngắn gọn
- Scroll để xem tất cả nếu có nhiều sự cố

#### Bước 5: Xác nhận
- Nhập ghi chú chung (tùy chọn)
- Click "Xác nhận giao"
- Hệ thống sẽ:
  - Gán nhân viên cho tất cả sự cố
  - Cập nhật trạng thái thành "Đang xử lý"
  - Reload trang để hiển thị kết quả

## 📊 Bộ đếm sự cố đã chọn
- Hiển thị ở góc trên bên trái bảng
- Format: "Đã chọn: X"
- Màu xanh lá, nổi bật
- Tự động cập nhật khi check/uncheck

## 🎨 Giao diện

### Checkbox
- Cột mới ở đầu bảng
- Checkbox to, dễ click (18x18px)
- Header có checkbox "Chọn tất cả"

### Modal Giao nhiều sự cố
- Header màu vàng nổi bật
- Alert info hiển thị số lượng đã chọn
- Danh sách sự cố với scroll đẹp mắt
- Animation hover khi di chuột
- Responsive, hoạt động tốt trên mobile

## 🔄 Workflow

```
1. Xem danh sách sự cố
   ↓
2. Chọn nhiều sự cố (checkbox)
   ↓
3. Click "Giao sự cố"
   ↓
4. Chọn nhân viên
   ↓
5. Xem preview & xác nhận
   ↓
6. Hệ thống gán tự động
   ↓
7. Trang reload với dữ liệu mới
```

## ⚡ Tips & Tricks

### Tạo dữ liệu mẫu nhanh
```
1. Vào trang quản lý sự cố
2. Click "Tạo dữ liệu mẫu"
3. Có ngay 3 sự cố để test
```

### Giao sự cố nhanh
```
1. Click "Chọn tất cả"
2. Click "Giao sự cố"
3. Chọn nhân viên
4. Xác nhận
→ Tất cả sự cố được giao trong vài giây!
```

### Chọn có chọn lọc
```
1. Uncheck các sự cố đã xử lý
2. Chỉ chọn sự cố khẩn cấp
3. Giao cho nhân viên có kinh nghiệm
```

## 🔧 Troubleshooting

### Không thấy nút "Tạo dữ liệu mẫu"?
- Kiểm tra đã login với quyền Admin
- Refresh trang (F5)

### Không chọn được checkbox?
- Kiểm tra JavaScript đã load
- Xem Console (F12) có lỗi không

### Không thấy nhân viên trong dropdown?
- Kiểm tra có nhân viên nào đang làm việc
- Trạng thái nhân viên phải là "Active"

### Giao sự cố không thành công?
- Kiểm tra kết nối database
- Xem logs trong Console
- Đảm bảo sự cố chưa bị xóa

## 📱 Mobile Support
- Responsive trên tất cả thiết bị
- Checkbox to, dễ chạm
- Modal full-screen trên mobile
- Scroll mượt mà

## 🎯 Use Cases

### Case 1: Giao tất cả sự cố cho nhân viên trực ca
```
Scenario: Đầu ca, có 5 sự cố chưa xử lý
→ Chọn tất cả 5 sự cố
→ Giao cho nhân viên trực ca
→ Done trong 10 giây!
```

### Case 2: Giao sự cố khẩn cấp
```
Scenario: 3 sự cố khẩn cấp cần xử lý ngay
→ Filter loại "Khẩn cấp"
→ Chọn tất cả
→ Giao cho nhân viên có kinh nghiệm
→ Priority được đảm bảo!
```

### Case 3: Phân công theo khu vực
```
Scenario: Nhân viên A phụ trách khu vực 1-10
→ Filter theo vị trí
→ Chọn sự cố trong khu vực
→ Giao cho nhân viên A
→ Hiệu quả tối ưu!
```

## 🚀 Performance
- Load danh sách nhân viên: < 100ms
- Giao nhiều sự cố: < 500ms
- Auto refresh: 30s
- Optimized queries

## 🔐 Security
- Chỉ Admin mới được giao sự cố
- Validate input trên server
- Prevent SQL injection
- CSRF protection

## 📞 Support
Nếu gặp vấn đề, liên hệ team phát triển.

---
**Version:** 1.0.0  
**Last Updated:** 2025-01-23  
**Author:** Parking Management System Team
