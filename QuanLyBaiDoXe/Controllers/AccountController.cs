using Microsoft.AspNetCore.Mvc;

namespace QuanLyBaiDoXe.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Email và mật khẩu là bắt buộc.");
                return View("~/Views/Account/Login.cshtml");
            }
            TempData["LoginMessage"] = "Đăng nhập demo thành công.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        public IActionResult Register(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                return View("~/Views/Account/Register.cshtml");
            }
            TempData["RegisterMessage"] = "Đăng ký demo thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}