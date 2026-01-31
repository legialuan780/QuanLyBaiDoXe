# S?a l?i ch?c n?ng Ki?m soát gi? làm trong B?ng ch?m công

## T?ng quan
?ã hoàn thi?n và s?a l?i ch?c n?ng **Ki?m soát gi? làm** trong module **B?ng ch?m công** (TimeSheet).

## Các l?i ?ã s?a

### 1. **Thi?u Request Models trong Controller**
**V?n ??**: Các class Request models (`AdjustShiftRequest`, `OvertimeRequest`, `BreakShiftRequest`) ch?a ???c ??nh ngh?a trong Controller, gây l?i compilation.

**Gi?i pháp**: ?ã thêm các Request models vào file `VehicleShiftController.cs`:

```csharp
public class AdjustShiftRequest
{
    public int ShiftId { get; set; }
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class OvertimeRequest
{
    public int EmployeeId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int Type { get; set; }
    public string? Note { get; set; }
}

public class BreakShiftRequest
{
    public int ShiftId { get; set; }
    public int Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool NeedReplacement { get; set; }
    public int? ReplacementEmployeeId { get; set; }
}
```

### 2. **Code JavaScript b? duplicate**
**V?n ??**: Trong file `TimeSheet.cshtml`, các hàm `exportTimeSheet()` và `printTimeSheet()` b? duplicate, và code kh?i t?o Chart b? ??t sai v? trí (ngoài `$(document).ready()`).

**Gi?i pháp**: 
- Xóa code duplicate
- Di chuy?n code kh?i t?o Chart vào trong `$(document).ready()` block
- C?u trúc l?i code ?? d? ??c và maintain

### 3. **Thi?u hàm load l?ch s?**
**V?n ??**: Các hàm `loadOvertimeHistory()` và `loadBreakHistory()` ???c g?i nh?ng ch?a ???c ??nh ngh?a.

**Gi?i pháp**: ?ã thêm 2 hàm:
- `loadOvertimeHistory(employeeId)`: T?i và hi?n th? l?ch s? gi? bù c?a nhân viên
- `loadBreakHistory(employeeId)`: T?i và hi?n th? l?ch s? ca b? ng?t c?a nhân viên

### 4. **Thi?u d? li?u GhiChuBanGiao trong API**
**V?n ??**: API `GetEmployeeShifts` không tr? v? field `GhiChuBanGiao`, khi?n không th? phân lo?i ca bù gi? và ca ng?t.

**Gi?i pháp**: ?ã thêm `c.GhiChuBanGiao` vào Select statement c?a API.

## Các ch?c n?ng ?ã hoàn thi?n

### 1. **Giao di?n Modal "Ki?m soát gi? làm"**
Modal có 4 tabs chính:

#### Tab 1: Danh sách ca
- Hi?n th? t?t c? ca làm vi?c c?a nhân viên trong tháng
- Có checkbox ?? ch?n nhi?u ca cùng lúc
- Hi?n th? thông tin: Mã ca, Ngày, Gi? vào/ra, T?ng gi?, Tr?ng thái
- Nút "?i?u ch?nh" nhanh cho t?ng ca

#### Tab 2: ?i?u ch?nh gi?
- Ch?n ca c?n ?i?u ch?nh
- Nh?p gi? vào m?i và gi? ra m?i
- Nh?p lý do ?i?u ch?nh (b?t bu?c)
- C?nh báo: C?n phê duy?t t? qu?n lý

**Ch?c n?ng**:
- G?i API `AdjustShiftTime` ?? c?p nh?t gi? làm
- L?u lý do vào `GhiChuBanGiao`

#### Tab 3: Bù gi?
- Nh?p ngày, gi? b?t ??u, gi? k?t thúc
- Ch?n lo?i gi? bù:
  - T?ng ca th??ng (x1.5)
  - T?ng ca cu?i tu?n (x2.0)
  - T?ng ca l? (x3.0)
  - Gi? bù (không h??ng l??ng)
- Nh?p ghi chú
- Hi?n th? l?ch s? gi? bù

**Ch?c n?ng**:
- G?i API `AddOvertime` ?? t?o ca bù gi? m?i
- T? ??ng load l?i l?ch s? sau khi thêm thành công

#### Tab 4: Ng?t ca
- Ch?n ca c?n ng?t
- Ch?n lo?i ng?t ca:
  - Ngh? phép có l??ng
  - Ngh? không l??ng
  - Ngh? ?m
  - Ngh? vi?c riêng
  - Kh?n c?p
- Nh?p lý do (b?t bu?c)
- Tùy ch?n tìm ng??i thay th?
- Hi?n th? l?ch s? ng?t ca

**Ch?c n?ng**:
- G?i API `BreakShift` ?? ?ánh d?u ca b? ng?t
- T? ??ng set th?i gian giao ca là th?i ?i?m hi?n t?i n?u ca ch?a k?t thúc

### 2. **Nút truy c?p**
Trong b?ng ch?m công (TimeSheet), m?i nhân viên có 3 nút:
1. **Ki?m soát gi? làm** (màu xanh lá) - M? modal ki?m soát
2. **Xem chi ti?t** - Xem thông tin chi ti?t
3. **Xu?t b?ng công** - Export d? li?u

### 3. **API Endpoints ?ã hoàn thi?n**

#### `GET /Admin/VehicleShift/GetEmployeeShifts`
Parameters: `employeeId`, `month`, `year`
Returns: Danh sách ca làm vi?c kèm `GhiChuBanGiao`

#### `POST /Admin/VehicleShift/AdjustShiftTime`
Body: `AdjustShiftRequest`
Function: ?i?u ch?nh gi? vào/ra c?a ca

#### `POST /Admin/VehicleShift/AddOvertime`
Body: `OvertimeRequest`
Function: Thêm ca bù gi? m?i

#### `POST /Admin/VehicleShift/BreakShift`
Body: `BreakShiftRequest`
Function: Ng?t ca làm vi?c

#### `GET /Admin/VehicleShift/GetAvailableEmployees`
Returns: Danh sách nhân viên có th? thay th? (Qu?n lý, B?o v?)

## Cách s? d?ng

### Truy c?p giao di?n
1. Vào menu **Ca làm vi?c** ? **Qu?n lý ca làm vi?c**
2. Click nút **B?ng ch?m công** ? góc ph?i trên
3. T?i b?ng ch?m công, click nút icon **Ki?m soát gi? làm** (màu xanh lá) c?a nhân viên c?n qu?n lý

### ?i?u ch?nh gi? làm
1. M? modal Ki?m soát gi? làm
2. Ch?n tab **?i?u ch?nh gi?**
3. Ch?n ca c?n ?i?u ch?nh
4. Nh?p gi? vào/ra m?i
5. Nh?p lý do
6. Click **L?u ?i?u ch?nh**

### Thêm gi? bù
1. M? modal Ki?m soát gi? làm
2. Ch?n tab **Bù gi?**
3. Nh?p thông tin: Ngày, gi?, lo?i bù gi?
4. Click **Thêm gi? bù**

### Ng?t ca
1. M? modal Ki?m soát gi? làm
2. Ch?n tab **Ng?t ca**
3. Ch?n ca c?n ng?t và lo?i ng?t ca
4. Nh?p lý do
5. N?u c?n, tick "C?n tìm ng??i thay th?" và ch?n nhân viên
6. Click **Xác nh?n ng?t ca**

## Files ?ã s?a ??i
- `QuanLyBaiDoXe\Areas\Admin\Controllers\VehicleShiftController.cs`
- `QuanLyBaiDoXe\Areas\Admin\Views\VehicleShift\TimeSheet.cshtml`

## Testing
? Build successful
? T?t c? API endpoints ?ã ???c ??nh ngh?a
? JavaScript functions hoàn ch?nh
? Modal hi?n th? ?úng v?i 4 tabs
? Các form validation ho?t ??ng

## Ghi chú
- Ch?c n?ng này yêu c?u quy?n Admin ho?c Qu?n lý ?? truy c?p
- T?t c? thay ??i ?i?u ch?nh gi? và ng?t ca ??u ???c ghi nh?n vào `GhiChuBanGiao`
- Gi? bù ???c l?u d??i d?ng ca làm vi?c ??c bi?t v?i ghi chú "Bù gi?"
- Ca ng?t ???c ?ánh d?u b?ng tr?ng thái "?ã ch?t" v?i ghi chú "Ng?t ca"
