using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleShiftController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleShiftController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        // Danh sách ca làm việc hiện tại
        public async Task<IActionResult> Index()
        {
            var shifts = await _context.CaLamViecs
                .Include(c => c.MaNhanVienNavigation)
                .OrderByDescending(c => c.ThoiGianNhanCa)
                .Take(50)
                .Select(c => new ShiftViewModel
                {
                    MaCa = c.MaCa,
                    MaNhanVien = c.MaNhanVien,
                    TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                    ThoiGianNhanCa = c.ThoiGianNhanCa,
                    ThoiGianGiaoCa = c.ThoiGianGiaoCa,
                    TienDauCa = c.TienDauCa ?? 0,
                    TongTienThu = c.TongTienThu ?? 0,
                    TrangThaiCa = c.TrangThaiCa ?? 0
                })
                .ToListAsync();

            return View(shifts);
        }

        // Lịch làm việc - Xem full thời khóa biểu
        public async Task<IActionResult> Schedule(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var startDate = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
            var endDate = startDate.AddDays(6);

            var schedules = await _context.LichLamViecs
                .Include(l => l.MaNhanVienNavigation)
                .Where(l => l.NgayLamViec >= DateOnly.FromDateTime(startDate) 
                         && l.NgayLamViec <= DateOnly.FromDateTime(endDate))
                .Select(l => new ScheduleViewModel
                {
                    MaLich = l.MaLich,
                    MaNhanVien = l.MaNhanVien,
                    TenNhanVien = l.MaNhanVienNavigation != null ? l.MaNhanVienNavigation.HoTen : "N/A",
                    NgayLamViec = l.NgayLamViec,
                    CaLamViec = l.CaLamViec,
                    GhiChu = l.GhiChu
                })
                .ToListAsync();

            ViewBag.SelectedDate = selectedDate;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(schedules);
        }

        // Bảng chấm công - Tính giờ công nhân viên
        public async Task<IActionResult> TimeSheet(int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            var employees = await _context.NhanViens
                .Where(nv => nv.TrangThaiLamViec == true)
                .Select(nv => new EmployeeTimeSheetViewModel
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    ChucVu = nv.ChucVu ?? 0
                })
                .ToListAsync();

            foreach (var emp in employees)
            {
                var shifts = await _context.CaLamViecs
                    .Where(c => c.MaNhanVien == emp.MaNhanVien
                             && c.ThoiGianNhanCa.HasValue
                             && c.ThoiGianNhanCa.Value.Month == selectedMonth
                             && c.ThoiGianNhanCa.Value.Year == selectedYear)
                    .ToListAsync();

                emp.SoCaLam = shifts.Count;
                emp.TongGioLam = shifts
                    .Where(s => s.ThoiGianGiaoCa.HasValue)
                    .Sum(s => (decimal)(s.ThoiGianGiaoCa!.Value - s.ThoiGianNhanCa!.Value).TotalHours);
                emp.TongDoanhThu = shifts.Sum(s => s.TongTienThu ?? 0);
            }

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            return View(employees);
        }

        // Danh sách nhân viên
        public async Task<IActionResult> EmployeeList()
        {
            var employees = await _context.NhanViens
                .Include(nv => nv.MaTaiKhoanNavigation)
                .Select(nv => new EmployeeViewModel
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    GioiTinh = nv.GioiTinh,
                    NgaySinh = nv.NgaySinh,
                    SoDienThoai = nv.SoDienThoai,
                    DiaChi = nv.DiaChi,
                    ChucVu = nv.ChucVu ?? 0,
                    NgayVaoLam = nv.NgayVaoLam,
                    TrangThaiLamViec = nv.TrangThaiLamViec ?? false
                })
                .ToListAsync();

            return View(employees);
        }

        // API: Lấy dữ liệu thống kê ca làm việc
        [HttpGet]
        public async Task<IActionResult> GetShiftStats(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var startOfDay = selectedDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            var stats = new
            {
                TotalShifts = await _context.CaLamViecs
                    .Where(c => c.ThoiGianNhanCa >= startOfDay && c.ThoiGianNhanCa < endOfDay)
                    .CountAsync(),
                
                ActiveShifts = await _context.CaLamViecs
                    .Where(c => c.ThoiGianNhanCa >= startOfDay && c.ThoiGianNhanCa < endOfDay
                             && c.TrangThaiCa == 1)
                    .CountAsync(),
                
                CompletedShifts = await _context.CaLamViecs
                    .Where(c => c.ThoiGianNhanCa >= startOfDay && c.ThoiGianNhanCa < endOfDay
                             && c.TrangThaiCa == 2)
                    .CountAsync(),
                
                TotalRevenue = await _context.CaLamViecs
                    .Where(c => c.ThoiGianNhanCa >= startOfDay && c.ThoiGianNhanCa < endOfDay)
                    .SumAsync(c => c.TongTienThu ?? 0)
            };

            return Json(stats);
        }

        // API: Lấy chi tiết ca làm việc
        [HttpGet]
        public async Task<IActionResult> GetShiftDetail(int id)
        {
            var shift = await _context.CaLamViecs
                .Include(c => c.MaNhanVienNavigation)
                .Include(c => c.LuotGuiMaCaVaoNavigations)
                .Include(c => c.LuotGuiMaCaRaNavigations)
                .Where(c => c.MaCa == id)
                .Select(c => new
                {
                    c.MaCa,
                    c.MaNhanVien,
                    TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                    c.ThoiGianNhanCa,
                    c.ThoiGianGiaoCa,
                    c.TienDauCa,
                    c.TongTienThu,
                    c.TrangThaiCa,
                    SoXeVao = c.LuotGuiMaCaVaoNavigations.Count,
                    SoXeRa = c.LuotGuiMaCaRaNavigations.Count
                })
                .FirstOrDefaultAsync();

            if (shift == null)
            {
                return NotFound();
            }

            return Json(shift);
        }
    }
}
