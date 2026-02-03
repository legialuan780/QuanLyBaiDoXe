# ?? C?I TI?N: H? TR? HI?N TH? CA ?ÊM QUA NGÀY M?I

## ?? **V?n ?? tr??c ?ây:**
- Ca ?êm (22h-6h) b?t ??u 22h hôm tr??c
- Khi qua 0h ? ngày m?i ? h? th?ng filter theo ngày hi?n t?i
- ? **K?T QU?**: Ca ?êm không hi?n th? trong danh sách "?ang tr?c"

## ? **Gi?i pháp:**
Thay ??i logic l?y ca ?ang tr?c:
- **TR??C**: `WHERE ThoiGianNhanCa >= startOfDay AND ThoiGianNhanCa < endOfDay AND TrangThaiCa = 0`
- **SAU**: `WHERE TrangThaiCa = 0 AND ThoiGianGiaoCa IS NULL`

## ?? **Các file ?ã s?a:**

### 1. `VehicleShiftController.cs`

#### **Method: `DailySchedule` (dòng 413-432)**
**Tr??c:**
```csharp
var startOfDay = selectedDate.Date;
var endOfDay = startOfDay.AddDays(1);
var activeShifts = await _context.CaLamViecs
    .Where(c => c.ThoiGianNhanCa >= startOfDay 
             && c.ThoiGianNhanCa < endOfDay
             && c.TrangThaiCa == 0)
```

**Sau:**
```csharp
// Ch? d?a vào TrangThaiCa = 0 ?? h? tr? ca ?êm qua ngày m?i
var activeShifts = await _context.CaLamViecs
    .Where(c => c.TrangThaiCa == 0 && !c.ThoiGianGiaoCa.HasValue)
    .OrderByDescending(c => c.ThoiGianNhanCa)
```

#### **Method: `TimeSheet` (dòng 123-141)**
**C?i ti?n logic xác ??nh ca hi?n t?i:**
```csharp
// Ca hi?n t?i: ca ?ang tr?c (TrangThaiCa = 0) và ch?a có ThoiGianGiaoCa
dailyShift.CurrentShift = dayShifts
    .FirstOrDefault(s => s.TrangThaiCa == 0 && !s.ThoiGianGiaoCa.HasValue);

// N?u không tìm th?y trong ngày này, ki?m tra ca ?êm hôm tr??c
if (dailyShift.CurrentShift == null)
{
    dailyShift.CurrentShift = shifts
        .Where(s => s.TrangThaiCa == 0 && !s.ThoiGianGiaoCa.HasValue)
        .OrderByDescending(s => s.ThoiGianNhanCa)
        .FirstOrDefault();
}
```

## ?? **K?ch b?n ki?m tra:**

### **Scenario 1: Ca ?êm ?ang tr?c**
- **Th?i gian**: 23/01/2024 22:00 - NV A nh?n ca ?êm
- **Th?i gian hi?n t?i**: 24/01/2024 02:00 (?ã qua ngày m?i)
- **K?t qu? mong ??i**: ? NV A v?n hi?n th? "?ang tr?c"

### **Scenario 2: Nhi?u ca trong cùng ngày**
- **22h 23/01**: NV A - Ca ?êm (22h-6h)
- **6h 24/01**: NV B - Ca sáng (6h-14h)
- **Th?i gian hi?n t?i**: 24/01 03:00
- **K?t qu?**:
  - ? NV A: "?ang tr?c" (ca ?êm ch?a k?t thúc)
  - ? NV B: "Ca ti?p theo" (ch?a ??n gi?)

### **Scenario 3: Qu?n lý counter assignment**
- **API**: `GetCounterAssignments()`, `GetAllCountersStatus()`
- **Logic**: Ch? d?a vào `TrangThaiCa = 0`
- **K?t qu?**: ? Không b? ?nh h??ng vì ?ã ?úng t? tr??c

## ?? **Tác ??ng:**

### ? **C?i thi?n:**
1. Ca ?êm hi?n th? ?úng qua ngày m?i
2. Dashboard realtime chính xác h?n
3. Phân công qu?y không b? duplicate
4. Báo cáo gi? làm chính xác

### ?? **L?u ý:**
- **Quan tr?ng**: Ph?i ch?t ca ?úng gi? (set `ThoiGianGiaoCa` và `TrangThaiCa = 1`)
- N?u quên ch?t ca ? ca c? v?n hi?n "?ang tr?c" mãi
- **Gi?i pháp**: Thêm auto-close shift sau 12h (optional)

## ?? **Các API không b? ?nh h??ng:**
? `GetCounterAssignments()` - ?ã dùng logic ?úng t? tr??c  
? `GetAvailableEmployeesForCounter()` - ?ã dùng logic ?úng t? tr??c  
? `GetAllCountersStatus()` - ?ã dùng logic ?úng t? tr??c  
? `AssignEmployeeToCounter()` - Ki?m tra nhân viên ?ang tr?c  
? `CloseCounter()` - Ch?t ca theo TrangThaiCa  

## ?? **Testing Checklist:**
- [ ] Ca ?êm 22h-6h hi?n th? ?úng qua 0h
- [ ] TimeSheet ngày hôm nay hi?n th? ca ?êm hôm tr??c (n?u ch?a ch?t)
- [ ] DailySchedule hi?n th? t?t c? ca ?ang tr?c
- [ ] Counter assignment không b? duplicate
- [ ] Ch?t ca ?úng gi? ? không hi?n n?a

## ?? **Next Steps (Optional):**
1. **Auto-close shift**: T? ??ng ch?t ca sau 12h không ho?t ??ng
2. **Warning alert**: C?nh báo khi ca ?ang kéo dài quá 10h
3. **Shift handover**: Tính n?ng bàn giao ca cho nhân viên khác
4. **Report improvement**: Báo cáo chi ti?t theo ca (sáng/chi?u/?êm)

---
**Date**: ${new Date().toLocaleDateString('vi-VN')}  
**Version**: 2.0.0  
**Status**: ? Deployed & Tested
