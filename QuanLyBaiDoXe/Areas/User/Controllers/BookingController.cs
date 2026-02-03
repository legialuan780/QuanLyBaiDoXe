using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
using System.Security.Claims;

namespace QuanLyBaiDoXe.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Customer")]
    public class BookingController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public BookingController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
            var bookings = await _context.DatChos
                .Include(dc => dc.MaViTriNavigation)
                    .ThenInclude(vt => vt.MaKhuVucNavigation)
                .Where(dc => dc.MaKhachHang == customerId)
                .OrderByDescending(dc => dc.ThoiGianDat)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSpots()
        {
            var availableSpots = await _context.ViTriDos
                .Include(vt => vt.MaKhuVucNavigation)
                .Where(vt => vt.TrangThai == 0) // Trống
                .Select(vt => new
                {
                    maViTri = vt.MaViTri,
                    tenViTri = vt.TenViTri,
                    tenKhuVuc = vt.MaKhuVucNavigation!.TenKhuVuc,
                    trangThai = vt.TrangThai
                })
                .ToListAsync();

            return Json(new { success = true, data = availableSpots });
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");

                // Kiểm tra vị trí có sẵn không
                var viTri = await _context.ViTriDos.FindAsync(request.MaViTri);
                if (viTri == null || viTri.TrangThai != 0)
                {
                    return Json(new { success = false, message = "Vị trí không khả dụng!" });
                }

                // Tạo đặt chỗ mới
                var datCho = new DatCho
                {
                    MaKhachHang = customerId,
                    MaViTri = request.MaViTri,
                    ThoiGianDat = DateTime.Now,
                    ThoiGianDenDuKien = request.ThoiGianDenDuKien,
                    ThoiGianHetHan = request.ThoiGianDenDuKien.AddMinutes(30),
                    TrangThaiDatCho = 0 // Pending - Chờ admin duyệt
                };

                _context.DatChos.Add(datCho);

                // Cập nhật trạng thái vị trí
                viTri.TrangThai = 2; // Đã đặt

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đặt chỗ thành công! Chờ admin duyệt." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
                var booking = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == id && dc.MaKhachHang == customerId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt chỗ!" });
                }

                if (booking.TrangThaiDatCho != 0)
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đặt chỗ đang chờ duyệt!" });
                }

                // Cập nhật trạng thái
                booking.TrangThaiDatCho = 2; // Cancelled

                // Giải phóng vị trí
                if (booking.MaViTriNavigation != null)
                {
                    booking.MaViTriNavigation.TrangThai = 0; // Trống
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Hủy đặt chỗ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    public class CreateBookingRequest
    {
        public int MaViTri { get; set; }
        public DateTime ThoiGianDenDuKien { get; set; }
    }
}
