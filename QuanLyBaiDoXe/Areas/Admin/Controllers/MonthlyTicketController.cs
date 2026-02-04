using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class MonthlyTicketController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public MonthlyTicketController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Lấy danh sách vé tháng
            var monthlyTickets = await _context.TheThangs
                .Include(v => v.MaKhachHangNavigation)
                .Include(v => v.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .OrderByDescending(v => v.NgayBatDau)
                .Select(v => new MonthlyTicketDto
                {
                    MaTheThang = v.MaTheThang,
                    MaKhachHang = v.MaKhachHang,
                    TenKhachHang = v.MaKhachHangNavigation != null ? v.MaKhachHangNavigation.HoTen : null,
                    SoDienThoai = v.MaKhachHangNavigation != null ? v.MaKhachHangNavigation.SoDienThoai : null,
                    // Lấy biển số từ thẻ tháng (TheThang.BienSoXe), không lấy từ khách hàng
                    BienSoXe = v.BienSoXe,
                    MaThe = v.MaThe,
                    TenLoaiXe = v.MaTheNavigation != null && v.MaTheNavigation.MaLoaiXeNavigation != null
                        ? v.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    NgayBatDau = v.NgayBatDau,
                    NgayHetHan = v.NgayHetHan,
                    SoTienDong = v.SoTienDong,
                    TrangThai = v.TrangThai,
                    SoNgayConLai = v.NgayHetHan.HasValue 
                        ? (v.NgayHetHan.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days 
                        : 0
                })
                .ToListAsync();


            // Lấy danh sách khách hàng
            var customers = await _context.KhachHangs
                .Select(k => new CustomerSelectDto
                {
                    MaKhachHang = k.MaKhachHang,
                    HoTen = k.HoTen,
                    SoDienThoai = k.SoDienThoai,
                    BienSoXeMacDinh = k.BienSoXeMacDinh
                })
                .ToListAsync();

            // Lấy danh sách thẻ tháng còn trống (LoaiThe = 1: vé tháng, TrangThai = 1: hoạt động)
            var usedCardIds = await _context.TheThangs
                .Where(v => v.TrangThai == true)
                .Select(v => v.MaThe)
                .ToListAsync();

            var availableCards = await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .Where(t => t.LoaiThe == 1 && t.TrangThai == 1 && !usedCardIds.Contains(t.MaThe))
                .Select(t => new CardSelectDto
                {
                    MaThe = t.MaThe,
                    TenLoaiXe = t.MaLoaiXeNavigation != null ? t.MaLoaiXeNavigation.TenLoaiXe : null,
                    MaLoaiXe = t.MaLoaiXe,
                    GiaThang = t.MaLoaiXeNavigation != null ? t.MaLoaiXeNavigation.GiaThang : null
                })
                .ToListAsync();

            // Lấy danh sách loại xe
            var vehicleTypes = await _context.LoaiXes
                .Select(l => new VehicleTypeSelectDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe
                })
                .ToListAsync();

            // Thống kê
            var statistics = new MonthlyTicketStatistics
            {
                TotalTickets = monthlyTickets.Count,
                ActiveTickets = monthlyTickets.Count(t => t.TrangThai == true && t.SoNgayConLai > 0),
                ExpiringTickets = monthlyTickets.Count(t => t.TrangThai == true && t.SoNgayConLai > 0 && t.SoNgayConLai <= 7),
                ExpiredTickets = monthlyTickets.Count(t => t.TrangThai == false || t.SoNgayConLai <= 0),
                TotalRevenue = monthlyTickets.Sum(t => t.SoTienDong ?? 0)
            };

            var model = new MonthlyTicketViewModel
            {
                MonthlyTickets = monthlyTickets,
                Customers = customers,
                AvailableCards = availableCards,
                VehicleTypes = vehicleTypes,
                Statistics = statistics
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyTickets()
        {
            var monthlyTickets = await _context.TheThangs
                .Include(v => v.MaKhachHangNavigation)
                .Include(v => v.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .OrderByDescending(v => v.NgayBatDau)
                .Select(v => new MonthlyTicketDto
                {
                    MaTheThang = v.MaTheThang,
                    MaKhachHang = v.MaKhachHang,
                    TenKhachHang = v.MaKhachHangNavigation != null ? v.MaKhachHangNavigation.HoTen : null,
                    SoDienThoai = v.MaKhachHangNavigation != null ? v.MaKhachHangNavigation.SoDienThoai : null,
                    // Lấy biển số từ thẻ tháng (TheThang.BienSoXe)
                    BienSoXe = v.BienSoXe,
                    MaThe = v.MaThe,
                    TenLoaiXe = v.MaTheNavigation != null && v.MaTheNavigation.MaLoaiXeNavigation != null
                        ? v.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    NgayBatDau = v.NgayBatDau,
                    NgayHetHan = v.NgayHetHan,
                    SoTienDong = v.SoTienDong,
                    TrangThai = v.TrangThai,
                    SoNgayConLai = v.NgayHetHan.HasValue
                        ? (v.NgayHetHan.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days
                        : 0
                })
                .ToListAsync();

            return Json(new { data = monthlyTickets });
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyTicket(int id)
        {
            var ticket = await _context.TheThangs
                .Include(v => v.MaKhachHangNavigation)
                .Include(v => v.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(v => v.MaTheThang == id)
                .Select(v => new MonthlyTicketDto
                {
                    MaTheThang = v.MaTheThang,
                    MaKhachHang = v.MaKhachHang,
                    TenKhachHang = v.MaKhachHangNavigation != null ? v.MaKhachHangNavigation.HoTen : null,
                    SoDienThoai = v.MaKhachHangNavigation != null ? v.MaKhachHangNavigation.SoDienThoai : null,
                    // Lấy biển số từ thẻ tháng (TheThang.BienSoXe)
                    BienSoXe = v.BienSoXe,
                    MaThe = v.MaThe,
                    TenLoaiXe = v.MaTheNavigation != null && v.MaTheNavigation.MaLoaiXeNavigation != null
                        ? v.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    GiaThang = v.MaTheNavigation != null && v.MaTheNavigation.MaLoaiXeNavigation != null
                        ? v.MaTheNavigation.MaLoaiXeNavigation.GiaThang : null,
                    NgayBatDau = v.NgayBatDau,
                    NgayHetHan = v.NgayHetHan,
                    SoTienDong = v.SoTienDong,
                    TrangThai = v.TrangThai,
                    SoNgayConLai = v.NgayHetHan.HasValue
                        ? (v.NgayHetHan.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days
                        : 0
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy vé tháng" });
            }

            return Json(new { success = true, data = ticket });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.KhachHangs
                .Select(k => new CustomerSelectDto
                {
                    MaKhachHang = k.MaKhachHang,
                    HoTen = k.HoTen,
                    SoDienThoai = k.SoDienThoai,
                    BienSoXeMacDinh = k.BienSoXeMacDinh
                })
                .ToListAsync();

            return Json(new { success = true, data = customers });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableCards()
        {
            var usedCardIds = await _context.TheThangs
                .Where(v => v.TrangThai == true)
                .Select(v => v.MaThe)
                .ToListAsync();

            var availableCards = await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .Where(t => t.LoaiThe == 1 && t.TrangThai == 1 && !usedCardIds.Contains(t.MaThe))
                .Select(t => new CardSelectDto
                {
                    MaThe = t.MaThe,
                    TenLoaiXe = t.MaLoaiXeNavigation != null ? t.MaLoaiXeNavigation.TenLoaiXe : null,
                    MaLoaiXe = t.MaLoaiXe
                })
                .ToListAsync();

            return Json(new { success = true, data = availableCards });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMonthlyTicketRequest request)
        {
            try
            {
                // Validate
                if (request.MaKhachHang <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn khách hàng!" });
                }

                if (string.IsNullOrEmpty(request.MaThe))
                {
                    return Json(new { success = false, message = "Vui lòng chọn thẻ!" });
                }

                if (request.SoThang <= 0)
                {
                    return Json(new { success = false, message = "Số tháng phải lớn hơn 0!" });
                }

                // Kiểm tra thẻ đã được sử dụng chưa
                var existingTicket = await _context.TheThangs
                    .AnyAsync(v => v.MaThe == request.MaThe && v.TrangThai == true);

                if (existingTicket)
                {
                    return Json(new { success = false, message = "Thẻ này đã được đăng ký vé tháng!" });
                }

                // Kiểm tra khách hàng
                var customer = await _context.KhachHangs.FindAsync(request.MaKhachHang);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng!" });
                }

                // Chuẩn hóa biển số xe
                var bienSoXe = !string.IsNullOrEmpty(request.BienSoXe) 
                    ? request.BienSoXe.Trim().ToUpper() 
                    : null;

                // Cập nhật biển số xe mặc định cho khách hàng nếu chưa có
                if (!string.IsNullOrEmpty(bienSoXe) && string.IsNullOrEmpty(customer.BienSoXeMacDinh))
                {
                    customer.BienSoXeMacDinh = bienSoXe;
                }

                var today = DateOnly.FromDateTime(DateTime.Today);
                var TheThang = new TheThang
                {
                    MaKhachHang = request.MaKhachHang,
                    MaThe = request.MaThe,
                    // Lưu biển số xe vào thẻ tháng
                    BienSoXe = bienSoXe,
                    NgayBatDau = today,
                    NgayHetHan = today.AddMonths(request.SoThang),
                    SoTienDong = request.SoTienDong,
                    TrangThai = true
                };

                _context.TheThangs.Add(TheThang);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đăng ký vé tháng thành công!" });
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
                if (request.MaTheThang <= 0)
                {
                    return Json(new { success = false, message = "Mã vé tháng không hợp lệ!" });
                }

                if (request.SoThang <= 0)
                {
                    return Json(new { success = false, message = "Số tháng phải lớn hơn 0!" });
                }

                var TheThang = await _context.TheThangs.FindAsync(request.MaTheThang);
                if (TheThang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vé tháng!" });
                }

                // Tính ngày gia hạn
                var today = DateOnly.FromDateTime(DateTime.Today);
                var startDate = TheThang.NgayHetHan.HasValue && TheThang.NgayHetHan.Value > today
                    ? TheThang.NgayHetHan.Value
                    : today;

                TheThang.NgayHetHan = startDate.AddMonths(request.SoThang);
                TheThang.SoTienDong = (TheThang.SoTienDong ?? 0) + request.SoTienDong;
                TheThang.TrangThai = true;

                // Lưu lịch sử gia hạn
                var lichSu = new LichSuGiaHanThe
                {
                    MaTheThang = TheThang.MaTheThang,
                    NgayGiaHan = DateTime.Now,
                    ThoiHanCu = startDate,
                    ThoiHanMoi = TheThang.NgayHetHan,
                    SoTien = request.SoTienDong
                };

                _context.LichSuGiaHanThes.Add(lichSu);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Gia hạn vé tháng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var TheThang = await _context.TheThangs.FindAsync(id);
                if (TheThang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vé tháng!" });
                }

                TheThang.TrangThai = false;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Hủy vé tháng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var TheThang = await _context.TheThangs
                    .Include(v => v.LichSuGiaHanThes)
                    .FirstOrDefaultAsync(v => v.MaTheThang == id);

                if (TheThang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vé tháng!" });
                }

                // Xóa lịch sử gia hạn trước
                _context.LichSuGiaHanThes.RemoveRange(TheThang.LichSuGiaHanThes);
                _context.TheThangs.Remove(TheThang);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa vé tháng thành công!" });
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
                .Where(l => l.MaTheThang == id)
                .OrderByDescending(l => l.NgayGiaHan)
                .Select(l => new
                {
                    l.MaGiaHan,
                    l.NgayGiaHan,
                    l.ThoiHanCu,
                    l.ThoiHanMoi,
                    l.SoTien
                })
                .ToListAsync();

            return Json(new { success = true, data = history });
        }
    }
}
