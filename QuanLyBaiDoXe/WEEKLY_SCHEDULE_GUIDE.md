# Giao di?n L?p l?ch làm vi?c tu?n

## T?ng quan
?ã thêm ch?c n?ng **L?p l?ch làm vi?c tu?n** - M?t công c? tr?c quan ?? qu?n lý và phân công ca làm vi?c cho nhân viên theo tu?n.

## ?? Ch?c n?ng chính

### 1. **B?ng l?ch tu?n (Weekly Schedule Grid)**
- Hi?n th? l?ch làm vi?c d?ng b?ng v?i:
  - **C?t d?c**: Danh sách nhân viên (Qu?n lý & B?o v?)
  - **C?t ngang**: 7 ngày trong tu?n (Th? 2 ? Ch? nh?t)
  - **Ô**: Ca làm vi?c ???c phân công

### 2. **Phân ca linh ho?t**
#### Các lo?i ca:
- **Ca sáng** (6h - 14h) - Màu vàng/cam
- **Ca chi?u** (14h - 22h) - Màu xanh lá
- **Ca ?êm** (22h - 6h) - Màu xám/?en

#### Cách phân ca:
1. Click vào ô tr?ng (ngày + nhân viên)
2. Modal hi?n th? 3 l?a ch?n ca
3. Ch?n ca và nh?p ghi chú (tùy ch?n)
4. Ca ???c l?u t? ??ng

### 3. **?i?u h??ng tu?n**
- Nút **Tu?n tr??c** / **Tu?n sau**
- Ch?n ngày b?t k? ?? nh?y ??n tu?n ?ó
- Hi?n th? rõ ràng: "Tu?n t? DD/MM/YYYY ??n DD/MM/YYYY"

### 4. **Th?ng kê real-time**
- **T?ng nhân viên**: S? nhân viên ?ang ho?t ??ng
- **Ca ?ã x?p**: T?ng s? ca ?ã phân công trong tu?n
- **T?ng gi?**: T?ng s? gi? làm vi?c (m?i ca = 8h)
- **Nhân viên ???c phân**: S? nhân viên ?ã ???c giao ca

### 5. **Công c? t? ??ng**
#### T? ??ng phân ca:
- Phân ca ??u cho t?t c? nhân viên
- M?i ngày có 2 ca (sáng + chi?u)
- Luân phiên công b?ng

#### Xóa t?t c?:
- Xóa toàn b? l?ch tu?n hi?n t?i
- Có xác nh?n tr??c khi xóa

### 6. **Ch?nh s?a & xóa ca**
- Click vào ca ?ã phân ?? ch?nh s?a
- Nút "X" trên badge ca ?? xóa nhanh
- Có xác nh?n tr??c khi xóa

## ?? Files ?ã thêm/s?a

### Files m?i:
1. **QuanLyBaiDoXe\Areas\Admin\Views\VehicleShift\WeeklySchedule.cshtml**
   - View chính cho giao di?n l?p l?ch tu?n
   - Modal ch?n ca làm vi?c
   - JavaScript x? lý t??ng tác

### Files ?ã s?a:
1. **QuanLyBaiDoXe\Areas\Admin\Views\VehicleShift\Index.cshtml**
   - Thêm nút "L?p l?ch tu?n" vào header

2. **QuanLyBaiDoXe\Areas\Admin\Controllers\VehicleShiftController.cs**
   - Thêm action `WeeklySchedule()` - Hi?n th? giao di?n
   - Thêm API `SaveShiftSchedule()` - L?u ca làm vi?c
   - Thêm API `DeleteShiftSchedule()` - Xóa ca
   - Thêm API `ClearWeekSchedule()` - Xóa toàn b? l?ch tu?n
   - Thêm API `AutoAssignWeek()` - T? ??ng phân ca
   - Thêm Request model `SaveScheduleRequest`

3. **QuanLyBaiDoXe\Areas\Admin\ViewModels\VehicleShiftViewModel.cs**
   - Thêm `WeeklyScheduleViewModel` - ViewModel cho giao di?n l?p l?ch

4. **QuanLyBaiDoXe\wwwroot\css\vehicle-shift.css**
   - Thêm styles cho giao di?n l?ch tu?n
   - Styles cho b?ng, ca làm vi?c, modal

## ?? Cách s? d?ng

### Truy c?p giao di?n:
1. Vào menu **Ca làm vi?c** ? **Qu?n lý ca làm vi?c**
2. Click nút **L?p l?ch tu?n** (icon calendar-week)

### Phân ca cho nhân viên:
1. Ch?n tu?n mu?n l?p l?ch
2. Click vào ô (ngày + nhân viên)
3. Ch?n lo?i ca (Sáng/Chi?u/?êm)
4. Nh?p ghi chú n?u c?n
5. Ca ???c l?u t? ??ng

### S?a/Xóa ca:
- **S?a**: Click vào badge ca ? Ch?n ca m?i
- **Xóa**: Click vào nút "X" trên badge ca

### T? ??ng phân ca:
1. Click nút **T? ??ng phân ca**
2. H? th?ng s? phân ??u ca cho nhân viên
3. M?i ngày có 2 ca (sáng + chi?u)

### Xóa toàn b? l?ch tu?n:
1. Click nút **Xóa t?t c?**
2. Xác nh?n
3. Toàn b? ca trong tu?n s? b? xóa

## ?? Giao di?n

### Màu s?c ca làm vi?c:
- **Ca sáng**: Gradient vàng ? cam `#FFD93D ? #FFA500`
- **Ca chi?u**: Gradient xanh lá `#21A691 ? #87DF2C`
- **Ca ?êm**: Gradient xám ? ?en `#4A5568 ? #2D3748`

### Highlight:
- **Hôm nay**: N?n màu xanh nh?t
- **Hover ô**: Hi?u ?ng highlight
- **Ca ???c ch?n**: Hi?n th? badge có th? xóa

## ?? Database

### B?ng s? d?ng:
- **LichLamViec** (?ã có s?n)
  - `MaLich` (PK)
  - `MaNhanVien` (FK ? NhanVien)
  - `NgayLamViec` (DateOnly)
  - `CaLamViec` (int: 1=Sáng, 2=Chi?u, 3=?êm)
  - `GhiChu` (string, nullable)

## ?? API Endpoints

### 1. GET `/Admin/VehicleShift/WeeklySchedule`
**Parameters**: 
- `year` (int, optional)
- `month` (int, optional)
- `day` (int, optional)

**Returns**: View v?i WeeklyScheduleViewModel

### 2. POST `/Admin/VehicleShift/SaveShiftSchedule`
**Body**: SaveScheduleRequest
```json
{
  "MaLich": null,  // null = create, có giá tr? = update
  "MaNhanVien": 1,
  "NgayLamViec": "2024-01-15",
  "CaLamViec": 1,
  "GhiChu": "Ghi chú tùy ch?n"
}
```
**Returns**: JSON `{ success: bool, message: string, scheduleId: int }`

### 3. POST `/Admin/VehicleShift/DeleteShiftSchedule`
**Parameters**: `id` (int)
**Returns**: JSON `{ success: bool, message: string }`

### 4. POST `/Admin/VehicleShift/ClearWeekSchedule`
**Parameters**: `weekStart` (string, format: "yyyy-MM-dd")
**Returns**: JSON `{ success: bool, message: string }`

### 5. POST `/Admin/VehicleShift/AutoAssignWeek`
**Parameters**: `weekStart` (string, format: "yyyy-MM-dd")
**Returns**: JSON `{ success: bool, message: string }`

## ?? Responsive Design
- Giao di?n responsive cho mobile
- B?ng cu?n ngang trên màn hình nh?
- Buttons và fonts t? ??ng ?i?u ch?nh

## ?? Tính n?ng nâng cao

### T? ??ng tính tu?n:
- Tu?n b?t ??u t? **Th? 2**
- Khi ch?n ngày b?t k? ? T? ??ng tính Th? 2 c?a tu?n ?ó

### Ki?m tra trùng l?p:
- M?i nhân viên ch? có t?i ?a 1 ca/ngày
- N?u ?ã có ca ? Update thay vì t?o m?i

### L?u t? ??ng:
- Không c?n nh?n "L?u l?ch tu?n"
- M?i thao tác thêm/s?a/xóa ca ???c l?u ngay

## ? Testing
- ? Build successful
- ? T?t c? API endpoints ho?t ??ng
- ? UI responsive và thân thi?n
- ? T? ??ng phân ca ho?t ??ng ?úng
- ? Xóa và ch?nh s?a ca ho?t ??ng
- ? Th?ng kê real-time chính xác

## ?? Use Cases

### Use case 1: L?p l?ch tu?n k? ti?p
1. Manager vào **L?p l?ch tu?n**
2. Ch?n tu?n k? ti?p
3. S? d?ng **T? ??ng phân ca** ?? phân ??u
4. ?i?u ch?nh th? công n?u c?n

### Use case 2: Thay ??i ca ??t xu?t
1. Nhân viên A xin ngh?
2. Manager vào l?ch tu?n ?ó
3. Click vào ca c?a nhân viên A
4. Ch?n nhân viên khác

### Use case 3: Xem l?ch ?ã l?p
1. Vào **L?p l?ch tu?n**
2. Ch?n tu?n mu?n xem
3. Xem t?t c? ca ?ã ???c phân

## ?? Tips
- Nên l?p l?ch tr??c 1 tu?n
- S? d?ng t? ??ng phân ca làm n?n, sau ?ó ?i?u ch?nh
- Thêm ghi chú ?? nh?c nh? ??c bi?t
- Ki?m tra th?ng kê ?? ??m b?o phân công ??u

## ?? Tính n?ng có th? m? r?ng
- Copy l?ch tu?n tr??c
- Template l?ch ??nh k?
- Thông báo cho nhân viên khi có l?ch m?i
- Export l?ch tu?n ra Excel
- Xem l?ch theo tháng
- Qu?n lý ngày ngh? và ngày l?
