# Giao di?n L?ch Ca Làm Vi?c (Shift Calendar)

## ?? T?ng quan
?ã t?o giao di?n **Cu?n l?ch màu** (Monthly Calendar) ?? xem các ca làm vi?c th?c t? t? b?ng `CaLamViec`. ?ây là m?t calendar view ??p m?t v?i màu s?c phân bi?t tr?ng thái ca.

## ? Tính n?ng chính

### 1. **Monthly Calendar View**
- Hi?n th? l?ch tháng ??y ?? (7 c?t x 5-6 hàng)
- M?i ngày hi?n th? t?i ?a 3 ca + "xem thêm"
- Màu s?c phân bi?t tr?ng thái:
  - **Xanh lá**: Ca ?ang tr?c (TrangThaiCa = 0)
  - **Xanh d??ng**: Ca ?ã ch?t (TrangThaiCa = 1)
  - **Cam**: Nhi?u ca trong ngày
  - **Vàng**: Ngày hôm nay

### 2. **?i?u h??ng tháng**
- **Tháng tr??c / Tháng sau**: Nút m?i tên
- **Dropdown**: Ch?n tháng & n?m tr?c ti?p
- **Nút "Hôm nay"**: Nh?y v? tháng hi?n t?i
- **Keyboard shortcuts**: 
  - ? ? ?? chuy?n tháng
  - T ?? v? hôm nay

### 3. **Th?ng kê tháng**
- **Ca ?ang tr?c**: S? ca hi?n ?ang ho?t ??ng
- **Ca ?ã ch?t**: T?ng ca ?ã hoàn thành
- **T?ng gi? làm**: T?ng s? gi? làm vi?c
- **T?ng doanh thu**: Doanh thu t? `TongTienHeThong`

### 4. **Chi ti?t m?i ngày**
Click vào ngày ? Modal hi?n th?:
- Th?ng kê ngày (T?ng ca, ?ang tr?c, ?ã ch?t, Doanh thu)
- B?ng chi ti?t t?t c? ca trong ngày:
  - Mã ca, Nhân viên, Gi? nh?n/giao ca
  - S? gi? làm, Doanh thu, Tr?ng thái
  - Nút "Xem chi ti?t" ? M? trang ca làm vi?c

### 5. **UI/UX ??p m?t**
- **Gradient colors** cho các tr?ng thái
- **Hover effects** khi di chu?t
- **Badge indicators** cho s? l??ng ca
- **Empty state** cho ngày không có ca
- **Today highlight** v?i border vàng
- **Responsive** cho mobile/tablet

## ?? D? li?u s? d?ng

### B?ng: `CaLamViec`
```sql
- MaCa: Mã ca làm vi?c
- MaNhanVien: Nhân viên tr?c ca
- ThoiGianNhanCa: Th?i gian b?t ??u ca
- ThoiGianGiaoCa: Th?i gian k?t thúc ca
- TongTienHeThong: Doanh thu ca
- TrangThaiCa: 0 = ?ang tr?c, 1 = ?ã ch?t
```

## ?? Files ?ã t?o/s?a

### Files m?i:
1. **ShiftCalendar.cshtml**
   - View calendar tháng
   - Modal chi ti?t ngày
   - JavaScript ?i?u h??ng

### Files ?ã s?a:
1. **VehicleShiftController.cs**
   - Thêm `ShiftCalendar()` action
   - Thêm `GetShiftsForDate()` API

2. **VehicleShiftViewModel.cs**
   - Thêm `ShiftCalendarViewModel`
   - Thêm `MonthStatsViewModel`

3. **Index.cshtml**
   - C?p nh?t nút "L?ch làm vi?c" ? `ShiftCalendar`

4. **vehicle-shift.css**
   - Thêm 500+ dòng CSS cho calendar
   - Responsive styles

## ?? Màu s?c & Design

### Tr?ng thái ca:
```css
Ca ?ang tr?c: 
  background: linear-gradient(135deg, #28a745, #20c997)
  
Ca ?ã ch?t:
  background: linear-gradient(135deg, #17a2b8, #138496)
  
Nhi?u ca:
  background: rgba(253, 126, 20, 0.1)
  border-left: 3px solid #fd7e14
  
Hôm nay:
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.1), ...)
  border: 2px solid #ffc107
```

### Layout:
- **Calendar grid**: 7 columns (days of week)
- **Min height per day**: 120px (desktop), 60px (mobile)
- **Gap**: 1px v?i background màu border
- **Hover**: Highlight v?i box-shadow primary color

## ?? Cách s? d?ng

### 1. Truy c?p:
- Vào **Ca làm vi?c** ? Click **L?ch làm vi?c**
- URL: `/Admin/VehicleShift/ShiftCalendar`

### 2. Xem l?ch tháng:
- Xem các ca ???c hi?n th? theo ngày
- Màu xanh lá = ?ang tr?c
- Màu xanh d??ng = ?ã ch?t
- Badge s? l??ng ? cu?i m?i ngày

### 3. Chuy?n tháng:
- Click **Tháng tr??c/sau**
- Ho?c ch?n trong dropdown
- Ho?c dùng phím ? ?

### 4. Xem chi ti?t ngày:
- Click vào b?t k? ngày nào
- Modal hi?n th? t?t c? ca trong ngày
- B?ng chi ti?t v?i ??y ?? thông tin

### 5. Xem chi ti?t ca:
- Trong modal ? Click icon "Xem chi ti?t"
- M? tab m?i v?i trang qu?n lý ca

## ?? API Endpoints

### 1. GET `/Admin/VehicleShift/ShiftCalendar`
**Parameters**:
- `month` (int, optional) - Tháng c?n xem
- `year` (int, optional) - N?m c?n xem

**Returns**: View v?i `ShiftCalendarViewModel`
```csharp
{
  SelectedMonth: 12,
  SelectedYear: 2024,
  ShiftsByDate: Dictionary<DateTime, List<ShiftViewModel>>,
  MonthStats: {
    TotalActiveShifts: 15,
    TotalCompletedShifts: 45,
    TotalWorkHours: 480,
    TotalRevenue: 12500000
  }
}
```

### 2. GET `/Admin/VehicleShift/GetShiftsForDate`
**Parameters**:
- `date` (string) - Ngày c?n xem, format: "yyyy-MM-dd"

**Returns**: JSON
```json
{
  "success": true,
  "shifts": [
    {
      "maCa": 123,
      "tenNhanVien": "Nguy?n V?n A",
      "thoiGianNhanCa": "2024-01-15T06:00:00",
      "thoiGianGiaoCa": "2024-01-15T14:00:00",
      "trangThaiCa": 1,
      "tongTienHeThong": 1200000,
      "soGioLam": 8.0
    }
  ],
  "activeCount": 2,
  "completedCount": 3,
  "totalRevenue": 6500000
}
```

## ?? Use Cases

### UC1: Xem t?ng quan ca làm vi?c tháng
1. Manager vào **L?ch làm vi?c**
2. Nhìn t?ng quan toàn b? tháng
3. Th?y ngày nào có nhi?u ca (màu cam)
4. Th?y ca nào ?ang tr?c (màu xanh lá)

### UC2: Ki?m tra ca trong ngày c? th?
1. Click vào ngày c?n xem
2. Modal hi?n th? chi ti?t
3. Xem danh sách ca và tr?ng thái
4. Click xem chi ti?t t?ng ca n?u c?n

### UC3: Theo dõi th?ng kê tháng
1. Xem 4 stats ? ??u trang
2. So sánh v?i các tháng khác
3. ?ánh giá hi?u su?t làm vi?c

### UC4: ?i?u h??ng nhanh
1. Dùng phím ? ? ?? xem tháng tr??c/sau
2. Dùng phím T ?? v? hôm nay
3. Dropdown ?? nh?y ??n tháng b?t k?

## ?? Responsive Design

### Desktop (>1024px):
- Calendar ??y ?? 7x6
- Hi?n th? 3 ca + more
- Min height 120px/ngày

### Tablet (768px - 1024px):
- Calendar 7x6 nh? h?n
- Hi?n th? 2 ca + more
- Min height 100px/ngày

### Mobile (<768px):
- Calendar 7x5/6 compact
- Ch? hi?n th? icon & s? ca
- ?n tên nhân viên
- Min height 80px/ngày

### Extra Small (<480px):
- Calendar t?i gi?n
- Ch? badge s? l??ng
- Min height 60px/ngày
- Stats grid 2 c?t

## ? Performance

### T?i ?u:
- Load m?t tháng m?i l?n
- Dictionary lookup O(1) cho ShiftsByDate
- Lazy load modal content
- CSS animations dùng transform

### Caching:
- Browser cache cho CSS/JS
- Server có th? cache stats tháng

## ?? Highlights

### 1. **Visual indicators**:
- Border trái màu theo tr?ng thái
- Badge count ? footer cell
- Today highlight rõ ràng
- Empty state thân thi?n

### 2. **Interactions**:
- Click ngày ? Modal detail
- Hover cell ? Highlight
- Keyboard navigation
- Smooth transitions

### 3. **Information density**:
- V?a ?? thông tin trên m?i ngày
- Không quá t?i
- "+X ca khác" cho days có nhi?u ca
- Modal cho chi ti?t ??y ??

## ?? Tính n?ng có th? m? r?ng

- [ ] Drag & drop ?? chuy?n ca
- [ ] Filter theo nhân viên/tr?ng thái
- [ ] Export calendar ra PDF/Excel
- [ ] Print-friendly view
- [ ] Week view (7 ngày cu?n)
- [ ] Year view (12 tháng overview)
- [ ] Notifications trên calendar
- [ ] Color themes
- [ ] Custom views (2 tu?n, 3 tu?n, etc.)
- [ ] Quick add ca t? calendar

## ? Testing

- ? Build successful
- ? Calendar hi?n th? ?úng
- ? ?i?u h??ng tháng ho?t ??ng
- ? Click ngày ? Modal OK
- ? Stats tính toán chính xác
- ? Màu s?c hi?n th? ?úng
- ? Responsive mobile/tablet
- ? Keyboard shortcuts ho?t ??ng
- ? API endpoints tr? v? ?úng

## ?? So sánh v?i các view khác

| Feature | ShiftCalendar | DailySchedule | WeeklySchedule |
|---------|---------------|---------------|----------------|
| View type | Monthly | Daily Timeline | Weekly Grid |
| Data source | CaLamViec | LichLamViec + CaLamViec | LichLamViec |
| Best for | T?ng quan tháng | Chi ti?t ngày | L?p l?ch tu?n |
| Time range | 1 tháng | 1 ngày | 1 tu?n |
| Edit ability | ? View only | ? View only | ? Can edit |

## ?? K?t lu?n

**Shift Calendar** là công c? hoàn h?o ??:
- ? Xem t?ng quan ca làm vi?c theo tháng
- ? Theo dõi tr?ng thái ca (?ang tr?c/?ã ch?t)
- ? Ki?m tra nhanh ca trong ngày c? th?
- ? Th?ng kê hi?u su?t tháng
- ? Giao di?n ??p, màu s?c tr?c quan
- ? Responsive cho m?i thi?t b?

?ây là giao di?n **"cu?n l?ch màu"** nh? yêu c?u, s? d?ng 100% d? li?u t? b?ng `CaLamViec`!
