# C?u hình Default Login Page

## T?ng quan
H? th?ng ?ã ???c c?u hình ?? **luôn luôn m? trang Login** khi truy c?p URL g?c.

## Các thay ??i

### 1. Program.cs - Root URL Redirect

#### Redirect "/" v? Login
```csharp
// Redirect root URL to Login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});
```

**Ch?c n?ng**:
- B?t request ??n `/` (root)
- Redirect v? `/Account/Login`
- T? ??ng, không c?n check authentication

#### Default Route thay ??i
```csharp
// OLD - Default v? Home
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// NEW - Default v? Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
```

### 2. HomeController.cs - Require Login

#### Thêm [Authorize]
```csharp
[Authorize] // Yêu c?u ??ng nh?p
public IActionResult Index()
{
    return View();
}

[Authorize]
public IActionResult Privacy()
{
    return View();
}
```

**Ch?c n?ng**:
- N?u ch?a ??ng nh?p ? Redirect v? `/Account/Login`
- N?u ?ã ??ng nh?p ? Hi?n th? trang Home

## Flow ho?t ??ng

### Truy c?p Root URL
```
1. User m?: https://localhost:7093/
   ?
2. app.MapGet("/") b?t request
   ?
3. Redirect: /Account/Login
   ?
4. AccountController.Login() ? Login view
```

### Truy c?p Home khi ch?a login
```
1. User m?: https://localhost:7093/Home
   ?
2. HomeController.Index() - [Authorize]
   ?
3. Not authenticated
   ?
4. Redirect: /Account/Login?ReturnUrl=/Home
   ?
5. Sau khi login ? Return v? /Home
```

### Truy c?p khi ?ã login
```
1. User ?ã ??ng nh?p
   ?
2. M?: https://localhost:7093/
   ?
3. Redirect: /Account/Login
   ?
4. AccountController.Login() check authentication
   ?
5. User.Identity.IsAuthenticated == true
   ?
6. Redirect v? Home ho?c Admin
```

## Testing

### Test Case 1: M? root URL
```
Action: 
- Browser m? https://localhost:7093/

Expected:
? Redirect to /Account/Login
? Hi?n th? trang Login
? URL: https://localhost:7093/Account/Login
```

### Test Case 2: M? Home khi ch?a login
```
Action:
- Ch?a ??ng nh?p
- Browser m? https://localhost:7093/Home

Expected:
? Redirect to /Account/Login?ReturnUrl=/Home
? Hi?n th? trang Login
? Sau khi login ? Quay l?i /Home
```

### Test Case 3: ?ã login, m? root
```
Action:
- ?ã ??ng nh?p (Admin)
- Browser m? https://localhost:7093/

Expected:
? Redirect to /Account/Login
? AccountController check authenticated
? Redirect to /Admin/Dashboard (vì role Admin)
```

### Test Case 4: ?ã login, m? Home
```
Action:
- ?ã ??ng nh?p (Customer)
- Browser m? https://localhost:7093/Home

Expected:
? [Authorize] pass
? Hi?n th? Home page
? User có th? s? d?ng tính n?ng
```

## URL Mapping

| URL | Ch?a Login | ?ã Login (Customer) | ?ã Login (Admin) |
|-----|------------|---------------------|------------------|
| `/` | ? /Account/Login | ? /Account/Login ? /Home | ? /Account/Login ? /Admin |
| `/Home` | ? /Account/Login?ReturnUrl=/Home | ? /Home | ? /Home |
| `/Admin` | ? /Account/Login?ReturnUrl=/Admin | ? Access Denied | ? /Admin/Dashboard |
| `/Account/Login` | Show Login | ? /Home or /Admin | ? /Admin/Dashboard |
| `/Account/Register` | Show Register | ? /Home or /Admin | ? /Admin/Dashboard |

## AccountController Logic

### Login GET - Redirect n?u ?ã ??ng nh?p
```csharp
[HttpGet]
public IActionResult Login(string? returnUrl)
{
    // N?u ?ã ??ng nh?p, redirect v? trang ch?
    if (User.Identity?.IsAuthenticated == true)
    {
        // Có th? custom logic ? ?ây
        return RedirectToAction("Index", "Home");
    }

    ViewBag.ReturnUrl = returnUrl;
    return View("~/Views/Account/Login.cshtml");
}
```

### Custom Redirect sau khi Login
```csharp
// Redirect based on role
if (role == "Admin" || role == "Employee")
{
    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
}

if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
{
    return Redirect(returnUrl);
}

return RedirectToAction("Index", "Home");
```

## Customization

### Option 1: Redirect Admin v? Admin, Customer v? Login
```csharp
app.MapGet("/", context =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Admin" || role == "Employee")
        {
            context.Response.Redirect("/Admin/Dashboard");
        }
        else
        {
            context.Response.Redirect("/Home");
        }
    }
    else
    {
        context.Response.Redirect("/Account/Login");
    }
    return Task.CompletedTask;
});
```

### Option 2: Landing Page v?i Welcome screen
```csharp
// T?o LandingController
public class LandingController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Redirect based on role
        }
        return View(); // Welcome page
    }
}

// Program.cs
app.MapGet("/", context =>
{
    context.Response.Redirect("/Landing");
    return Task.CompletedTask;
});
```

### Option 3: Smart Redirect
```csharp
app.MapGet("/", async context =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var returnUrl = context.Request.Query["returnUrl"].ToString();
        
        if (!string.IsNullOrEmpty(returnUrl))
        {
            context.Response.Redirect(returnUrl);
        }
        else if (role == "Admin" || role == "Employee")
        {
            context.Response.Redirect("/Admin/Dashboard");
        }
        else
        {
            context.Response.Redirect("/Home");
        }
    }
    else
    {
        context.Response.Redirect("/Account/Login");
    }
});
```

## Security Considerations

### 1. Luôn check authentication
```csharp
[Authorize] // Trên controller ho?c action
```

### 2. ReturnUrl validation
```csharp
if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
{
    return Redirect(returnUrl);
}
```

### 3. Role-based redirect
```csharp
if (role == "Admin" || role == "Employee")
{
    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
}
```

## Benefits

### 1. Security
- ? Không th? truy c?p h? th?ng khi ch?a ??ng nh?p
- ? M?i page ??u require authentication
- ? T? ??ng redirect v? login

### 2. User Experience
- ? Clear entry point (Login)
- ? Không b? l?c trang
- ? Consistent behavior

### 3. Maintenance
- ? D? debug
- ? Rõ ràng flow
- ? Centralized redirect logic

## Troubleshooting

### Issue 1: Redirect loop
**Tri?u ch?ng**: Trang c? redirect mãi

**Nguyên nhân**:
- Login page không accessible
- Cookie không set ???c

**Gi?i pháp**:
```csharp
// AccountController.Login không ???c có [Authorize]
[HttpGet]
public IActionResult Login() // NO [Authorize] here
```

### Issue 2: Không redirect v? returnUrl
**Tri?u ch?ng**: Sau login không v? trang c?

**Nguyên nhân**: ReturnUrl b? lost

**Gi?i pháp**:
```csharp
[HttpGet]
public IActionResult Login(string? returnUrl)
{
    ViewBag.ReturnUrl = returnUrl; // Pass to view
}

[HttpPost]
public async Task<IActionResult> Login(string email, string password, string? returnUrl)
{
    // Use returnUrl after successful login
}
```

### Issue 3: 404 Error
**Tri?u ch?ng**: Redirect ??n trang không t?n t?i

**Gi?i pháp**:
```csharp
// Verify routes exist
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
```

## Summary

? **?ã c?u hình**:
- Root URL (`/`) ? Redirect to `/Account/Login`
- Default route ? `Account/Login`
- HomeController ? Require `[Authorize]`
- Smart redirect based on authentication

?? **K?t qu?**:
- M? trang web luôn vào Login
- Không th? bypass authentication
- Clear user flow
- Secure by default

?? **URLs**:
```
https://localhost:7093/              ? /Account/Login
https://localhost:7093/Home          ? /Account/Login (if not auth)
https://localhost:7093/Account/Login ? Login page
https://localhost:7093/Admin         ? /Account/Login (if not auth)
```

?? **Security**:
- All pages require authentication
- ReturnUrl validation
- Role-based access control
- Secure cookie authentication
