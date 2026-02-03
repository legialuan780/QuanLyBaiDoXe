using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using System.Security.Claims;

namespace QuanLyBaiDoXe.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Customer")]
    public class DashboardController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public DashboardController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
            var customer = await _context.KhachHangs
                .Include(k => k.MaTaiKhoanNavigation)
                .Include(k => k.TheThangs)
                    .ThenInclude(tt => tt.MaTheNavigation)
                        .ThenInclude(t => t.MaLoaiXeNavigation)
                .Include(k => k.DatChos)
                    .ThenInclude(dc => dc.MaViTriNavigation)
                .FirstOrDefaultAsync(k => k.MaKhachHang == customerId);

            if (customer == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            ViewBag.Customer = customer;
            
            // Lấy thống kê
            var activeMonthlyTickets = customer.TheThangs
                .Where(tt => tt.TrangThai == true && tt.NgayHetHan >= DateOnly.FromDateTime(DateTime.Now))
                .Count();

            var pendingBookings = customer.DatChos
                .Where(dc => dc.TrangThaiDatCho == 0)
                .Count();

            ViewBag.ActiveMonthlyTickets = activeMonthlyTickets;
            ViewBag.PendingBookings = pendingBookings;

            return View();
        }
    }
}
