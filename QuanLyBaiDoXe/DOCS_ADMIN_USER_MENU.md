# H??ng d?n s? d?ng User Menu và Logout trong Admin Panel

## T?ng quan
?ã thêm menu ng??i dùng vào header c?a Admin Panel v?i các tính n?ng:
- ? Hi?n th? thông tin ng??i dùng ?ang ??ng nh?p
- ? Avatar v?i ch? cái ??u c?a tên
- ? Dropdown menu v?i các tùy ch?n
- ? Nút ??ng xu?t

## Tính n?ng

### 1. Hi?n th? thông tin ng??i dùng

#### Thông tin hi?n th?
```
???????????????????????????
?  [A]  Nguy?n V?n A     ??
?       Qu?n tr? viên      ?
???????????????????????????
```

**D? li?u ???c l?y t? Claims**:
- `FullName`: H? tên ??y ?? (t? NhanVien.HoTen ho?c KhachHang.HoTen)
- `Role`: Admin, Employee, ho?c Customer
- Avatar: Ch? cái ??u tiên c?a h? tên

### 2. Dropdown Menu

Khi click vào thông tin ng??i dùng, dropdown menu s? hi?n v?i các m?c:

```
?????????????????????????????
?  Nguy?n V?n A            ?
?  @admin                  ?
?????????????????????????????
?  ?? Thông tin cá nhân    ?
?  ??  Cài ??t              ?
?  ?? ??i m?t kh?u          ?
?????????????????????????????
?  ?? ??ng xu?t            ?
?????????????????????????????
```

#### Các menu item:
1. **Thông tin cá nhân**: Xem/s?a profile (tính n?ng t??ng lai)
2. **Cài ??t**: C?u hình cá nhân (tính n?ng t??ng lai)
3. **??i m?t kh?u**: Thay ??i m?t kh?u (tính n?ng t??ng lai)
4. **??ng xu?t**: Thoát kh?i h? th?ng ?

### 3. Ch?c n?ng ??ng xu?t

#### Flow ??ng xu?t
```
1. Click vào "??ng xu?t"
   ?
2. Submit POST form ? /Account/Logout
   ?
3. AccountController.Logout()
   ?
4. SignOutAsync() - Clear authentication cookie
   ?
5. Set TempData["LogoutMessage"]
   ?
6. Redirect v? /Account/Login
   ?
7. Hi?n th? thông báo "??ng xu?t thành công!"
```

#### Routing
```html
<!-- Trong _AdminLayout.cshtml -->
<form action="/Account/Logout" method="post">
    @Html.AntiForgeryToken()
    <button type="submit">??ng xu?t</button>
</form>
```

**L?u ý**: 
- ? S? d?ng absolute path `/Account/Logout`
- ? KHÔNG s? d?ng `asp-controller/asp-action` trong Admin area
- ? Tránh l?i routing `/Admin/Account/Logout` (không t?n t?i)

#### Code flow
```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    TempData["LogoutMessage"] = "??ng xu?t thành công!";
    return RedirectToAction("Login", "Account");
}
```

## Giao di?n

### Header Layout
```
???????????????????????????????????????????????????????????????
?  [?]  [?? Tìm ki?m...]         [??5] [??2]  [Avatar Menu]  ?
???????????????????????????????????????????????????????????????
```

### Avatar Styling
- **Shape**: Circular
- **Size**: 40x40px
- **Background**: Linear gradient (purple to pink)
- **Content**: Ch? cái ??u tiên c?a tên
- **Font**: Bold, white color

### Dropdown Styling
- **Position**: Absolute, top-right aligned
- **Width**: 220px minimum
- **Shadow**: Soft shadow for depth
- **Animation**: Smooth fade in/out
- **Border**: Rounded corners (8px)

## CSS Classes

### Main Classes
```css
.header-user                    // Container
.header-user-avatar            // Avatar circle
.header-user-info              // Name + role container
.header-user-name              // Full name
.header-user-role              // Role text
.header-user-dropdown          // Dropdown menu
.dropdown-item-custom          // Menu items
.dropdown-item-logout          // Logout button (red)
```

### States
```css
.header-user:hover             // Hover effect
.header-user.show              // Dropdown visible
.dropdown-item-custom:hover    // Item hover
```

## JavaScript Behavior

### Toggle Dropdown
```javascript
$('#headerUserDropdown').on('click', function(e) {
    e.stopPropagation();
    $(this).toggleClass('show');
});
```

### Close on Outside Click
```javascript
$(document).on('click', function(e) {
    if (!$(e.target).closest('#headerUserDropdown').length) {
        $('#headerUserDropdown').removeClass('show');
    }
});
```

## Claims Usage

### L?y thông tin t? User Claims
```razor
@{
    var fullName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "User";
    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";
    var roleName = role == "Admin" ? "Qu?n tr? viên" 
                 : (role == "Employee" ? "Nhân viên" : "Ng??i dùng");
    var username = User.Identity?.Name ?? "";
    var firstLetter = !string.IsNullOrEmpty(fullName) 
                     ? fullName.Substring(0, 1).ToUpper() : "?";
}
```

### Claims ???c s? d?ng
| Claim Type | Mô t? | Ví d? |
|------------|-------|-------|
| `FullName` | H? tên ??y ?? | "Nguy?n V?n A" |
| `ClaimTypes.Role` | Vai trò | "Admin", "Employee", "Customer" |
| `ClaimTypes.Name` | Username | "admin" |
| `EmployeeId` | Mã nhân viên (n?u có) | "1" |
| `CustomerId` | Mã khách hàng (n?u có) | "5" |

## Testing

### Test Case 1: Hi?n th? thông tin Admin
```
Login as: admin / admin123

Expected:
- Avatar: "Q" (t? "Qu?n tr? viên")
- Name: "Qu?n Tr? Viên"
- Role: "Qu?n tr? viên"
```

### Test Case 2: Hi?n th? thông tin Nhân viên
```
Login as: nhanvien / nv123

Expected:
- Avatar: "N" (t? "Nguy?n...")
- Name: "Nguy?n V?n A"
- Role: "Nhân viên"
```

### Test Case 3: Toggle Dropdown
```
Action: Click vào user menu

Expected:
- Dropdown hi?n ra
- Background hover effect
- Click outside ? Dropdown ?n
```

### Test Case 4: ??ng xu?t t? Admin
```
URL: https://localhost:7093/Admin/Dashboard
Action: Click "??ng xu?t"

Expected:
1. POST request to /Account/Logout (NOT /Admin/Account/Logout)
2. Cookie cleared
3. Redirect to /Account/Login
4. Message: "??ng xu?t thành công!"
5. Không th? truy c?p /Admin/* n?a
```

### Test Case 5: Routing Check
```
CORRECT ?:
- Form action="/Account/Logout"
- Routes to AccountController.Logout()

INCORRECT ?:
- Form asp-controller="Account" asp-action="Logout" (in Admin area)
- Routes to /Admin/Account/Logout (404 error)
```

## Routing Details

### Admin Area Routing Issue

**Problem**:
```razor
<!-- WRONG in Admin area -->
<form asp-controller="Account" asp-action="Logout" method="post">

<!-- This tries to find Admin/AccountController -->
<!-- URL: /Admin/Account/Logout ? -->
</form>
```

**Solution**:
```razor
<!-- CORRECT -->
<form action="/Account/Logout" method="post">
    <!-- Direct path to root AccountController -->
    <!-- URL: /Account/Logout ? -->
</form>
```

### Why This Happens

1. **Admin Layout** n?m trong `/Areas/Admin/Views/Shared/`
2. Khi dùng `asp-controller`, ASP.NET Core tìm trong **current area** (Admin)
3. `AccountController` n?m ? **root** (`/Controllers/`), không ph?i Admin area
4. ? 404 error vì không có `/Areas/Admin/Controllers/AccountController.cs`

### Alternative Solutions

#### Option 1: Absolute Path (Current) ?
```razor
<form action="/Account/Logout" method="post">
```

#### Option 2: Clear Area
```razor
<form asp-area="" asp-controller="Account" asp-action="Logout" method="post">
    <!-- asp-area="" tells it to use root controllers -->
</form>
```

#### Option 3: Create Admin AccountController
```csharp
// Areas/Admin/Controllers/AccountController.cs
namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Logout() { }
    }
}
```

## Customization

### 1. Thay ??i màu Avatar
```css
.header-user-avatar {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    /* Ho?c */
    background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
    /* Ho?c */
    background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
}
```

### 2. Thêm menu item m?i
```razor
<a href="#" class="dropdown-item-custom">
    <i class="fas fa-question-circle"></i>
    <span>Tr? giúp</span>
</a>
```

### 3. Thay ??i role display
```csharp
var roleName = role switch
{
    "Admin" => "Qu?n tr? viên",
    "Employee" => "Nhân viên",
    "Customer" => "Khách hàng",
    _ => "Ng??i dùng"
};
```

## Security

### 1. Authorization
```csharp
[HttpPost]
[Authorize]  // Ph?i ??ng nh?p m?i ???c logout
public async Task<IActionResult> Logout()
```

### 2. Anti-Forgery Token
```razor
<form action="/Account/Logout" method="post">
    @Html.AntiForgeryToken()  // CSRF protection
    <button type="submit">??ng xu?t</button>
</form>
```

### 3. Cookie Clear
```csharp
await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
```

## Troubleshooting

### Issue 1: 404 Error - /Admin/Account/Logout

**Tri?u ch?ng**:
```
URL: https://localhost:7093/Admin/Account/Logout
Status: 404 Not Found
```

**Nguyên nhân**: 
- Dùng `asp-controller/asp-action` trong Admin area view
- ASP.NET tìm AccountController trong Admin area
- AccountController ch? có ? root

**Gi?i pháp**:
```razor
<!-- ??i t? -->
<form asp-controller="Account" asp-action="Logout" method="post">

<!-- Sang -->
<form action="/Account/Logout" method="post">
```

### Issue 2: Avatar hi?n th? "?"
**Nguyên nhân**: FullName claim không t?n t?i

**Gi?i pháp**:
```csharp
// Khi login, ??m b?o add claim
claims.Add(new Claim("FullName", account.NhanVien.HoTen));
```

### Issue 3: Role hi?n th? "Unknown"
**Nguyên nhân**: Role claim không ?úng

**Gi?i pháp**:
```csharp
// Check role value
var role = account.NhanVien.ChucVu == 0 ? "Admin" : "Employee";
claims.Add(new Claim(ClaimTypes.Role, role));
```

### Issue 4: Dropdown không ?óng
**Nguyên nhân**: JavaScript conflict

**Gi?i pháp**:
- Check jQuery loaded
- Verify no console errors
- Ensure event handlers attached

### Issue 5: Logout không ho?t ??ng
**Nguyên nhân**: Form submit ho?c routing issue

**Gi?i pháp**:
```csharp
// Verify route exists
[HttpPost]
[Route("Account/Logout")]
public async Task<IActionResult> Logout()
```

## Browser Compatibility

| Browser | Version | Support |
|---------|---------|---------|
| Chrome | 90+ | ? Full |
| Firefox | 88+ | ? Full |
| Safari | 14+ | ? Full |
| Edge | 90+ | ? Full |

## Responsive Design

### Desktop (>992px)
- Full user info displayed
- Avatar + Name + Role
- Dropdown aligned right

### Tablet (768px - 992px)
- Avatar + Name only
- Role hidden
- Dropdown same

### Mobile (<768px)
- Avatar only
- Click ? Show full info in dropdown

## Performance

- **JavaScript**: Minimal, event delegation
- **CSS**: No heavy animations
- **Rendering**: No layout shifts
- **Network**: No additional API calls

## Future Enhancements

### Planned Features
1. ? Basic logout
2. ?? Profile page
3. ?? Change password
4. ?? Settings page
5. ?? Upload avatar image
6. ?? Theme switcher (dark mode)
7. ?? Language switcher

### API Integration
```csharp
// Profile endpoint
[HttpGet]
public IActionResult Profile() { }

// Change password endpoint
[HttpPost]
public IActionResult ChangePassword(ChangePasswordViewModel model) { }
```

## Summary

? **?ã implement**:
- Hi?n th? thông tin user t? Claims
- Avatar ??ng v?i ch? cái ??u
- Dropdown menu v?i animations
- Logout functionality
- CSRF protection
- Responsive design
- **Fixed routing issue v?i absolute path**

?? **S? d?ng**:
1. ??ng nh?p vào h? th?ng
2. Truy c?p Admin panel (https://localhost:7093/Admin)
3. Click vào avatar/tên ? góc ph?i
4. Ch?n "??ng xu?t"
5. POST to /Account/Logout (NOT /Admin/Account/Logout)
6. ???c redirect v? trang login

?? **Security**:
- Authorization required
- Anti-forgery token
- Cookie authentication
- Secure logout flow

?? **L?u ý quan tr?ng**:
- Luôn dùng absolute path `/Account/Logout` trong Admin area
- Ho?c dùng `asp-area=""` ?? clear area context
- KHÔNG dùng `asp-controller/asp-action` tr?c ti?p trong Admin views
