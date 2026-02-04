# C?p nh?t thanh toán MoMo cho ??t ch?

## T?ng quan
?ã c?p nh?t h? th?ng thanh toán MoMo cho ch?c n?ng ??t ch?, theo pattern c?a VehicleEntry ?? ??m b?o tính nh?t quán và x? lý ?úng.

## Các thay ??i chính

### 1. **ReservationController.cs** - C?p nh?t x? lý thanh toán MoMo

#### a. Action `ThanhToanMoMo` (POST)
```csharp
[HttpPost]
public async Task<IActionResult> ThanhToanMoMo([FromBody] ReservationPaymentRequest request)
```

**Thay ??i:**
- ? S? d?ng format orderId: `RESERVATION_{MaDatCho}_{timestamp}` (thay vì `DC{MaDatCho}_{timestamp}`)
- ? Tr? v? ??y ?? thông tin: `payUrl`, `qrCodeUrl`, `deepLink`, `orderId`, `amount`
- ? X? lý response gi?ng VehicleEntry
- ? Message rõ ràng h?n khi t?o thanh toán thành công/th?t b?i

**Response JSON:**
```json
{
    "success": true,
    "payUrl": "https://payment.momo.vn/...",
    "qrCodeUrl": "https://qr.momo.vn/...",
    "deepLink": "momo://...",
    "orderId": "RESERVATION_123_20240101120000",
    "amount": 50000,
    "message": "T?o thanh toán MoMo thành công!"
}
```

#### b. Action `MoMoReturn` (GET) - Callback URL
```csharp
[HttpGet]
public async Task<IActionResult> MoMoReturn(
    string partnerCode, string orderId, string requestId, 
    long amount, string orderInfo, string orderType,
    long transId, int resultCode, string message,
    string payType, long responseTime, 
    string extraData, string signature)
```

**Ch?c n?ng:**
- ? Nh?n callback t? MoMo sau khi thanh toán
- ? Parse orderId theo format: `RESERVATION_{MaDatCho}_{timestamp}`
- ? C?p nh?t tr?ng thái ??t ch? thành "?ã thanh toán" (1) n?u `resultCode == 0`
- ? Format s? ti?n v?i d?u phân cách: `amount.ToString("N0")`
- ? Set TempData message ?? hi?n th? k?t qu?
- ? Redirect v? `Index` n?u thành công, `Create` n?u th?t b?i

**TempData Messages:**
- Success: `"Thanh toán ??t c?c thành công! S? ti?n: {formattedAmount} VN?"`
- Error: `"Thanh toán th?t b?i: {message}"`

#### c. Action `MoMoNotify` (POST) - IPN t? MoMo
```csharp
[HttpPost]
public async Task<IActionResult> MoMoNotify([FromBody] MoMoCallbackRequest request)
```

**Ch?c n?ng:**
- ? Nh?n thông báo server-to-server t? MoMo (IPN)
- ? Xác minh ch? ký b?ng `_momoService.VerifySignature(request)`
- ? Parse orderId và c?p nh?t tr?ng thái ??t ch?
- ? Ch? c?p nh?t n?u tr?ng thái hi?n t?i là 0 (Ch? thanh toán)
- ? Return `Ok(new { message = "Success" })` cho MoMo

**L?u ý b?o m?t:**
- N?u signature không h?p l? ? return `BadRequest`
- Prevent double processing b?ng check `TrangThaiDatCho == 0`

### 2. **Index.cshtml** - Hi?n th? k?t qu? thanh toán

#### Thêm script ki?m tra TempData
```javascript
window.addEventListener('DOMContentLoaded', function() {
    @if(TempData["SuccessMessage"] != null)
    {
        alert('@TempData["SuccessMessage"]');
    }
    
    @if(TempData["ErrorMessage"] != null)
    {
        alert('@TempData["ErrorMessage"]');
    }
});
```

**Ch?c n?ng:**
- ? Hi?n th? thông báo thành công/l?i t? MoMo callback
- ? T? ??ng hi?n th? khi load trang sau khi redirect

### 3. **Create.cshtml** - JavaScript thanh toán MoMo

Function `payWithMoMo()` ?ã có s?n và ho?t ??ng ?úng:
```javascript
async function payWithMoMo() {
    const response = await fetch('@Url.Action("ThanhToanMoMo", "Reservation", new { area = "User" })', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            maDatCho: currentMaDatCho,
            tienCoc: currentTienCoc,
            phuongThucThanhToan: 'MoMo'
        })
    });

    const result = await response.json();
    if (result.success) {
        window.location.href = result.payUrl;
    } else {
        alert('L?i: ' + result.message);
    }
}
```

## Flow thanh toán MoMo

### 1. **User nh?n "Thanh toán MoMo"**
```
User ? payWithMoMo() ? POST /User/Reservation/ThanhToanMoMo
```

### 2. **Server t?o payment request v?i MoMo**
```
Controller ? MoMoService.CreatePaymentAsync()
         ? Return payUrl
```

### 3. **Redirect user ??n MoMo**
```
Browser ? window.location.href = result.payUrl
       ? Trang thanh toán MoMo
```

### 4. **User thanh toán trên MoMo**
```
User ? Quét QR / Nh?p OTP ? Xác nh?n thanh toán
```

### 5. **MoMo g?i callback (Return URL)**
```
MoMo ? GET /User/Reservation/MoMoReturn?orderId=...&resultCode=0
    ? Parse orderId
    ? Update TrangThaiDatCho = 1
    ? Set TempData["SuccessMessage"]
    ? Redirect to Index
```

### 6. **MoMo g?i IPN (Notify URL)**
```
MoMo Server ? POST /User/Reservation/MoMoNotify
           ? Verify signature
           ? Update TrangThaiDatCho = 1 (if not already)
           ? Return OK
```

### 7. **Hi?n th? k?t qu?**
```
Index page load ? Check TempData ? alert(message)
```

## OrderId Format

### C? (có v?n ??):
```
DC{maDatCho}_{timestamp}
Ví d?: DC123_20240101120000
```

### M?i (chu?n):
```
RESERVATION_{maDatCho}_{timestamp}
Ví d?: RESERVATION_123_20240101120000
```

**Lý do thay ??i:**
- D? parse h?n v?i `Split('_')`
- Nh?t quán v?i VehicleEntry: `PARKING_{maLuotGui}_{timestamp}`
- Prefix rõ ràng h?n (`RESERVATION` vs `DC`)

## Tr?ng thái ??t ch? (TrangThaiDatCho)

| Mã | Tên | Mô t? |
|----|-----|-------|
| 0 | Ch? thanh toán | V?a t?o ??t ch?, ch?a thanh toán |
| 1 | ?ã thanh toán | ?ã thanh toán ??t c?c thành công |
| 2 | Hoàn thành | ?ã check-in và s? d?ng d?ch v? |
| 3 | ?ã t? ch?i | Admin t? ch?i (n?u có) |
| 4 | ?ã h?y | User h?y ??t ch? |
| 5 | H?t h?n | Quá th?i gian ??n d? ki?n |

## Configuration MoMo

??m b?o c?u hình trong `appsettings.json`:

```json
{
  "MoMo": {
    "PartnerCode": "YOUR_PARTNER_CODE",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
    "ReturnUrl": "https://yourdomain.com/User/Reservation/MoMoReturn",
    "NotifyUrl": "https://yourdomain.com/User/Reservation/MoMoNotify",
    "RequestType": "captureWallet"
  }
}
```

**L?u ý:**
- `ReturnUrl`: User ???c redirect v? sau khi thanh toán
- `NotifyUrl`: MoMo g?i IPN ?? thông báo k?t qu? (server-to-server)
- C? 2 URL ??u c?n x? lý ?? ??m b?o thanh toán ???c ghi nh?n

## Testing

### Test Case 1: Thanh toán thành công
1. T?o ??t ch? m?i
2. Ch?n "Thanh toán MoMo"
3. Scan QR code ho?c nh?p OTP
4. Xác nh?n thanh toán
5. **Expected:** Redirect v? Index v?i message "Thanh toán ??t c?c thành công!"
6. **Verify:** TrangThaiDatCho = 1 trong database

### Test Case 2: Thanh toán th?t b?i
1. T?o ??t ch? m?i
2. Ch?n "Thanh toán MoMo"
3. H?y thanh toán trên MoMo
4. **Expected:** Redirect v? Create v?i message "Thanh toán th?t b?i"
5. **Verify:** TrangThaiDatCho = 0 (không thay ??i)

### Test Case 3: Invalid Signature
1. Mock request v?i signature sai
2. POST ??n `/User/Reservation/MoMoNotify`
3. **Expected:** HTTP 400 BadRequest
4. **Verify:** Không c?p nh?t database

## Troubleshooting

### L?i: "Yêu c?u sai ??nh d?ng"
- **Nguyên nhân:** Request body không ?úng format
- **Gi?i pháp:** ?ã thêm ??y ?? parameters cho `CreatePaymentAsync`

### L?i: Không nh?n ???c callback
- **Nguyên nhân:** URL không accessible t? internet
- **Gi?i pháp:** 
  - S? d?ng ngrok ?? expose localhost
  - Deploy lên server có public IP
  - C?p nh?t ReturnUrl/NotifyUrl trong MoMo Dashboard

### L?i: Signature không h?p l?
- **Nguyên nhân:** SecretKey không ?úng ho?c cách tính signature sai
- **Gi?i pháp:** Check `MoMoService.VerifySignature()` implementation

## So sánh v?i VehicleEntry

| Feature | VehicleEntry | Reservation |
|---------|-------------|-------------|
| OrderId Format | `PARKING_{maLuotGui}_{timestamp}` | `RESERVATION_{maDatCho}_{timestamp}` |
| Return URL | `/Admin/VehicleEntry/MoMoReturn` | `/User/Reservation/MoMoReturn` |
| Notify URL | `/Admin/VehicleEntry/MoMoNotify` | `/User/Reservation/MoMoNotify` |
| Payment Info | "Thanh toan phi gui xe" | "Dat coc dat cho" |
| Status Update | XuLyXeRaAsync() | TrangThaiDatCho = 1 |
| Success Message | "Thanh toán MoMo thành công!" | "Thanh toán ??t c?c thành công!" |

## K?t lu?n

? **?ã hoàn thành:**
- C?p nh?t ReservationController v?i ??y ?? 3 actions: ThanhToanMoMo, MoMoReturn, MoMoNotify
- Format orderId chu?n: `RESERVATION_{MaDatCho}_{timestamp}`
- X? lý callback và IPN t? MoMo
- Hi?n th? k?t qu? thanh toán v?i TempData
- Nh?t quán v?i pattern c?a VehicleEntry

? **Build thành công** - Không có l?i compilation

? **S?n sàng test** v?i MoMo sandbox environment
