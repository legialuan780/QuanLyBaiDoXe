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
    public class MonthlyTicketController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public MonthlyTicketController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
            var tickets = await _context.TheThangs
                .Include(tt => tt.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(tt => tt.MaKhachHang == customerId)
                .OrderByDescending(tt => tt.NgayBatDau)
                .ToListAsync();

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableCards()
        {
            var availableCards = await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .Where(t => t.LoaiThe == 1 && t.TrangThai == 1 && !t.TheThangs.Any(tt => tt.TrangThai == true))
                .Select(t => new
                {
                    maThe = t.MaThe,
                    tenLoaiXe = t.MaLoaiXeNavigation!.TenLoaiXe,
                    giaThang = t.MaLoaiXeNavigation.GiaThang
                })
                .ToListAsync();

            return Json(new { success = true, data = availableCards });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterMonthlyTicketRequest request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");

                // Kiểm tra thẻ có sẵn không
                var card = await _context.TheXes
                    .Include(t => t.MaLoaiXeNavigation)
                    .FirstOrDefaultAsync(t => t.MaThe == request.MaThe && t.LoaiThe == 1 && t.TrangThai == 1);

                if (card == null)
                {
                    return Json(new { success = false, message = "Thẻ không khả dụng!" });
                }

                // Kiểm tra thẻ đã được sử dụng chưa
                var existingTicket = await _context.TheThangs
                    .AnyAsync(tt => tt.MaThe == request.MaThe && tt.TrangThai == true);

                if (existingTicket)
                {
                    return Json(new { success = false, message = "Thẻ đã được đăng ký!" });
                }

                var ngayBatDau = DateOnly.FromDateTime(DateTime.Now);
                var ngayHetHan = ngayBatDau.AddMonths(request.SoThang);

                var theThang = new TheThang
                {
                    MaKhachHang = customerId,
                    MaThe = request.MaThe,
                    BienSoXe = request.BienSoXe,
                    NgayBatDau = ngayBatDau,
                    NgayHetHan = ngayHetHan,
                    SoTienDong = (card.MaLoaiXeNavigation?.GiaThang ?? 0) * request.SoThang,
                    TrangThai = true
                };

                _context.TheThangs.Add(theThang);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đăng ký thẻ tháng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Renew([FromBody] RenewMonthlyTicketRequest request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
                var ticket = await _context.TheThangs
                    .Include(tt => tt.MaTheNavigation)
                        .ThenInclude(t => t!.MaLoaiXeNavigation)
                    .FirstOrDefaultAsync(tt => tt.MaTheThang == request.MaTheThang && tt.MaKhachHang == customerId);

                if (ticket == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thẻ tháng!" });
                }

                if (ticket.TrangThai != true)
                {
                    return Json(new { success = false, message = "Thẻ tháng đã bị hủy!" });
                }

                // Lưu lịch sử gia hạn
                var lichSuGiaHan = new LichSuGiaHanThe
                {
                    MaTheThang = ticket.MaTheThang,
                    NgayGiaHan = DateTime.Now,
                    ThoiHanCu = ticket.NgayHetHan,
                    ThoiHanMoi = ticket.NgayHetHan?.AddMonths(request.SoThang),
                    SoTien = (ticket.MaTheNavigation?.MaLoaiXeNavigation?.GiaThang ?? 0) * request.SoThang
                };

                _context.LichSuGiaHanThes.Add(lichSuGiaHan);

                // Cập nhật ngày hết hạn
                if (ticket.NgayHetHan.HasValue)
                {
                    ticket.NgayHetHan = ticket.NgayHetHan.Value.AddMonths(request.SoThang);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Gia hạn thẻ tháng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRenewalHistory(int id)
        {
            var history = await _context.LichSuGiaHanThes
                .Where(lsgh => lsgh.MaTheThang == id)
                .OrderByDescending(lsgh => lsgh.NgayGiaHan)
                .Select(lsgh => new
                {
                    ngayGiaHan = lsgh.NgayGiaHan,
                    thoiHanCu = lsgh.ThoiHanCu,
                    thoiHanMoi = lsgh.ThoiHanMoi,
                    soTien = lsgh.SoTien
                })
                .ToListAsync();

            return Json(new { success = true, data = history });
        }
    }

    public class RegisterMonthlyTicketRequest
    {
        public string MaThe { get; set; } = null!;
        public int SoThang { get; set; }
        public string? BienSoXe { get; set; }
    }

    public class RenewMonthlyTicketRequest
    {
        public int MaTheThang { get; set; }
        public int SoThang { get; set; }
    }
}
