# H? Th?ng ??t Ch? - Tài Li?u K? Thu?t

## T?ng Quan

H? th?ng ??t ch? cho phép khách hàng ??t tr??c v? trí ?? xe v?i 2 lo?i:

1. **??t Trong Ngày (G?p)** - ??t và s? d?ng ngay
2. **??t H?n L?ch** - ??t tr??c ít nh?t 2 ngày

## Ki?n Trúc

### 1. Entities (Models/Entities/)

#### DatCho.cs
```csharp
- MaDatCho (PK)
- MaKhachHang (FK -> KhachHang)
- MaViTri (FK -> ViTriDo)
- ThoiGianDat
- ThoiGianDenDuKien
- ThoiGianHetHan
- TrangThaiDatCho (0: Ch? x? lý, 1: ?ã duy?t, 2: Hoàn thành, 3: T? ch?i, 4: ?ã h?y, 5: H?t h?n)
```

### 2. Services

#### IReservationService & ReservationService
- `TaoDatChoAsync()` - T?o ??t ch? m?i
- `DuyetDatChoAsync()` - Admin duy?t ??t ch?
- `TuChoiDatChoAsync()` - Admin t? ch?i
- `HuyDatChoAsync()` - User h?y ??t ch?
- `ThanhToanDatCocAsync()` - Thanh toán ??t c?c
- `TinhTienCocAsync()` - Tính ti?n c?c (50% giá 1 gi?)
- `KiemTraViTriDaDatChoAsync()` - Ki?m tra v? trí ?ã ??t
- `XuLyDatChoHetHanAsync()` - X? lý t? ??ng ??t ch? h?t h?n
- `SetupViTriDatTruocAsync()` - Admin setup v? trí cho ngày mai

### 3. Controllers

#### User/ReservationController
- `Index()` - Xem danh sách ??t ch? c?a mình
- `Create()` - Form t?o ??t ch?
- `TaoDatCho()` - API t?o ??t ch?
- `GetViTriTrong()` - L?y v? trí tr?ng theo lo?i xe
- `ThanhToanTienMat()` - Thanh toán ti?n m?t
- `ThanhToanMoMo()` - Thanh toán qua MoMo
- `HuyDatCho()` - H?y ??t ch?

#### Admin/ReservationManagementController
- `Index()` - Qu?n lý t?t c? ??t ch?
- `ChoDuyet()` - Danh sách ch? duy?t
- `DuyetDatCho()` - Duy?t ??t ch?
- `TuChoiDatCho()` - T? ch?i ??t ch?
- `SetupViTriDatTruoc()` - Setup v? trí ??t tr??c
- `GetThongKeDatCho()` - Th?ng kê
- `XuLyDatChoHetHan()` - Background job x? lý h?t h?n

## Lu?ng X? Lý

### A. ??t Trong Ngày (G?p)

1. **User ch?n v? trí và lo?i xe**
   - Xem b?n ?? bãi ??
   - Ch?n v? trí tr?ng phù h?p v?i lo?i xe

2. **T?o ??t ch?**
   - G?i request: `POST /User/Reservation/TaoDatCho`
   - System t?o ??t ch? v?i TrangThaiDatCho = 0 (Ch? thanh toán)
   - ?ánh d?u v? trí TrangThai = 2 (?ã ??t)
   - Th?i gian h?t h?n: ThoiGianDenDuKien + 2 gi?

3. **Thanh toán ??t c?c**
   - **Ti?n m?t**: `POST /User/Reservation/ThanhToanTienMat`
     - C?p nh?t TrangThaiDatCho = 1 (?ã thanh toán)
   
   - **MoMo**: `POST /User/Reservation/ThanhToanMoMo`
     - T?o link thanh toán MoMo
     - User quét QR ho?c m? link
     - Callback v? `/User/Reservation/MoMoReturn`
     - C?p nh?t TrangThaiDatCho = 1 (?ã thanh toán)

4. **Xe vào**
   - Khi xe ??n, quét th? t?i VehicleEntry
   - System t? ??ng gán v? trí ?ã ??t
   - T?o LuotGui v?i MaDatCho

5. **H?t h?n n?u không ??n**
   - Sau ThoiGianHetHan, background job t? ??ng:
     - C?p nh?t TrangThaiDatCho = 5 (H?t h?n)
     - Gi?i phóng v? trí: TrangThai = 0 (Tr?ng)

### B. ??t H?n L?ch (Tr??c ? 2 ngày)

1. **User g?i yêu c?u ??t ch?**
   - Ch?n ngày ??n (ph?i ? 2 ngày t? hi?n t?i)
   - Ch?n v? trí
   - G?i request: `POST /User/Reservation/TaoDatCho` v?i `IsDatTrongNgay = false`
   - System t?o ??t ch? v?i TrangThaiDatCho = 0 (Ch? duy?t)
   - V? trí v?n TrangThai = 0 (ch?a lock)

2. **Admin duy?t**
   - Admin vào `/Admin/ReservationManagement/ChoDuyet`
   - Xem chi ti?t ??t ch?
   - **Duy?t**: `POST /Admin/ReservationManagement/DuyetDatCho`
     - C?p nh?t TrangThaiDatCho = 1 (?ã duy?t)
     - ?ánh d?u v? trí TrangThai = 2 (?ã ??t)
   
   - **T? ch?i**: `POST /Admin/ReservationManagement/TuChoiDatCho`
     - C?p nh?t TrangThaiDatCho = 3 (T? ch?i)
     - Thông báo lý do cho khách hàng

3. **Setup ngày hôm tr??c**
   - Admin setup v? trí: `POST /Admin/ReservationManagement/SetupViTriDatTruoc`
   - Ki?m tra v? trí có xe ?ang ?? không:
     - **N?u có xe**: Thông báo "Xe ph?i ra tr??c 23h hôm nay"
     - **N?u tr?ng**: Confirm setup thành công

4. **Xe vào ngày ?ã ??t**
   - T??ng t? ??t trong ngày
   - System gán v? trí ?ã ??t
   - C?p nh?t TrangThaiDatCho = 2 (Hoàn thành)

5. **H?t h?n**
   - N?u không ??n ?úng ngày (sau 23h)
   - Background job t? ??ng h?y và gi?i phóng v? trí

## Tr?ng Thái V? Trí (ViTriDo.TrangThai)

| Giá tr? | Ý ngh?a | Mô t? |
|---------|---------|-------|
| 0 | Tr?ng | V? trí tr?ng, có th? ??t/?? |
| 1 | ?ã ?? | Có xe ?ang ?? |
| 2 | ?ã ??t | ?ã có ng??i ??t tr??c |

## Tr?ng Thái ??t Ch? (DatCho.TrangThaiDatCho)

| Giá tr? | Ý ngh?a | Áp d?ng |
|---------|---------|---------|
| 0 | Ch? x? lý | - ??t trong ngày: Ch? thanh toán<br>- ??t h?n l?ch: Ch? admin duy?t |
| 1 | ?ã duy?t | - ??t trong ngày: ?ã thanh toán<br>- ??t h?n l?ch: Admin ?ã duy?t |
| 2 | Hoàn thành | Xe ?ã vào và s? d?ng xong |
| 3 | T? ch?i | Admin t? ch?i ??t h?n l?ch |
| 4 | ?ã h?y | User t? h?y |
| 5 | H?t h?n | Quá th?i gian, t? ??ng h?y |

## API Endpoints

### User APIs

```
GET  /User/Reservation/Index
     - Xem danh sách ??t ch?

GET  /User/Reservation/Create
     - Form t?o ??t ch?

POST /User/Reservation/TaoDatCho
     Body: { MaViTri, BienSoXe, MaLoaiXe, ThoiGianDenDuKien, IsDatTrongNgay }
     - T?o ??t ch?

GET  /User/Reservation/GetViTriTrong?maLoaiXe=1&thoiGianDenDuKien=2024-01-01
     - L?y v? trí tr?ng

POST /User/Reservation/ThanhToanTienMat
     Body: { MaDatCho, TienCoc, PhuongThucThanhToan: "TienMat" }
     - Thanh toán ti?n m?t

POST /User/Reservation/ThanhToanMoMo
     Body: { MaDatCho, TienCoc, PhuongThucThanhToan: "MoMo" }
     - T?o link thanh toán MoMo

GET  /User/Reservation/MoMoReturn
     - Callback MoMo

POST /User/Reservation/HuyDatCho?maDatCho=1
     - H?y ??t ch?
```

### Admin APIs

```
GET  /Admin/ReservationManagement/Index
     - Qu?n lý t?t c? ??t ch?

GET  /Admin/ReservationManagement/ChoDuyet
     - Danh sách ch? duy?t

POST /Admin/ReservationManagement/DuyetDatCho?maDatCho=1
     - Duy?t ??t ch?

POST /Admin/ReservationManagement/TuChoiDatCho?maDatCho=1&lyDo=...
     - T? ch?i ??t ch?

POST /Admin/ReservationManagement/SetupViTriDatTruoc?maDatCho=1
     - Setup v? trí ??t tr??c

GET  /Admin/ReservationManagement/GetChiTietDatCho?maDatCho=1
     - Chi ti?t ??t ch?

GET  /Admin/ReservationManagement/GetThongKeDatCho
     - Th?ng kê

POST /Admin/ReservationManagement/XuLyDatChoHetHan
     - Background job x? lý h?t h?n (t? ??ng ch?y)
```

### ParkingMap APIs (C?p nh?t)

```
GET  /User/ParkingMap/GetParkingSpots?maLoaiXe=1&thoiGianDenDuKien=2024-01-01
     - Xem b?n ?? bãi, filter theo lo?i xe và th?i gian

GET  /User/ParkingMap/GetLoaiXes
     - L?y danh sách lo?i xe
```

## Tính Ti?n C?c

- Ti?n c?c = 50% giá gi? ??u tiên
- Query t? b?ng `CauHinhGia` và `ChiTietGia`
- M?c ??nh: 10,000 VN? n?u không có c?u hình

## Background Jobs

### X? Lý ??t Ch? H?t H?n

Ch?y ??nh k? (ví d?: m?i 15 phút) ??:
1. Tìm các ??t ch? có `ThoiGianHetHan <= DateTime.Now`
2. C?p nh?t `TrangThaiDatCho = 5` (H?t h?n)
3. Gi?i phóng v? trí: `ViTriDo.TrangThai = 0`

**Cách setup:**
- S? d?ng Hangfire ho?c Windows Task Scheduler
- Endpoint: `POST /Admin/ReservationManagement/XuLyDatChoHetHan`

## Database Schema

```sql
-- B?ng DatCho ?ã t?n t?i, không c?n t?o m?i
-- Các c?t:
-- MaDatCho INT PRIMARY KEY
-- MaKhachHang INT FK
-- MaViTri INT FK
-- ThoiGianDat DATETIME DEFAULT GETDATE()
-- ThoiGianDenDuKien DATETIME
-- ThoiGianHetHan DATETIME
-- TrangThaiDatCho INT DEFAULT 0

-- B?ng LuotGui có c?t MaDatCho ?? liên k?t
-- MaDatCho INT FK -> DatCho.MaDatCho
```

## Testing

### Test Case 1: ??t Trong Ngày - Thành Công
1. User ch?n v? trí tr?ng
2. Ch?n lo?i xe phù h?p
3. Nh?p th?i gian ??n (trong vòng 2 gi?)
4. Thanh toán MoMo thành công
5. V? trí ???c lock
6. Xe ??n và vào bãi thành công

### Test Case 2: ??t H?n L?ch - Admin Duy?t
1. User ch?n ngày ??n sau 3 ngày
2. G?i yêu c?u
3. Admin xem và duy?t
4. V? trí ???c lock
5. Ngày ??n, xe vào bãi thành công

### Test Case 3: H?t H?n
1. User ??t nh?ng không thanh toán
2. Sau 30 phút, background job ch?y
3. ??t ch? b? h?y, v? trí ???c gi?i phóng

### Test Case 4: Setup V? Trí Có Xe
1. Admin duy?t ??t ch? cho ngày mai
2. V? trí ?ó hi?n có xe ?ang ??
3. Admin setup -> Thông báo "Xe ph?i ra tr??c 23h"
4. Xe ra tr??c 23h
5. Ngày mai v? trí tr?ng cho khách ??t

## Deployment Checklist

- [ ] ??ng ký `IReservationService` trong `Program.cs` ?
- [ ] C?p nh?t migrations n?u có thay ??i schema
- [ ] Setup background job cho `XuLyDatChoHetHan`
- [ ] C?u hình MoMo credentials
- [ ] T?o Views cho User/Reservation và Admin/ReservationManagement
- [ ] Test t?t c? flows
- [ ] Thêm menu navigation cho User và Admin
- [ ] Setup email notifications (optional)

## Notes

- V? trí v?i `TrangThai = 2` (?ã ??t) không cho xe khác vào
- Khi xe vào b?ng ??t ch?, ?u tiên gán ?úng v? trí ?ã ??t
- Admin có th? t? ch?i ??t h?n l?ch n?u không phù h?p
- User ch? h?y ???c ??t ch? ch?a hoàn thành
- Ti?n c?c không hoàn l?i n?u user không ??n (có th? customize)
