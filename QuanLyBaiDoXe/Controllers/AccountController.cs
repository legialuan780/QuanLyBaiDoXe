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
        private readonly IEmailService _emailService;

        public AccountController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        #region Login

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            // Nếu đã đăng nhập, redirect về Dashboard tùy role
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                
                // Admin và Employee redirect về Admin Dashboard
                if (role == "Admin" || role == "Employee")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                
                // Customer redirect về User Dashboard
                return RedirectToAction("Index", "Dashboard", new { area = "User" });
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
                ModelState.AddModelError(string.Empty, "Email và mật khẩu là bắt buộc.");
                ViewBag.ReturnUrl = returnUrl;
                return View("~/Views/Account/Login.cshtml");
            }

            // Authenticate user
            var (success, errorMessage, account, role) = await _authService.AuthenticateAsync(email, password);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đăng nhập thất bại!");
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

            // Thêm thông tin nhân viên hoặc khách hàng
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

            TempData["LoginMessage"] =
                $"Đăng nhập thành công! Chào mừng {claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? account.TenDangNhap}";

            // Redirect based on role
            if (role == "Admin" || role == "Employee")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // Nếu có returnUrl và là local URL, redirect về đó
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Customer redirect về User Dashboard
            return RedirectToAction("Index", "Dashboard", new { area = "User" });
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            // Nếu đã đăng nhập, redirect về trang tương ứng với role
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                
                // Admin và Employee redirect về Admin Dashboard
                if (role == "Admin" || role == "Employee")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                
                // Customer redirect về User Dashboard
                return RedirectToAction("Index", "Dashboard", new { area = "User" });
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

            // Đăng ký tài khoản khách hàng (chỉ cho phép đăng ký quyền khách)
            var (success, errorMessage, customerId) = await _authService.RegisterCustomerAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đăng ký thất bại!");
                return View("~/Views/Account/Register.cshtml", model);
            }

            TempData["RegisterSuccess"] =
                "Đăng ký tài khoản khách hàng thành công! Vui lòng đăng nhập để tiếp tục.";

            return RedirectToAction("Login");
        }

        // API để kiểm tra tên đăng nhập
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

        // API để kiểm tra email
        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { available = false });
            }

            var exists = await _authService.EmailExistsAsync(email);
            return Json(new { available = !exists });
        }

        // API để kiểm tra số điện thoại
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

        // API để kiểm tra CCCD
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
            TempData["LogoutMessage"] = "Đăng xuất thành công!";
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

        #region Forgot Password

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("~/Views/Account/ForgotPassword.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/ForgotPassword.cshtml", model);
            }

            var (success, errorMessage, account) = await _authService.GetAccountByEmailAsync(model.Email);

            if (!success || account == null)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Email không tồn tại trong hệ thống!");
                return View("~/Views/Account/ForgotPassword.cshtml", model);
            }

            // Generate OTP
            var otpCode = await _authService.GenerateOtpAsync(account.MaTaiKhoan);

            // Get user name for email
            var userName = account.NhanVien?.HoTen ?? account.KhachHang?.HoTen ?? account.TenDangNhap;

            // Send OTP email
            var emailSent = await _emailService.SendOtpEmailAsync(model.Email, otpCode, userName);

            if (!emailSent)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.");
                return View("~/Views/Account/ForgotPassword.cshtml", model);
            }

            TempData["Email"] = model.Email;
            TempData["OtpSent"] = true;
            return RedirectToAction("VerifyOtp");
        }

        #endregion

        #region Verify OTP

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            TempData.Keep("Email");
            var model = new VerifyOtpViewModel { Email = email };
            return View("~/Views/Account/VerifyOtp.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/VerifyOtp.cshtml", model);
            }

            var (success, errorMessage) = await _authService.VerifyOtpAsync(model.Email, model.OtpCode);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Mã OTP không đúng!");
                return View("~/Views/Account/VerifyOtp.cshtml", model);
            }

            // Store email and OTP in TempData for reset password step
            TempData["Email"] = model.Email;
            TempData["OtpCode"] = model.OtpCode;
            TempData["OtpVerified"] = true;
            return RedirectToAction("ResetPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email không hợp lệ!" });
            }

            var (success, errorMessage, account) = await _authService.GetAccountByEmailAsync(email);

            if (!success || account == null)
            {
                return Json(new { success = false, message = errorMessage ?? "Email không tồn tại!" });
            }

            // Generate new OTP
            var otpCode = await _authService.GenerateOtpAsync(account.MaTaiKhoan);

            // Get user name for email
            var userName = account.NhanVien?.HoTen ?? account.KhachHang?.HoTen ?? account.TenDangNhap;

            // Send OTP email
            var emailSent = await _emailService.SendOtpEmailAsync(email, otpCode, userName);

            if (!emailSent)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi email!" });
            }

            return Json(new { success = true, message = "Mã OTP mới đã được gửi đến email của bạn!" });
        }

        #endregion

        #region Reset Password

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData["Email"]?.ToString();
            var otpCode = TempData["OtpCode"]?.ToString();
            var otpVerified = TempData["OtpVerified"] as bool?;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode) || otpVerified != true)
            {
                return RedirectToAction("ForgotPassword");
            }

            TempData.Keep("Email");
            TempData.Keep("OtpCode");
            TempData.Keep("OtpVerified");

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = otpCode
            };

            return View("~/Views/Account/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/ResetPassword.cshtml", model);
            }

            var (success, errorMessage) = await _authService.ResetPasswordWithOtpAsync(model.Email, model.Token, model.NewPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Có lỗi xảy ra!");
                return View("~/Views/Account/ResetPassword.cshtml", model);
            }

            TempData["ResetPasswordSuccess"] = "Mật khẩu đã được đặt lại thành công! Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction("Login");
        }

        #endregion
    }
}
