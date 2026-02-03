using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using System.Security.Claims;

namespace QuanLyBaiDoXe.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Customer")]
    public class ProfileController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public ProfileController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
            var customer = await _context.KhachHangs
                .Include(k => k.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(k => k.MaKhachHang == customerId);

            if (customer == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
                var customer = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaKhachHang == customerId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng!" });
                }

                customer.HoTen = request.HoTen.Trim();
                customer.SoDienThoai = request.SoDienThoai.Trim();
                customer.DiaChi = string.IsNullOrWhiteSpace(request.DiaChi) ? null : request.DiaChi.Trim();
                customer.BienSoXeMacDinh = string.IsNullOrWhiteSpace(request.BienSoXeMacDinh) ? null : request.BienSoXeMacDinh.Trim().ToUpper();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCCCD([FromBody] UpdateCCCDRequest request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
                var customer = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaKhachHang == customerId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng!" });
                }

                // Kiểm tra CCCD đã tồn tại chưa
                var cccdExists = await _context.KhachHangs
                    .AnyAsync(k => k.Cccd == request.Cccd && k.MaKhachHang != customerId);

                if (cccdExists)
                {
                    return Json(new { success = false, message = "CCCD đã được sử dụng bởi tài khoản khác!" });
                }

                customer.Cccd = request.Cccd.Trim();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật CCCD thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var account = await _context.TaiKhoans.FindAsync(accountId);

                if (account == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
                }

                // Kiểm tra mật khẩu cũ
                if (account.MatKhau != request.OldPassword)
                {
                    return Json(new { success = false, message = "Mật khẩu cũ không đúng!" });
                }

                // Cập nhật mật khẩu mới
                account.MatKhau = request.NewPassword;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    public class UpdateProfileRequest
    {
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? DiaChi { get; set; }
        public string? BienSoXeMacDinh { get; set; }
    }

    public class UpdateCCCDRequest
    {
        public string Cccd { get; set; } = null!;
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
