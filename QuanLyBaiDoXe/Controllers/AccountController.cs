using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaiDoXe.Services;
using QuanLyBaiDoXe.ViewModels;
using System.Security.Claims;

namespace QuanLyBaiDoXe.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        #region Login

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            // N?u ?ã ??ng nh?p, redirect v? trang ch?
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Email và m?t kh?u là b?t bu?c.");
                ViewBag.ReturnUrl = returnUrl;
                return View("~/Views/Account/Login.cshtml");
            }

            // Authenticate user
            var (success, errorMessage, account, role) = await _authService.AuthenticateAsync(email, password);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "??ng nh?p th?t b?i!");
                ViewBag.ReturnUrl = returnUrl;
                return View("~/Views/Account/Login.cshtml");
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account!.MaTaiKhoan.ToString()),
                new Claim(ClaimTypes.Name, account.TenDangNhap),
                new Claim(ClaimTypes.Role, role!)
            };

            // Thêm thông tin nhân viên ho?c khách hàng
            if (account.NhanVien != null)
            {
                claims.Add(new Claim("EmployeeId", account.NhanVien.MaNhanVien.ToString()));
                claims.Add(new Claim("FullName", account.NhanVien.HoTen));
                claims.Add(new Claim("Position", account.NhanVien.ChucVu?.ToString() ?? "1"));
            }
            else if (account.KhachHang != null)
            {
                claims.Add(new Claim("CustomerId", account.KhachHang.MaKhachHang.ToString()));
                claims.Add(new Claim("FullName", account.KhachHang.HoTen));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Remember me
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["LoginMessage"] = $"??ng nh?p thành công! Chào m?ng {claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? account.TenDangNhap}";

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
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            // N?u ?ã ??ng nh?p, redirect v? trang ch?
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Register.cshtml", model);
            }

            // ??ng ký theo lo?i tài kho?n
            if (model.AccountType == "Employee")
            {
                // Ki?m tra các tr??ng b?t bu?c cho nhân viên
                if (string.IsNullOrEmpty(model.CCCD))
                {
                    ModelState.AddModelError("CCCD", "CCCD/CMND là b?t bu?c ??i v?i nhân viên!");
                    return View("~/Views/Account/Register.cshtml", model);
                }

                var (success, errorMessage, employeeId) = await _authService.RegisterEmployeeAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, errorMessage ?? "??ng ký th?t b?i!");
                    return View("~/Views/Account/Register.cshtml", model);
                }

                TempData["RegisterSuccess"] = "??ng ký tài kho?n nhân viên thành công! Vui lòng ??ng nh?p ?? ti?p t?c.";
            }
            else // Customer
            {
                var (success, errorMessage, customerId) = await _authService.RegisterCustomerAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, errorMessage ?? "??ng ký th?t b?i!");
                    return View("~/Views/Account/Register.cshtml", model);
                }

                TempData["RegisterSuccess"] = "??ng ký tài kho?n khách hàng thành công! Vui lòng ??ng nh?p ?? ti?p t?c.";
            }

            return RedirectToAction("Login");
        }

        // API ?? ki?m tra tên ??ng nh?p
        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new { available = false });
            }

            var exists = await _authService.UsernameExistsAsync(username);
            return Json(new { available = !exists });
        }

        // API ?? ki?m tra s? ?i?n tho?i
        [HttpGet]
        public async Task<IActionResult> CheckPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Json(new { available = false });
            }

            var exists = await _authService.PhoneNumberExistsAsync(phoneNumber);
            return Json(new { available = !exists });
        }

        // API ?? ki?m tra CCCD
        [HttpGet]
        public async Task<IActionResult> CheckCCCD(string cccd)
        {
            if (string.IsNullOrWhiteSpace(cccd))
            {
                return Json(new { available = false });
            }

            var exists = await _authService.CCCDExistsAsync(cccd);
            return Json(new { available = !exists });
        }

        #endregion

        #region Logout

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["LogoutMessage"] = "??ng xu?t thành công!";
            return RedirectToAction("Login", "Account");
        }

        #endregion

        #region Access Denied

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Account/AccessDenied.cshtml");
        }

        #endregion
    }
}