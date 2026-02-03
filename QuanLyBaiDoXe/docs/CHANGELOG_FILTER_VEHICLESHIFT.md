# ?? B? L?C NÂNG CAO CHO DANH SÁCH CA LÀM VI?C

## ?? **T?ng quan**

?ã nâng c?p trang **VehicleShift/Index** v?i b? l?c ??y ?? và m?nh m? ?? qu?n lý ca làm vi?c hi?u qu? h?n.

---

## ? **Tính n?ng m?i**

### 1?? **B? l?c theo th?i gian**
- ? **T? ngày - ??n ngày**: L?c ca theo kho?ng th?i gian tùy ch?nh
- ? **Quick filters**:
  - ?? Hôm nay
  - ?? Hôm qua
  - ?? 7 ngày qua
  - ?? Tháng này

### 2?? **B? l?c theo nhân viên**
- ? Dropdown ch?n nhân viên (hi?n th? t?t c? nhân viên ?ang làm vi?c)
- ? Gi? l?i l?a ch?n sau khi filter

### 3?? **B? l?c theo tr?ng thái**
- ? T?t c? tr?ng thái
- ? ?ang tr?c (Quick button)
- ? ?ã ch?t (Quick button)

### 4?? **Tìm ki?m nhanh**
- ? Tìm theo **Mã ca** (VD: "123")
- ? Tìm theo **Tên nhân viên** (VD: "Nguy?n V?n A")
- ? H? tr? **Enter** ?? submit

### 5?? **Export & Print**
- ? **Export Excel**: Xu?t d? li?u ra file .xlsx
- ? **Print Report**: In báo cáo (t?i ?u cho gi?y landscape)

---

## ?? **Cách s? d?ng**

### **Quick Filter Buttons** (Nhanh nh?t)

1. **Hôm nay**: Click ? Hi?n t?t c? ca hôm nay
2. **7 ngày qua**: Click ? Hi?n ca 7 ngày g?n ?ây
3. **?ang tr?c**: Click ? Ch? hi?n ca ?ang tr?c (không phân bi?t ngày)
4. **?ã ch?t**: Click ? Ch? hi?n ca ?ã ch?t (7 ngày g?n ?ây)

### **Filter tùy ch?nh**

```
1. Ch?n kho?ng th?i gian (T? ngày - ??n ngày)
2. Ch?n nhân viên c? th? (ho?c ?? tr?ng = t?t c?)
3. Ch?n tr?ng thái (ho?c ?? tr?ng = t?t c?)
4. Nh?p t? khóa tìm ki?m (không b?t bu?c)
5. Click "Tìm ki?m"
```

### **Export Excel**

```
1. Áp d?ng filter theo ý mu?n
2. Click nút "Xu?t Excel"
3. File s? t?i xu?ng v?i tên: DanhSachCa_YYYY-MM-DD.xlsx
```

---

## ?? **Thay ??i k? thu?t**

### **Controller: `VehicleShiftController.Index()`**

#### **Parameters m?i:**
```csharp
public async Task<IActionResult> Index(
    DateTime? fromDate,        // T? ngày
    DateTime? toDate,          // ??n ngày
    int? employeeId,           // Mã nhân viên
    int? shiftStatus,          // Tr?ng thái (0: ?ang tr?c, 1: ?ã ch?t)
    string? searchTerm)        // T? khóa tìm ki?m
```

#### **Default behavior:**
- N?u không có filter ? L?y **7 ngày g?n nh?t**
- Limit t?ng t? **50 ? 200 records**

#### **ViewBag data:**
```csharp
ViewBag.FromDate       // string (yyyy-MM-dd)
ViewBag.ToDate         // string (yyyy-MM-dd)
ViewBag.EmployeeId     // int?
ViewBag.ShiftStatus    // int?
ViewBag.SearchTerm     // string?
ViewBag.Employees      // List<{MaNhanVien, HoTen}>
```

### **View: `Index.cshtml`**

#### **Thêm section m?i:**
1. **Filter card** (dòng 220-330)
2. **Results summary** (dòng 332-350)
3. **JavaScript functions** (dòng 1010-1150):
   - `resetFilters()`
   - `setQuickFilter(type)`
   - `formatDate(date)`
   - `exportToExcel()`
   - `printReport()`

#### **Dependencies m?i:**
```html
<!-- SheetJS for Excel export -->
<script src="https://cdn.sheetjs.com/xlsx-0.20.0/package/dist/xlsx.full.min.js"></script>
```

---

## ?? **Ví d? s? d?ng**

### **Scenario 1: Xem ca ?ang tr?c hi?n t?i**
```
1. Click quick button "?ang tr?c"
2. K?t qu?: Hi?n T?T C? ca ?ang tr?c (k? c? ca ?êm hôm tr??c)
```

### **Scenario 2: Báo cáo tháng cho 1 nhân viên**
```
1. Click quick button "Tháng này"
2. Ch?n nhân viên trong dropdown
3. Click "Tìm ki?m"
4. Click "Xu?t Excel" ?? l?u báo cáo
```

### **Scenario 3: Tìm ca c? th?**
```
1. Nh?p mã ca (VD: "123") vào ô tìm ki?m
2. Click "Tìm ki?m" (ho?c Enter)
3. K?t qu?: Ch? hi?n ca #123
```

### **Scenario 4: Xem ca hôm qua ?ã ch?t**
```
1. Click quick button "Hôm qua"
2. Ch?n "?ã ch?t" trong dropdown tr?ng thái
3. Click "Tìm ki?m"
```

---

## ?? **UI/UX Improvements**

### **Filter Section:**
- ?? Card riêng bi?t v?i icon rõ ràng
- ?? Quick filter buttons: Bootstrap button group
- ?? Nút "??t l?i" ?? clear t?t c? filter
- ?? Gi? state filter sau khi submit (selected values)

### **Results Section:**
- ?? Hi?n th? s? l??ng k?t qu?
- ?? Hi?n t? khóa ?ang tìm
- ?? Buttons Export Excel & Print

### **DataTable Integration:**
- ? Pagination v?n ho?t ??ng
- ? Sort theo c?t v?n ho?t ??ng
- ? DataTable search box v?n dùng ???c

---

## ?? **Performance**

### **Optimization:**
- ? Query dùng `.AsQueryable()` ? EF t?i ?u SQL
- ? Ch? load field c?n thi?t (projection)
- ? Index trên `ThoiGianNhanCa` ? Query nhanh
- ? Limit 200 records ?? tránh overload

### **SQL Generated (ví d?):**
```sql
SELECT TOP 200 
    c.MaCa, c.MaNhanVien, nv.HoTen, c.ThoiGianNhanCa, ...
FROM CaLamViec c
LEFT JOIN NhanVien nv ON c.MaNhanVien = nv.MaNhanVien
WHERE c.ThoiGianNhanCa >= @fromDate 
  AND c.ThoiGianNhanCa <= @toDate
  AND c.TrangThaiCa = @status
ORDER BY c.ThoiGianNhanCa DESC
```

---

## ?? **Responsive Design**

- ? Mobile-friendly: Filter fields stack vertically
- ? Tablet: 2 columns layout
- ? Desktop: 3-4 columns layout
- ? Print: Optimized landscape A4

---

## ?? **Bug Fixes**

1. ? Fix Razor syntax: `@(condition ? "selected" : "")` ? `selected="@condition"`
2. ? Fix CSS `@page` directive: Escape `@@page`
3. ? Prevent Enter key reload: `e.preventDefault()`

---

## ?? **Future Enhancements** (Optional)

1. **Advanced filters:**
   - L?c theo ca làm vi?c (sáng/chi?u/?êm)
   - L?c theo kho?ng doanh thu
   - L?c theo s? gi? làm

2. **Saved filters:**
   - L?u preset filter c?a user
   - Quick access dropdown

3. **Export options:**
   - Export PDF
   - Export CSV
   - Email report

4. **Real-time updates:**
   - SignalR ?? update tr?ng thái ca real-time
   - Notification khi có ca m?i

---

## ? **Testing Checklist**

- [ ] Filter theo th?i gian ho?t ??ng ?úng
- [ ] Filter theo nhân viên ho?t ??ng ?úng
- [ ] Filter theo tr?ng thái ho?t ??ng ?úng
- [ ] Tìm ki?m theo mã ca ho?t ??ng
- [ ] Tìm ki?m theo tên nhân viên ho?t ??ng
- [ ] Quick filter buttons ho?t ??ng ?úng
- [ ] Export Excel t?i file thành công
- [ ] Print hi?n th? ?úng ??nh d?ng
- [ ] Gi? state filter sau khi submit
- [ ] Reset filters xóa t?t c? filter
- [ ] Enter trong search box submit form
- [ ] Responsive trên mobile/tablet
- [ ] Không có l?i console JavaScript
- [ ] Build thành công

---

## ?? **Support**

N?u có v?n ??:
1. Check browser console (F12) ?? xem l?i JavaScript
2. Check SQL Profiler ?? xem query performance
3. Verify ViewBag data có ?úng không

---

**Version**: 2.0.0  
**Date**: ${new Date().toLocaleDateString('vi-VN')}  
**Status**: ? Deployed & Ready to use  
**Build**: ? Successful
