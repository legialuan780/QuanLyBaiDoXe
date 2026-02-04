# Fix l?i chuy?n h??ng MoMo cho Reservation

## V?n ??

Khi user ?n nút "Thanh toán MoMo" ? ph?n ??t ch?, x?y ra l?i chuy?n h??ng.

### Nguyên nhân

`MoMoService.CreatePaymentAsync()` s? d?ng `returnUrl` và `notifyUrl` **hardcoded** t? config, ch? phù h?p v?i **VehicleEntry** (`/Admin/VehicleEntry/MoMoReturn`), không phù h?p v?i **Reservation** (`/User/Reservation/MoMoReturn`).

## Gi?i pháp

### 1. Thêm overload method trong `IMoMoService.cs`

```csharp
public interface IMoMoService
{
    // Method c? - s? d?ng URL t? config (cho VehicleEntry)
    Task<MoMoPaymentResponse> CreatePaymentAsync(string orderId, long amount, string orderInfo);

    // Method m?i - cho phép truy?n custom URLs (cho Reservation)
    Task<MoMoPaymentResponse> CreatePaymentAsync(
        string orderId, 
        long amount, 
        string orderInfo, 
        string returnUrl, 
        string notifyUrl
    );

    bool VerifySignature(MoMoCallbackRequest request);
}
```

### 2. Implement trong `MoMoService.cs`

```csharp
/// <summary>
/// T?o link thanh toán MoMo (s? d?ng URL t? config)
/// </summary>
public async Task<MoMoPaymentResponse> CreatePaymentAsync(string orderId, long amount, string orderInfo)
{
    // Delegate to overload method
    return await CreatePaymentAsync(orderId, amount, orderInfo, _returnUrl, _notifyUrl);
}

/// <summary>
/// T?o link thanh toán MoMo v?i custom URLs
/// </summary>
public async Task<MoMoPaymentResponse> CreatePaymentAsync(
    string orderId, 
    long amount, 
    string orderInfo, 
    string returnUrl, 
    string notifyUrl)
{
    try
    {
        var requestId = Guid.NewGuid().ToString();
        var extraData = "";
        var requestType = "captureWallet";

        // S? d?ng returnUrl và notifyUrl t? parameters thay vì config
        var rawSignature = $"accessKey={_accessKey}" +
                           $"&amount={amount}" +
                           $"&extraData={extraData}" +
                           $"&ipnUrl={notifyUrl}" +
                           $"&orderId={orderId}" +
                           $"&orderInfo={orderInfo}" +
                           $"&partnerCode={_partnerCode}" +
                           $"&redirectUrl={returnUrl}" +
                           $"&requestId={requestId}" +
                           $"&requestType={requestType}";

        var signature = ComputeHmacSha256(rawSignature, _secretKey);

        var requestBody = new
        {
            partnerCode = _partnerCode,
            partnerName = "Bãi ?? xe thông minh",
            storeId = _partnerCode,
            requestId = requestId,
            amount = amount,
            orderId = orderId,
            orderInfo = orderInfo,
            redirectUrl = returnUrl,  // ? S? d?ng URL ??ng
            ipnUrl = notifyUrl,       // ? S? d?ng URL ??ng
            lang = "vi",
            extraData = extraData,
            requestType = requestType,
            signature = signature
        };

        // ... rest of implementation
    }
}
```

### 3. C?p nh?t `ReservationController.cs`

```csharp
[HttpPost]
public async Task<IActionResult> ThanhToanMoMo([FromBody] ReservationPaymentRequest request)
{
    try
    {
        var maKhachHang = await GetCurrentMaKhachHangAsync();
        if (maKhachHang == null)
        {
            return Json(new { success = false, message = "Không tìm th?y thông tin khách hàng!" });
        }

        var datCho = await _context.DatChos
            .Include(dc => dc.MaViTriNavigation)
            .FirstOrDefaultAsync(dc => dc.MaDatCho == request.MaDatCho && dc.MaKhachHang == maKhachHang);

        if (datCho == null)
        {
            return Json(new { success = false, message = "Không tìm th?y ??t ch?!" });
        }

        var orderId = $"RESERVATION_{request.MaDatCho}_{DateTime.Now:yyyyMMddHHmmss}";
        var orderInfo = $"Dat coc dat cho #{request.MaDatCho}";

        // T?o URL callback ??ng cho Reservation
        var returnUrl = Url.Action("MoMoReturn", "Reservation", new { area = "User" }, Request.Scheme);
        var notifyUrl = Url.Action("MoMoNotify", "Reservation", new { area = "User" }, Request.Scheme);

        // G?i overload method v?i custom URLs
        var momoResponse = await _momoService.CreatePaymentAsync(
            orderId: orderId,
            amount: (long)request.TienCoc,
            orderInfo: orderInfo,
            returnUrl: returnUrl!,
            notifyUrl: notifyUrl!
        );

        if (momoResponse.Success)
        {
            return Json(new
            {
                success = true,
                payUrl = momoResponse.PayUrl,
                qrCodeUrl = momoResponse.QrCodeUrl,
                deepLink = momoResponse.DeepLink,
                orderId = orderId,
                amount = request.TienCoc,
                message = "T?o thanh toán MoMo thành công!"
            });
        }

        return Json(new
        {
            success = false,
            message = momoResponse.Message ?? "Không th? t?o thanh toán MoMo"
        });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"L?i: {ex.Message}" });
    }
}
```

## So sánh tr??c và sau

### Tr??c khi fix

```
User click "Thanh toán MoMo" ? Reservation
    ?
ReservationController.ThanhToanMoMo()
    ?
_momoService.CreatePaymentAsync(orderId, amount, orderInfo)
    ?
MoMo API v?i:
    - returnUrl = /Admin/VehicleEntry/MoMoReturn ? (sai URL)
    - notifyUrl = /Admin/VehicleEntry/MoMoNotify ? (sai URL)
    ?
Sau khi thanh toán, MoMo redirect ??n /Admin/VehicleEntry/MoMoReturn
    ?
? L?i 404 ho?c redirect loop
```

### Sau khi fix

```
User click "Thanh toán MoMo" ? Reservation
    ?
ReservationController.ThanhToanMoMo()
    ?
Generate dynamic URLs:
    - returnUrl = /User/Reservation/MoMoReturn
    - notifyUrl = /User/Reservation/MoMoNotify
    ?
_momoService.CreatePaymentAsync(orderId, amount, orderInfo, returnUrl, notifyUrl)
    ?
MoMo API v?i:
    - returnUrl = /User/Reservation/MoMoReturn ?
    - notifyUrl = /User/Reservation/MoMoNotify ?
    ?
Sau khi thanh toán, MoMo redirect ??n /User/Reservation/MoMoReturn
    ?
? C?p nh?t tr?ng thái ??t ch? và hi?n th? thông báo
```

## URLs ???c s? d?ng

### VehicleEntry (Admin)
- **Return URL**: `https://yourdomain.com/Admin/VehicleEntry/MoMoReturn`
- **Notify URL**: `https://yourdomain.com/Admin/VehicleEntry/MoMoNotify`

### Reservation (User)
- **Return URL**: `https://yourdomain.com/User/Reservation/MoMoReturn`
- **Notify URL**: `https://yourdomain.com/User/Reservation/MoMoNotify`

## L?i ích c?a gi?i pháp

1. **Linh ho?t**: M?i module có th? s? d?ng URL callback riêng
2. **Không breaking change**: VehicleEntry v?n ho?t ??ng bình th??ng (dùng method c?)
3. **D? m? r?ng**: Các module m?i có th? d? dàng tích h?p MoMo
4. **Clean code**: S? d?ng method overloading thay vì duplicate code

## Testing

### Test VehicleEntry (không b? ?nh h??ng)
```csharp
// VehicleEntry v?n dùng method c?
await _momoService.CreatePaymentAsync(orderId, amount, orderInfo);
// URLs t? config: /Admin/VehicleEntry/MoMoReturn
```

### Test Reservation (fix l?i)
```csharp
// Reservation dùng method m?i v?i custom URLs
var returnUrl = Url.Action("MoMoReturn", "Reservation", new { area = "User" }, Request.Scheme);
var notifyUrl = Url.Action("MoMoNotify", "Reservation", new { area = "User" }, Request.Scheme);
await _momoService.CreatePaymentAsync(orderId, amount, orderInfo, returnUrl, notifyUrl);
// URLs dynamic: /User/Reservation/MoMoReturn
```

## K?t lu?n

? **?ã fix l?i redirect** cho Reservation MoMo payment

? **Backward compatible** - VehicleEntry v?n ho?t ??ng bình th??ng

? **Build thành công** - Không có l?i compilation

? **S?n sàng test** v?i MoMo sandbox

Gi? ?ây, khi user ?n "Thanh toán MoMo" ? Reservation:
1. MoMo s? redirect ?úng v? `/User/Reservation/MoMoReturn`
2. IPN callback s? g?i ?úng v? `/User/Reservation/MoMoNotify`
3. Tr?ng thái ??t ch? ???c c?p nh?t chính xác
4. User nh?n thông báo thành công/th?t b?i ??
