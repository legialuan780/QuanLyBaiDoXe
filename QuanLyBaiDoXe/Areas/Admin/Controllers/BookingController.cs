using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // CHỈ ADMIN mới truy cập được
    public class BookingController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public BookingController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? status, int page = 1, int pageSize = 20)
        {
            var query = _context.DatChos
                .Include(dc => dc.MaKhachHangNavigation)
                .Include(dc => dc.MaViTriNavigation)
                    .ThenInclude(vt => vt!.MaKhuVucNavigation)
                .AsQueryable();

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(dc => dc.TrangThaiDatCho == status.Value);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var bookings = await query
                .OrderByDescending(dc => dc.ThoiGianDat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.StatusFilter = status;

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var pending = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 0);
            var approved = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 1);
            var cancelled = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 2);
            var expired = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 3);

            return Json(new
            {
                success = true,
                pending,
                approved,
                cancelled,
                expired,
                total = pending + approved + cancelled + expired
            });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var booking = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == id);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu đặt chỗ!" });
                }

                if (booking.TrangThaiDatCho != 0)
                {
                    return Json(new { success = false, message = "Yêu cầu đã được xử lý trước đó!" });
                }

                // Check if spot is still available
                if (booking.MaViTriNavigation != null && booking.MaViTriNavigation.TrangThai != 2)
                {
                    return Json(new { success = false, message = "Vị trí không còn ở trạng thái đã đặt!" });
                }

                // Approve booking
                booking.TrangThaiDatCho = 1; // Approved

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Duyệt đặt chỗ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                var booking = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == id);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu đặt chỗ!" });
                }

                if (booking.TrangThaiDatCho != 0)
                {
                    return Json(new { success = false, message = "Yêu cầu đã được xử lý trước đó!" });
                }

                // Reject booking
                booking.TrangThaiDatCho = 2; // Cancelled

                // Free the spot
                if (booking.MaViTriNavigation != null)
                {
                    booking.MaViTriNavigation.TrangThai = 0; // Available
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã từ chối yêu cầu đặt chỗ!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var booking = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == id);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt chỗ!" });
                }

                // Free the spot if it's still reserved
                if (booking.MaViTriNavigation != null && booking.MaViTriNavigation.TrangThai == 2)
                {
                    booking.MaViTriNavigation.TrangThai = 0; // Available
                }

                _context.DatChos.Remove(booking);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa đặt chỗ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReleaseExpired()
        {
            try
            {
                // Find expired bookings
                var expiredBookings = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .Where(dc => dc.TrangThaiDatCho == 0 && dc.ThoiGianHetHan < DateTime.Now)
                    .ToListAsync();

                foreach (var booking in expiredBookings)
                {
                    booking.TrangThaiDatCho = 3; // Expired

                    // Free the spot
                    if (booking.MaViTriNavigation != null)
                    {
                        booking.MaViTriNavigation.TrangThai = 0; // Available
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã giải phóng {expiredBookings.Count} đặt chỗ hết hạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingDetail(int id)
        {
            var booking = await _context.DatChos
                .Include(dc => dc.MaKhachHangNavigation)
                    .ThenInclude(kh => kh!.MaTaiKhoanNavigation)
                .Include(dc => dc.MaViTriNavigation)
                    .ThenInclude(vt => vt!.MaKhuVucNavigation)
                .FirstOrDefaultAsync(dc => dc.MaDatCho == id);

            if (booking == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đặt chỗ!" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    maDatCho = booking.MaDatCho,
                    maKhachHang = booking.MaKhachHang,
                    tenKhachHang = booking.MaKhachHangNavigation?.HoTen,
                    soDienThoai = booking.MaKhachHangNavigation?.SoDienThoai,
                    cccd = booking.MaKhachHangNavigation?.Cccd,
                    bienSoXe = booking.MaKhachHangNavigation?.BienSoXeMacDinh,
                    tenViTri = booking.MaViTriNavigation?.TenViTri,
                    tenKhuVuc = booking.MaViTriNavigation?.MaKhuVucNavigation?.TenKhuVuc,
                    thoiGianDat = booking.ThoiGianDat,
                    thoiGianDenDuKien = booking.ThoiGianDenDuKien,
                    thoiGianHetHan = booking.ThoiGianHetHan,
                    trangThaiDatCho = booking.TrangThaiDatCho
                }
            });
        }
    }
}
