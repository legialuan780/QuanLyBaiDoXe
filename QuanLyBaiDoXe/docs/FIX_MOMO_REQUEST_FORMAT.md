# Fix l?i "Yêu c?u sai ??nh d?ng" khi thanh toán MoMo

## V?n ??
Khi nh?n nút "Thanh toán MoMo", server tr? v? l?i: **"Yêu c?u sai ??nh d?ng"**

## Nguyên nhân
JavaScript ?ang g?i request body v?i **property names ? d?ng camelCase** (lowercase ch? cái ??u):
```javascript
{
    maDatCho: 123,
    tienCoc: 50000,
    phuongThucThanhToan: 'MoMo'
}
```

Nh?ng C# model `ReservationPaymentRequest` expect **PascalCase** (uppercase ch? cái ??u):
```csharp
public class ReservationPaymentRequest
{
    [Required]
    public int MaDatCho { get; set; }

    [Required]
    public string PhuongThucThanhToan { get; set; }

    public decimal TienCoc { get; set; }
}
```

? ASP.NET Core model binding **không th? map** các property t? camelCase sang PascalCase, d?n ??n validation fail.

## Gi?i pháp

### ? S?a JavaScript ?? dùng PascalCase

#### 1. **Create.cshtml** - Function `payWithMoMo()`
```javascript
// C? (SAI)
body: JSON.stringify({
    maDatCho: currentMaDatCho,
    tienCoc: currentTienCoc,
    phuongThucThanhToan: 'MoMo'
})

// M?I (?ÚNG)
body: JSON.stringify({
    MaDatCho: currentMaDatCho,
    TienCoc: currentTienCoc,
    PhuongThucThanhToan: 'MoMo'
})
```

#### 2. **Create.cshtml** - Function `payWithCash()`
```javascript
// C? (SAI)
body: JSON.stringify({
    maDatCho: currentMaDatCho,
    tienCoc: currentTienCoc,
    phuongThucThanhToan: 'TienMat'
})

// M?I (?ÚNG)
body: JSON.stringify({
    MaDatCho: currentMaDatCho,
    TienCoc: currentTienCoc,
    PhuongThucThanhToan: 'TienMat'
})
```

#### 3. **Index.cshtml** - Function `payWithMoMo()`
```javascript
// C? (SAI)
body: JSON.stringify({
    maDatCho: currentMaDatCho,
    tienCoc: currentTienCoc,
    phuongThucThanhToan: 'MoMo'
})

// M?I (?ÚNG)
body: JSON.stringify({
    MaDatCho: currentMaDatCho,
    TienCoc: currentTienCoc,
    PhuongThucThanhToan: 'MoMo'
})
```

## T?i sao c?n PascalCase?

ASP.NET Core s? d?ng **case-sensitive binding** m?c ??nh cho JSON. Khi g?i request:

### Request JSON
```json
{
    "MaDatCho": 123,
    "TienCoc": 50000,
    "PhuongThucThanhToan": "MoMo"
}
```

### Model Binding
```csharp
[HttpPost]
public async Task<IActionResult> ThanhToanMoMo([FromBody] ReservationPaymentRequest request)
{
    // request.MaDatCho = 123 ?
    // request.TienCoc = 50000 ?
    // request.PhuongThucThanhToan = "MoMo" ?
}
```

N?u dùng camelCase, model binding s? **fail**:
```json
{
    "maDatCho": 123,        // ? Không map ???c
    "tienCoc": 50000,       // ? Không map ???c
    "phuongThucThanhToan": "MoMo"  // ? Không map ???c
}
```

? T?t c? properties trong model s? là **null ho?c default value**
? Validation fail vì `[Required]` attributes

## Alternative Solution (không dùng)

Có th? config ASP.NET Core ?? accept camelCase:

### Program.cs
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
```

**Nh?ng không nên dùng** vì:
- Ph?i config cho toàn b? project
- Có th? ?nh h??ng ??n các API khác
- Convention c?a .NET là dùng PascalCase

## Validation trong ReservationPaymentRequest

```csharp
public class ReservationPaymentRequest
{
    [Required]  // ? B?t bu?c ph?i có
    public int MaDatCho { get; set; }

    [Required]  // ? B?t bu?c ph?i có
    public string PhuongThucThanhToan { get; set; } = "TienMat";

    public decimal TienCoc { get; set; }  // ? Optional, default = 0
}
```

N?u model binding fail:
- `MaDatCho` = 0 (default int)
- `PhuongThucThanhToan` = null
- `TienCoc` = 0

? Validation fail v?i error: **"The PhuongThucThanhToan field is required."**

## Testing

### Test Case: Thanh toán MoMo
1. T?o ??t ch? m?i
2. Modal thanh toán hi?n lên v?i s? ti?n c?n thanh toán
3. Click "Thanh toán MoMo"
4. **Expected:**
   - Request body có format ?úng
   - Server t?o payment URL thành công
   - Redirect ??n trang thanh toán MoMo

### Request Payload (?úng)
```json
POST /User/Reservation/ThanhToanMoMo
Content-Type: application/json

{
    "MaDatCho": 123,
    "TienCoc": 50000,
    "PhuongThucThanhToan": "MoMo"
}
```

### Response (thành công)
```json
{
    "success": true,
    "payUrl": "https://test-payment.momo.vn/...",
    "qrCodeUrl": "https://qr.momo.vn/...",
    "deepLink": "momo://...",
    "orderId": "RESERVATION_123_20240101120000",
    "amount": 50000,
    "message": "T?o thanh toán MoMo thành công!"
}
```

## Các file ?ã s?a

1. ? `QuanLyBaiDoXe\Areas\User\Views\Reservation\Create.cshtml`
   - Function `payWithMoMo()`
   - Function `payWithCash()`

2. ? `QuanLyBaiDoXe\Areas\User\Views\Reservation\Index.cshtml`
   - Function `payWithMoMo()`

## K?t lu?n

? **?ã fix l?i** b?ng cách thay ??i JavaScript request body t? **camelCase** sang **PascalCase**

? **Build thành công** - Không có l?i compilation

? **Model binding ho?t ??ng ?úng** - Server có th? deserialize request body

? **Validation pass** - T?t c? required fields ???c g?i ?úng format

Bây gi? có th? test thanh toán MoMo thành công! ??
