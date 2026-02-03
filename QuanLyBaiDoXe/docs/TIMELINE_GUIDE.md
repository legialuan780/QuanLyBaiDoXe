# ?? H??ng d?n Giao di?n Timeline L?ch Làm Vi?c

## ?? Giao di?n m?i - Timeline Schedule

Giao di?n l?ch làm vi?c ?ã ???c thi?t k? l?i hoàn toàn theo ki?u **Timeline** (cu?n l?ch) v?i các tính n?ng sau:

### ? Các tính n?ng chính:

#### 1. **Timeline View theo Gi?**
- Tr?c th?i gian ngang: 0h ? 24h (hi?n th? t?t c? các gi? trong ngày)
- Danh sách nhân viên d?c bên trái
- Các ca làm vi?c hi?n th? d?ng **thanh màu kéo dài** theo khung gi?

#### 2. **Thanh Ca Làm Vi?c (Shift Bars)**
- **Ca sáng (6h-14h)**: Thanh màu vàng cam gradient ??
- **Ca chi?u (14h-22h)**: Thanh màu xanh lá gradient ???
- **Ca ?êm (22h-6h)**: Thanh màu xám ?en gradient ??

M?i thanh ca hi?n th?:
- Icon ca làm vi?c
- Tên nhân viên
- S? ca
- Icon ghi chú (n?u có)

#### 3. **T??ng tác**
- **Click vào thanh ca**: Xem chi ti?t và có th? xóa
- **Hover vào hàng nhân viên**: Hi?n nút "Thêm ca" n?u ch?a có ca
- **Click "Thêm ca"**: M? modal v?i thông tin nhân viên và ngày ?ã ?i?n s?n

#### 4. **Navigation**
- Nút "Hôm qua" / "Ngày mai": Chuy?n ngày nhanh
- Date picker: Ch?n ngày b?t k?
- Hi?n th? th? và ngày ??y ?? b?ng ti?ng Vi?t

#### 5. **Th?ng kê**
Ph?n summary bên d??i hi?n th?:
- S? ca sáng trong ngày
- S? ca chi?u trong ngày
- S? ca ?êm trong ngày
- T?ng s? ca làm vi?c

### ?? ?u ?i?m c?a giao di?n m?i:

1. **Tr?c quan**: Nhìn th?y toàn b? l?ch làm vi?c trong ngày m?t cách rõ ràng
2. **D? qu?n lý**: Xác ??nh nhanh ai làm ca nào, gi? nào
3. **T??ng tác t?t**: Hover, click, tooltip ??y ??
4. **Responsive**: T??ng thích mobile v?i scroll ngang
5. **Hi?n ??i**: Gradient màu, animation m??t mà

### ?? Cách s? d?ng:

1. **Xem l?ch ngày hi?n t?i**: M?c ??nh hi?n th? ngày hôm nay
2. **Chuy?n ngày**: Click "Hôm qua" / "Ngày mai" ho?c ch?n date picker
3. **Thêm ca m?i**: 
   - Click nút "Thêm l?ch" (góc ph?i)
   - Ho?c hover vào nhân viên ch?a có ca ? click "Thêm ca"
4. **Xem chi ti?t ca**: Click vào thanh màu
5. **Xóa ca**: Click vào thanh ? Modal chi ti?t ? Nút "Xóa"

### ?? Code Structure:

**Controller**: `VehicleShiftController.Schedule()`
- Load t?t c? nhân viên ?ang làm vi?c
- Load l?ch làm vi?c c?a ngày ???c ch?n
- Tr? v? view v?i d? li?u

**View**: `Schedule.cshtml`
- Timeline header: Hi?n th? tr?c gi?
- Timeline body: Loop nhân viên và hi?n th? các thanh ca
- Statistics: Th?ng kê s? ca

**CSS**: `vehicle-shift.css`
- `.timeline-container`: Container chính
- `.shift-bar`: Thanh ca làm vi?c
- `.shift-morning/afternoon/night`: Màu s?c các ca
- Responsive styles

### ?? Responsive Design:

- **Desktop**: Hi?n th? full timeline v?i scroll ngang
- **Tablet**: Thu nh? employee column, timeline scroll ngang
- **Mobile**: Compact view v?i overlay controls

---

**Developed with ?? for QuanLyBaiDoXe**
