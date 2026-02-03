using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using System.ComponentModel.DataAnnotations;

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
                    TongTienHeThong = c.TongTienHeThong ?? 0,
                    TienMatBanGiao = c.TienMatBanGiao ?? 0,
                    GhiChuBanGiao = c.GhiChuBanGiao,
                    TrangThaiCa = c.TrangThaiCa ?? 0
                })
                .ToListAsync();

            return View(shifts);
        }

        // Lịch làm việc - Timeline view theo ngày (Đã ẩn khỏi menu)
        /*
        public async Task<IActionResult> Schedule(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            // Lấy tất cả nhân viên đang làm việc
            var allEmployees = await _context.NhanViens
                .Where(nv => nv.TrangThaiLamViec == true)
                .OrderBy(nv => nv.HoTen)
                .ToListAsync();

            // Lấy lịch làm việc của ngày được chọn và vài ngày xung quanh để hiển thị
            var startDate = selectedDate.AddDays(-1);
            var endDate = selectedDate.AddDays(1);

            var schedules = await _context.LichLamViecs
                .Include(l => l.MaNhanVienNavigation)
                .Where(l => l.NgayLamViec >= DateOnly.FromDateTime(startDate) 
                         && l.NgayLamViec <= DateOnly.FromDateTime(endDate))
                .Select(l => new ScheduleViewModel
                {
                    MaLich = l.MaLich,
                    MaNhanVien = l.MaNhanVien,
                    TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                    NgayLamViec = l.NgayLamViec,
                    CaLamViec = l.CaLamViec,
                    GhiChu = l.GhiChu
                })
                .ToListAsync();

            // Thêm tất cả nhân viên vào danh sách (kể cả người chưa có lịch)
            var currentDateOnly = DateOnly.FromDateTime(selectedDate);
            foreach (var emp in allEmployees)
            {
                // Nếu nhân viên chưa có trong schedules của ngày này, thêm một record rỗng
                if (!schedules.Any(s => s.MaNhanVien == emp.MaNhanVien && s.NgayLamViec == currentDateOnly))
                {
                    schedules.Add(new ScheduleViewModel
                    {
                        MaNhanVien = emp.MaNhanVien,
                        TenNhanVien = emp.HoTen,
                        NgayLamViec = null // Không có lịch
                    });
                }
            }

            ViewBag.SelectedDate = selectedDate;

            return View(schedules);
        }
        */

        // Bảng chấm công - Tính giờ công nhân viên
        public async Task<IActionResult> TimeSheet(int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            // Lấy tất cả ca làm việc trong tháng
            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var shifts = await _context.CaLamViecs
                .Include(c => c.MaNhanVienNavigation)
                .Where(c => c.ThoiGianNhanCa.HasValue
                         && c.ThoiGianNhanCa.Value.Date >= startDate
                         && c.ThoiGianNhanCa.Value.Date <= endDate)
                .OrderBy(c => c.ThoiGianNhanCa)
                .Select(c => new ShiftViewModel
                {
                    MaCa = c.MaCa,
                    MaNhanVien = c.MaNhanVien,
                    TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                    ThoiGianNhanCa = c.ThoiGianNhanCa,
                    ThoiGianGiaoCa = c.ThoiGianGiaoCa,
                    TienDauCa = c.TienDauCa ?? 0,
                    TongTienHeThong = c.TongTienHeThong ?? 0,
                    TienMatBanGiao = c.TienMatBanGiao ?? 0,
                    GhiChuBanGiao = c.GhiChuBanGiao,
                    TrangThaiCa = c.TrangThaiCa ?? 0
                })
                .ToListAsync();

            // Nhóm ca theo ngày
            var dailyShifts = new List<DailyShiftViewModel>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var dayShifts = shifts
                    .Where(s => s.ThoiGianNhanCa.HasValue && s.ThoiGianNhanCa.Value.Date == currentDate.Date)
                    .OrderBy(s => s.ThoiGianNhanCa)
                    .ToList();

                var dailyShift = new DailyShiftViewModel
                {
                    Date = currentDate,
                    Shifts = dayShifts
                };

                // Xác định ca hiện tại và ca tiếp theo
                var now = DateTime.Now;
                if (currentDate.Date == now.Date)
                {
                    // Ca hiện tại: ca đang diễn ra (đã bắt đầu nhưng chưa kết thúc và đang trực)
                    dailyShift.CurrentShift = dayShifts
                        .FirstOrDefault(s => s.ThoiGianNhanCa.HasValue 
                                          && s.ThoiGianNhanCa.Value <= now
                                          && s.TrangThaiCa == 0); // Đang trực

                    // Ca tiếp theo: ca chưa bắt đầu (thời gian nhận ca > hiện tại)
                    dailyShift.NextShift = dayShifts
                        .Where(s => s.ThoiGianNhanCa.HasValue && s.ThoiGianNhanCa.Value > now)
                        .OrderBy(s => s.ThoiGianNhanCa)
                        .FirstOrDefault();
                }
                else if (currentDate.Date > now.Date)
                {
                    // Ngày tương lai: ca đầu tiên là ca tiếp theo
                    dailyShift.NextShift = dayShifts.FirstOrDefault();
                }

                dailyShifts.Add(dailyShift);
                currentDate = currentDate.AddDays(1);
            }

            // Tính thống kê tháng
            var stats = new MonthStatsViewModel
            {
                TotalActiveShifts = shifts.Count(s => s.TrangThaiCa == 0),
                TotalCompletedShifts = shifts.Count(s => s.TrangThaiCa == 1),
                TotalWorkHours = shifts.Sum(s => (decimal)s.SoGioLam),
                TotalRevenue = shifts.Sum(s => s.TongTienHeThong)
            };

            var viewModel = new ShiftCalendarViewModel
            {
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear,
                MonthStats = stats
            };

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.DailyShifts = dailyShifts;

            return View(viewModel);
        }

        // API: Lấy các ca của một ngày cụ thể
        [HttpGet]
        public async Task<IActionResult> GetDayShifts(string date)
        {
            try
            {
                var selectedDate = DateTime.Parse(date);
                
                var shifts = await _context.CaLamViecs
                    .Include(c => c.MaNhanVienNavigation)
                    .Where(c => c.ThoiGianNhanCa.HasValue 
                             && c.ThoiGianNhanCa.Value.Date == selectedDate.Date)
                    .OrderBy(c => c.ThoiGianNhanCa)
                    .Select(c => new
                    {
                        c.MaCa,
                        c.MaNhanVien,
                        TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                        c.ThoiGianNhanCa,
                        c.ThoiGianGiaoCa,
                        c.TienDauCa,
                        c.TongTienHeThong,
                        c.GhiChuBanGiao,
                        c.TrangThaiCa
                    })
                    .ToListAsync();

                return Json(shifts);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Cập nhật nhiều ca trong ngày
        [HttpPost]
        public async Task<IActionResult> UpdateDayShifts([FromBody] UpdateDayShiftsRequest request)
        {
            try
            {
                if (request.Updates == null || !request.Updates.Any())
                {
                    return Json(new { success = false, message = "Không có ca nào để cập nhật" });
                }

                foreach (var update in request.Updates)
                {
                    var shift = await _context.CaLamViecs.FindAsync(update.MaCa);
                    if (shift == null) continue;

                    // Chỉ cập nhật ca chưa chốt
                    if (shift.TrangThaiCa == 0)
                    {
                        if (update.MaNhanVien.HasValue)
                        {
                            shift.MaNhanVien = update.MaNhanVien.Value;
                        }
                        shift.TienDauCa = update.TienDauCa;
                        shift.GhiChuBanGiao = update.GhiChuBanGiao;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật ca thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Xóa một ca
        [HttpPost]
        public async Task<IActionResult> DeleteShift(int shiftId)
        {
            try
            {
                var shift = await _context.CaLamViecs.FindAsync(shiftId);
                if (shift == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc" });
                }

                // Chỉ xóa ca chưa chốt
                if (shift.TrangThaiCa != 0)
                {
                    return Json(new { success = false, message = "Không thể xóa ca đã chốt" });
                }

                _context.CaLamViecs.Remove(shift);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa ca thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Xóa tất cả ca trong ngày
        [HttpPost]
        public async Task<IActionResult> DeleteDayShifts(string date)
        {
            try
            {
                var selectedDate = DateTime.Parse(date);
                
                var shifts = await _context.CaLamViecs
                    .Where(c => c.ThoiGianNhanCa.HasValue 
                             && c.ThoiGianNhanCa.Value.Date == selectedDate.Date
                             && c.TrangThaiCa == 0) // Chỉ xóa ca chưa chốt
                    .ToListAsync();

                if (!shifts.Any())
                {
                    return Json(new { success = false, message = "Không có ca nào để xóa hoặc tất cả ca đã được chốt" });
                }

                _context.CaLamViecs.RemoveRange(shifts);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã xóa {shifts.Count} ca" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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

        // Lập lịch tuần
        public async Task<IActionResult> WeeklySchedule(int? year, int? month, int? day)
        {
            // Xác định tuần bắt đầu (Thứ 2)
            DateTime selectedDate = year.HasValue && month.HasValue && day.HasValue
                ? new DateTime(year.Value, month.Value, day.Value)
                : DateTime.Today;

            // Tìm thứ 2 của tuần
            int daysToMonday = ((int)selectedDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            DateTime weekStart = selectedDate.AddDays(-daysToMonday).Date;
            DateTime weekEnd = weekStart.AddDays(6);

            // Lấy danh sách nhân viên đang làm việc
            var employees = await _context.NhanViens
                .Where(nv => nv.TrangThaiLamViec == true && (nv.ChucVu == 1 || nv.ChucVu == 2)) // Quản lý hoặc Bảo vệ
                .Select(nv => new EmployeeViewModel
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    ChucVu = nv.ChucVu ?? 0,
                    TrangThaiLamViec = nv.TrangThaiLamViec ?? false
                })
                .OrderBy(nv => nv.HoTen)
                .ToListAsync();

            // Lấy lịch đã có trong tuần
            var existingSchedules = await _context.LichLamViecs
                .Include(l => l.MaNhanVienNavigation)
                .Where(l => l.NgayLamViec >= DateOnly.FromDateTime(weekStart) 
                         && l.NgayLamViec <= DateOnly.FromDateTime(weekEnd))
                .Select(l => new ScheduleViewModel
                {
                    MaLich = l.MaLich,
                    MaNhanVien = l.MaNhanVien,
                    TenNhanVien = l.MaNhanVienNavigation != null ? l.MaNhanVienNavigation.HoTen : "N/A",
                    NgayLamViec = l.NgayLamViec,
                    CaLamViec = l.LoaiCa,
                    GhiChu = l.GhiChu
                })
                .ToListAsync();

            var viewModel = new WeeklyScheduleViewModel
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Employees = employees,
                ExistingSchedules = existingSchedules
            };

            return View(viewModel);
        }

        // Lịch làm việc hàng ngày (Timeline)
        public async Task<IActionResult> DailySchedule(int? year, int? month, int? day)
        {
            DateTime selectedDate = year.HasValue && month.HasValue && day.HasValue
                ? new DateTime(year.Value, month.Value, day.Value)
                : DateTime.Today;

            // Lấy tất cả nhân viên
            var allEmployees = await _context.NhanViens
                .Where(nv => nv.TrangThaiLamViec == true && (nv.ChucVu == 1 || nv.ChucVu == 2))
                .Select(nv => new EmployeeViewModel
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    ChucVu = nv.ChucVu ?? 0
                })
                .OrderBy(nv => nv.HoTen)
                .ToListAsync();

            // Lấy lịch đã xếp trong ngày
            var scheduledShifts = await _context.LichLamViecs
                .Include(l => l.MaNhanVienNavigation)
                .Where(l => l.NgayLamViec == DateOnly.FromDateTime(selectedDate))
                .Select(l => new ScheduleViewModel
                {
                    MaLich = l.MaLich,
                    MaNhanVien = l.MaNhanVien,
                    TenNhanVien = l.MaNhanVienNavigation != null ? l.MaNhanVienNavigation.HoTen : "N/A",
                    NgayLamViec = l.NgayLamViec,
                    CaLamViec = l.LoaiCa,
                    GhiChu = l.GhiChu
                })
                .ToListAsync();

            // Lấy các ca đang trực (CaLamViec table)
            var startOfDay = selectedDate.Date;
            var endOfDay = startOfDay.AddDays(1);
            var activeShifts = await _context.CaLamViecs
                .Include(c => c.MaNhanVienNavigation)
                .Where(c => c.ThoiGianNhanCa >= startOfDay 
                         && c.ThoiGianNhanCa < endOfDay
                         && c.TrangThaiCa == 0) // Đang trực
                .Select(c => new ShiftViewModel
                {
                    MaCa = c.MaCa,
                    MaNhanVien = c.MaNhanVien,
                    TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                    ThoiGianNhanCa = c.ThoiGianNhanCa,
                    ThoiGianGiaoCa = c.ThoiGianGiaoCa,
                    TienDauCa = c.TienDauCa ?? 0,
                    TongTienHeThong = c.TongTienHeThong ?? 0,
                    TrangThaiCa = c.TrangThaiCa ?? 0
                })
                .ToListAsync();

            var viewModel = new DailyScheduleViewModel
            {
                SelectedDate = selectedDate,
                AllEmployees = allEmployees,
                ScheduledShifts = scheduledShifts,
                ActiveShifts = activeShifts
            };

            return View(viewModel);
        }

        // API: Lấy chi tiết ca làm việc theo ngày
        [HttpGet]
        public async Task<IActionResult> GetShiftDetailForDate(int employeeId, string date)
        {
            try
            {
                var selectedDate = DateOnly.Parse(date);
                
                // Lấy thông tin nhân viên
                var employee = await _context.NhanViens.FindAsync(employeeId);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });
                }

                // Lấy lịch đã xếp
                var schedule = await _context.LichLamViecs
                    .FirstOrDefaultAsync(l => l.MaNhanVien == employeeId && l.NgayLamViec == selectedDate);

                if (schedule == null)
                {
                    return Json(new { success = false, message = "Nhân viên không có ca làm việc trong ngày này" });
                }

                // Lấy ca đang trực (nếu có)
                var dateTime = selectedDate.ToDateTime(TimeOnly.MinValue);
                var startOfDay = dateTime.Date;
                var endOfDay = startOfDay.AddDays(1);
                var activeShift = await _context.CaLamViecs
                    .Where(c => c.MaNhanVien == employeeId
                             && c.ThoiGianNhanCa >= startOfDay
                             && c.ThoiGianNhanCa < endOfDay
                             && c.TrangThaiCa == 0)
                    .FirstOrDefaultAsync();

                var (startHour, endHour) = schedule.LoaiCa switch
                {
                    1 => (6, 14),
                    2 => (14, 22),
                    3 => (22, 6),
                    _ => (0, 0)
                };

                var shiftName = schedule.LoaiCa switch
                {
                    1 => "Ca sáng",
                    2 => "Ca chiều",
                    3 => "Ca đêm",
                    _ => "Không xác định"
                };

                var role = employee.ChucVu switch
                {
                    0 => "Admin",
                    1 => "Quản lý",
                    2 => "Bảo vệ",
                    3 => "Kỹ thuật",
                    4 => "Nhân viên",
                    _ => "Chưa xác định"
                };

                var result = new
                {
                    success = true,
                    employeeName = employee.HoTen,
                    role = role,
                    shiftName = shiftName,
                    startTime = $"{startHour:D2}:00",
                    endTime = schedule.LoaiCa == 3 ? "06:00" : $"{endHour:D2}:00",
                    note = schedule.GhiChu,
                    isActive = activeShift != null,
                    activeShift = activeShift != null ? new
                    {
                        thoiGianNhanCa = activeShift.ThoiGianNhanCa,
                        tienDauCa = activeShift.TienDauCa ?? 0,
                        tongTienHeThong = activeShift.TongTienHeThong ?? 0
                    } : null
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Lịch ca làm việc (Calendar view)
        public async Task<IActionResult> ShiftCalendar(int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            var firstDay = new DateTime(selectedYear, selectedMonth, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Lấy tất cả ca trong tháng
            var shifts = await _context.CaLamViecs
                .Include(c => c.MaNhanVienNavigation)
                .Where(c => c.ThoiGianNhanCa.HasValue 
                         && c.ThoiGianNhanCa.Value >= firstDay 
                         && c.ThoiGianNhanCa.Value <= lastDay.AddDays(1))
                .Select(c => new ShiftViewModel
                {
                    MaCa = c.MaCa,
                    MaNhanVien = c.MaNhanVien,
                    TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                    ThoiGianNhanCa = c.ThoiGianNhanCa,
                    ThoiGianGiaoCa = c.ThoiGianGiaoCa,
                    TienDauCa = c.TienDauCa ?? 0,
                    TongTienHeThong = c.TongTienHeThong ?? 0,
                    TienMatBanGiao = c.TienMatBanGiao ?? 0,
                    TrangThaiCa = c.TrangThaiCa ?? 0
                })
                .OrderBy(c => c.ThoiGianNhanCa)
                .ToListAsync();

            // Group by date
            var shiftsByDate = shifts
                .Where(s => s.ThoiGianNhanCa.HasValue)
                .GroupBy(s => s.ThoiGianNhanCa!.Value.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Calculate stats
            var monthStats = new MonthStatsViewModel
            {
                TotalActiveShifts = shifts.Count(s => s.TrangThaiCa == 0),
                TotalCompletedShifts = shifts.Count(s => s.TrangThaiCa == 1),
                TotalWorkHours = shifts.Sum(s => (decimal)s.SoGioLam),
                TotalRevenue = shifts.Sum(s => s.TongTienHeThong)
            };

            var viewModel = new ShiftCalendarViewModel
            {
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear,
                ShiftsByDate = shiftsByDate,
                MonthStats = monthStats
            };

            return View(viewModel);
        }

        // API: Lấy ca làm việc theo ngày
        [HttpGet]
        public async Task<IActionResult> GetShiftsForDate(string date)
        {
            try
            {
                var selectedDate = DateTime.Parse(date);
                var startOfDay = selectedDate.Date;
                var endOfDay = startOfDay.AddDays(1);

                var shifts = await _context.CaLamViecs
                    .Include(c => c.MaNhanVienNavigation)
                    .Where(c => c.ThoiGianNhanCa >= startOfDay && c.ThoiGianNhanCa < endOfDay)
                    .Select(c => new
                    {
                        c.MaCa,
                        c.MaNhanVien,
                        TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                        c.ThoiGianNhanCa,
                        c.ThoiGianGiaoCa,
                        c.TrangThaiCa,
                        c.TongTienHeThong,
                        SoGioLam = c.ThoiGianGiaoCa.HasValue 
                            ? (c.ThoiGianGiaoCa.Value - c.ThoiGianNhanCa!.Value).TotalHours 
                            : 0
                    })
                    .OrderBy(c => c.ThoiGianNhanCa)
                    .ToListAsync();

                var result = new
                {
                    success = true,
                    shifts = shifts,
                    activeCount = shifts.Count(s => s.TrangThaiCa == 0),
                    completedCount = shifts.Count(s => s.TrangThaiCa == 1),
                    totalRevenue = shifts.Sum(s => s.TongTienHeThong ?? 0)
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Lưu lịch ca làm việc
        [HttpPost]
        public async Task<IActionResult> SaveShiftSchedule([FromBody] SaveScheduleRequest request)
        {
            try
            {
                LichLamViec? schedule;

                // Nếu có MaLich, update existing
                if (request.MaLich.HasValue && request.MaLich.Value > 0)
                {
                    schedule = await _context.LichLamViecs.FindAsync(request.MaLich.Value);
                    if (schedule == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy lịch" });
                    }

                    schedule.LoaiCa = request.CaLamViec;
                    schedule.GhiChu = request.GhiChu;
                }
                else
                {
                    // Kiểm tra xem đã có lịch trong ngày này chưa
                    var ngayLamViec = DateOnly.Parse(request.NgayLamViec);
                    var existing = await _context.LichLamViecs
                        .FirstOrDefaultAsync(l => l.MaNhanVien == request.MaNhanVien && l.NgayLamViec == ngayLamViec);

                    if (existing != null)
                    {
                        // Update existing
                        existing.LoaiCa = request.CaLamViec;
                        existing.GhiChu = request.GhiChu;
                        schedule = existing;
                    }
                    else
                    {
                        // Create new
                        schedule = new LichLamViec
                        {
                            MaNhanVien = request.MaNhanVien,
                            NgayLamViec = ngayLamViec,
                            LoaiCa = request.CaLamViec,
                            GhiChu = request.GhiChu
                        };
                        _context.LichLamViecs.Add(schedule);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Lưu lịch thành công", scheduleId = schedule.MaLich });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Xóa lịch ca
        [HttpPost]
        public async Task<IActionResult> DeleteShiftSchedule(int id)
        {
            try
            {
                var schedule = await _context.LichLamViecs.FindAsync(id);
                if (schedule == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch" });
                }

                _context.LichLamViecs.Remove(schedule);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa lịch thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Xóa toàn bộ lịch tuần
        [HttpPost]
        public async Task<IActionResult> ClearWeekSchedule(string weekStart)
        {
            try
            {
                var startDate = DateOnly.Parse(weekStart);
                var endDate = startDate.AddDays(6);

                var schedules = await _context.LichLamViecs
                    .Where(l => l.NgayLamViec >= startDate && l.NgayLamViec <= endDate)
                    .ToListAsync();

                _context.LichLamViecs.RemoveRange(schedules);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa toàn bộ lịch tuần" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Tự động phân ca cho tuần
        [HttpPost]
        public async Task<IActionResult> AutoAssignWeek(string weekStart)
        {
            try
            {
                var startDate = DateOnly.Parse(weekStart);
                var endDate = startDate.AddDays(6);

                // Lấy danh sách nhân viên
                var employees = await _context.NhanViens
                    .Where(nv => nv.TrangThaiLamViec == true && (nv.ChucVu == 1 || nv.ChucVu == 2))
                    .ToListAsync();

                if (employees.Count < 2)
                {
                    return Json(new { success = false, message = "Cần ít nhất 2 nhân viên để tự động phân ca" });
                }

                // Xóa lịch cũ nếu có
                var oldSchedules = await _context.LichLamViecs
                    .Where(l => l.NgayLamViec >= startDate && l.NgayLamViec <= endDate)
                    .ToListAsync();
                _context.LichLamViecs.RemoveRange(oldSchedules);

                // Phân ca tự động: 2 ca/ngày (sáng và chiều), luân phiên nhân viên
                int employeeIndex = 0;
                for (int day = 0; day < 7; day++)
                {
                    var currentDate = startDate.AddDays(day);

                    // Ca sáng
                    _context.LichLamViecs.Add(new LichLamViec
                    {
                        MaNhanVien = employees[employeeIndex % employees.Count].MaNhanVien,
                        NgayLamViec = currentDate,
                        LoaiCa = 1, // Ca sáng
                        GhiChu = "Tự động phân ca"
                    });

                    employeeIndex++;

                    // Ca chiều
                    _context.LichLamViecs.Add(new LichLamViec
                    {
                        MaNhanVien = employees[employeeIndex % employees.Count].MaNhanVien,
                        NgayLamViec = currentDate,
                        LoaiCa = 2, // Ca chiều
                        GhiChu = "Tự động phân ca"
                    });

                    employeeIndex++;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã tự động phân ca cho tuần" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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
                             && c.TrangThaiCa == 0)
                    .CountAsync(),
                
                CompletedShifts = await _context.CaLamViecs
                    .Where(c => c.ThoiGianNhanCa >= startOfDay && c.ThoiGianNhanCa < endOfDay
                             && c.TrangThaiCa == 1)
                    .CountAsync(),
                
                TotalRevenue = await _context.CaLamViecs
                    .Where(c => c.ThoiGianNhanCa >= startOfDay && c.ThoiGianNhanCa < endOfDay)
                    .SumAsync(c => c.TongTienHeThong ?? 0)
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
                    c.TongTienHeThong,
                    c.TienMatBanGiao,
                    c.GhiChuBanGiao,
                    c.TrangThaiCa,
                    SoXeVao = c.LuotGuiMaCaVaoNavigations.Count,
                    SoXeRa = c.LuotGuiMaCaRaNavigations.Count,
                    SoGioLam = c.ThoiGianNhanCa.HasValue && c.ThoiGianGiaoCa.HasValue 
                        ? (c.ThoiGianGiaoCa.Value - c.ThoiGianNhanCa.Value).TotalHours 
                        : 0,
                    TienCuoiCa = (c.TienDauCa ?? 0) + (c.TongTienHeThong ?? 0),
                    ChenhLech = (c.TienMatBanGiao ?? 0) - ((c.TienDauCa ?? 0) + (c.TongTienHeThong ?? 0))
                })
                .FirstOrDefaultAsync();

            if (shift == null)
            {
                return NotFound();
            }

            return Json(shift);
        }

        // API: Tạo ca làm việc mới
        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftRequest request)
        {
            try
            {
                var shift = new CaLamViec
                {
                    MaNhanVien = request.MaNhanVien,
                    ThoiGianNhanCa = DateTime.Now,
                    TienDauCa = request.TienDauCa,
                    TongTienHeThong = 0,
                    TienMatBanGiao = 0,
                    TrangThaiCa = 0 // Đang trực
                };

                _context.CaLamViecs.Add(shift);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Mở ca thành công", shiftId = shift.MaCa });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Tạo nhiều ca làm việc cùng lúc (Lập ca cho ngày)
        [HttpPost]
        public async Task<IActionResult> CreateMultipleShifts([FromBody] CreateMultipleShiftsRequest request)
        {
            try
            {
                if (request.Shifts == null || !request.Shifts.Any())
                {
                    return Json(new { success = false, message = "Không có ca nào để tạo" });
                }

                var createdShifts = new List<CaLamViec>();

                foreach (var shiftRequest in request.Shifts)
                {
                    var shift = new CaLamViec
                    {
                        MaNhanVien = shiftRequest.MaNhanVien,
                        ThoiGianNhanCa = DateTime.Parse(shiftRequest.ThoiGianNhanCa),
                        TienDauCa = shiftRequest.TienDauCa,
                        TongTienHeThong = 0,
                        TienMatBanGiao = 0,
                        GhiChuBanGiao = shiftRequest.GhiChuBanGiao,
                        TrangThaiCa = 0 // Đang trực
                    };

                    _context.CaLamViecs.Add(shift);
                    createdShifts.Add(shift);
                }

                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã tạo thành công {createdShifts.Count} ca làm việc",
                    shiftIds = createdShifts.Select(s => s.MaCa).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Chốt ca làm việc
        [HttpPost]
        public async Task<IActionResult> EndShift([FromBody] EndShiftRequest request)
        {
            try
            {
                var shift = await _context.CaLamViecs.FindAsync(request.MaCa);
                if (shift == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc" });
                }

                shift.ThoiGianGiaoCa = DateTime.Now;
                shift.TienMatBanGiao = request.TienMatBanGiao;
                shift.GhiChuBanGiao = request.GhiChuBanGiao;
                shift.TrangThaiCa = 1; // Đã chốt

                // Tính tổng tiền hệ thống từ các lượt gửi
                var tongTienHeThong = await _context.LuotGuis
                    .Where(l => (l.MaCaVao == request.MaCa || l.MaCaRa == request.MaCa))
                    .SumAsync(l => l.TongTien ?? 0);

                shift.TongTienHeThong = tongTienHeThong;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Chốt ca thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Xóa lịch làm việc (Đã ẩn - Không dùng)
        /*
        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var schedule = await _context.LichLamViecs.FindAsync(id);
                if (schedule == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch làm việc" });
                }

                _context.LichLamViecs.Remove(schedule);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa lịch thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        */

        // API: Lấy danh sách nhân viên có thể phân ca
        [HttpGet]
        public async Task<IActionResult> GetAvailableEmployees()
        {
            var employees = await _context.NhanViens
                .Where(nv => nv.TrangThaiLamViec == true) // Lấy tất cả nhân viên đang làm việc
                .OrderBy(nv => nv.HoTen)
                .Select(nv => new
                {
                    maNhanVien = nv.MaNhanVien,
                    hoTen = nv.HoTen,
                    chucVu = nv.ChucVu == 0 ? "Admin" :
                             nv.ChucVu == 1 ? "Quản lý" :
                             nv.ChucVu == 2 ? "Bảo vệ" :
                             nv.ChucVu == 3 ? "Kỹ thuật" : "Nhân viên"
                })
                .ToListAsync();

            return Json(employees);
        }

        // API: Lấy danh sách ca làm việc của nhân viên
        [HttpGet]
        public async Task<IActionResult> GetEmployeeShifts(int employeeId, int month, int year)
        {
            var shifts = await _context.CaLamViecs
                .Where(c => c.MaNhanVien == employeeId
                         && c.ThoiGianNhanCa.HasValue
                         && c.ThoiGianNhanCa.Value.Month == month
                         && c.ThoiGianNhanCa.Value.Year == year)
                .OrderByDescending(c => c.ThoiGianNhanCa)
                .Select(c => new
                {
                    c.MaCa,
                    c.ThoiGianNhanCa,
                    c.ThoiGianGiaoCa,
                    c.TrangThaiCa,
                    c.GhiChuBanGiao,
                    SoGioLam = c.ThoiGianGiaoCa.HasValue 
                        ? (c.ThoiGianGiaoCa.Value - c.ThoiGianNhanCa.Value).TotalHours 
                        : 0
                })
                .ToListAsync();

            return Json(shifts);
        }

        // API: Điều chỉnh giờ làm việc
        [HttpPost]
        public async Task<IActionResult> AdjustShiftTime([FromBody] AdjustShiftRequest request)
        {
            try
            {
                var shift = await _context.CaLamViecs.FindAsync(request.ShiftId);
                if (shift == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc" });
                }

                // Chỉ điều chỉnh nếu có giá trị mới
                if (!string.IsNullOrEmpty(request.CheckIn))
                {
                    var checkInTime = TimeSpan.Parse(request.CheckIn);
                    var checkInDate = shift.ThoiGianNhanCa?.Date ?? DateTime.Today;
                    shift.ThoiGianNhanCa = checkInDate.Add(checkInTime);
                }

                if (!string.IsNullOrEmpty(request.CheckOut))
                {
                    var checkOutTime = TimeSpan.Parse(request.CheckOut);
                    var checkOutDate = shift.ThoiGianGiaoCa?.Date ?? DateTime.Today;
                    shift.ThoiGianGiaoCa = checkOutDate.Add(checkOutTime);
                }

                // Lưu lý do vào GhiChuBanGiao
                shift.GhiChuBanGiao = $"Điều chỉnh: {request.Reason}";

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Điều chỉnh giờ thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Thêm giờ bù
        [HttpPost]
        public async Task<IActionResult> AddOvertime([FromBody] OvertimeRequest request)
        {
            try
            {
                // Tạo một ca đặc biệt để ghi nhận giờ bù
                var overtimeShift = new CaLamViec
                {
                    MaNhanVien = request.EmployeeId,
                    ThoiGianNhanCa = DateTime.Parse($"{request.Date} {request.StartTime}"),
                    ThoiGianGiaoCa = DateTime.Parse($"{request.Date} {request.EndTime}"),
                    TrangThaiCa = 1, // Đã chốt
                    GhiChuBanGiao = $"Bù giờ loại {request.Type}: {request.Note}"
                };

                _context.CaLamViecs.Add(overtimeShift);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm giờ bù thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Ngắt ca
        [HttpPost]
        public async Task<IActionResult> BreakShift([FromBody] BreakShiftRequest request)
        {
            try
            {
                var shift = await _context.CaLamViecs.FindAsync(request.ShiftId);
                if (shift == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc" });
                }

                // Đánh dấu ca bị ngắt
                shift.GhiChuBanGiao = $"Ngắt ca - Loại: {request.Type} - Lý do: {request.Reason}";
                shift.TrangThaiCa = 1; // Đánh dấu đã chốt
                
                // Nếu ca chưa kết thúc, set thời gian giao ca là hiện tại
                if (!shift.ThoiGianGiaoCa.HasValue)
                {
                    shift.ThoiGianGiaoCa = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Ngắt ca thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Chốt ca đơn giản (dùng cho TimeSheet)
        [HttpPost]
        public async Task<IActionResult> CloseShift(int shiftId)
        {
            try
            {
                var shift = await _context.CaLamViecs.FindAsync(shiftId);
                if (shift == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc" });
                }

                // Chỉ chốt ca nếu đang trực
                if (shift.TrangThaiCa != 0)
                {
                    return Json(new { success = false, message = "Ca đã được chốt rồi" });
                }

                // Set thời gian giao ca là hiện tại nếu chưa có
                if (!shift.ThoiGianGiaoCa.HasValue)
                {
                    shift.ThoiGianGiaoCa = DateTime.Now;
                }

                // Tính tổng tiền hệ thống từ các lượt gửi
                var tongTienHeThong = await _context.LuotGuis
                    .Where(l => (l.MaCaVao == shiftId || l.MaCaRa == shiftId))
                    .SumAsync(l => l.TongTien ?? 0);

                shift.TongTienHeThong = tongTienHeThong;
                shift.TrangThaiCa = 1; // Đã chốt

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Chốt ca thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Lấy chi tiết nhân viên
        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetail(int id)
        {
            try
            {
                // Lấy thông tin nhân viên
                var employee = await _context.NhanViens
                    .Where(nv => nv.MaNhanVien == id)
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
                        TrangThaiLamViec = nv.TrangThaiLamViec ?? true
                    })
                    .FirstOrDefaultAsync();

                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });
                }

                // Lấy 10 ca làm việc gần nhất
                var recentShifts = await _context.CaLamViecs
                    .Include(c => c.MaNhanVienNavigation)
                    .Where(c => c.MaNhanVien == id)
                    .OrderByDescending(c => c.ThoiGianNhanCa)
                    .Take(10)
                    .Select(c => new ShiftViewModel
                    {
                        MaCa = c.MaCa,
                        MaNhanVien = c.MaNhanVien,
                        TenNhanVien = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : "N/A",
                        ThoiGianNhanCa = c.ThoiGianNhanCa,
                        ThoiGianGiaoCa = c.ThoiGianGiaoCa,
                        TienDauCa = c.TienDauCa ?? 0,
                        TongTienHeThong = c.TongTienHeThong ?? 0,
                        TienMatBanGiao = c.TienMatBanGiao ?? 0,
                        GhiChuBanGiao = c.GhiChuBanGiao,
                        TrangThaiCa = c.TrangThaiCa ?? 0
                    })
                    .ToListAsync();

                // Tính toán thống kê
                var allShifts = await _context.CaLamViecs
                    .Where(c => c.MaNhanVien == id)
                    .ToListAsync();

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var stats = new EmployeeStatsViewModel
                {
                    TotalShifts = allShifts.Count,
                    ActiveShifts = allShifts.Count(c => c.TrangThaiCa == 0),
                    CompletedShifts = allShifts.Count(c => c.TrangThaiCa == 1),
                    TotalWorkHours = (decimal)allShifts
                        .Where(c => c.ThoiGianNhanCa.HasValue && c.ThoiGianGiaoCa.HasValue)
                        .Sum(c => (c.ThoiGianGiaoCa!.Value - c.ThoiGianNhanCa!.Value).TotalHours),
                    TotalRevenue = allShifts.Sum(c => c.TongTienHeThong ?? 0),
                    CurrentMonthShifts = allShifts.Count(c => 
                        c.ThoiGianNhanCa.HasValue && 
                        c.ThoiGianNhanCa.Value.Month == currentMonth && 
                        c.ThoiGianNhanCa.Value.Year == currentYear),
                    CurrentMonthHours = (decimal)allShifts
                        .Where(c => c.ThoiGianNhanCa.HasValue && c.ThoiGianGiaoCa.HasValue &&
                               c.ThoiGianNhanCa.Value.Month == currentMonth && 
                               c.ThoiGianNhanCa.Value.Year == currentYear)
                        .Sum(c => (c.ThoiGianGiaoCa!.Value - c.ThoiGianNhanCa!.Value).TotalHours)
                };

                stats.AverageShiftHours = stats.CompletedShifts > 0 
                    ? stats.TotalWorkHours / stats.CompletedShifts 
                    : 0;

                var detail = new EmployeeDetailViewModel
                {
                    Employee = employee,
                    RecentShifts = recentShifts,
                    Stats = stats
                };

                return Json(new { success = true, data = detail });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Lấy lịch cá nhân của nhân viên (tuần hiện tại)
        [HttpGet]
        public async Task<IActionResult> GetPersonalSchedule(int employeeId)
        {
            try
            {
                // Lấy tuần hiện tại (từ thứ 2 đến chủ nhật)
                var today = DateTime.Today;
                var dayOfWeek = (int)today.DayOfWeek;
                var mondayOffset = dayOfWeek == 0 ? -6 : 1 - dayOfWeek; // Nếu là CN thì lùi 6 ngày
                var monday = today.AddDays(mondayOffset);
                var sunday = monday.AddDays(6);

                // Lấy các ca làm việc của nhân viên trong tuần
                var schedule = await _context.CaLamViecs
                    .Where(c => c.MaNhanVien == employeeId 
                             && c.ThoiGianNhanCa.HasValue
                             && c.ThoiGianNhanCa.Value.Date >= monday 
                             && c.ThoiGianNhanCa.Value.Date <= sunday)
                    .OrderBy(c => c.ThoiGianNhanCa)
                    .Select(c => new
                    {
                        maCa = c.MaCa,
                        ngay = c.ThoiGianNhanCa.Value,
                        thoiGianNhanCa = c.ThoiGianNhanCa,
                        thoiGianGiaoCa = c.ThoiGianGiaoCa,
                        soGioLam = c.ThoiGianGiaoCa.HasValue && c.ThoiGianNhanCa.HasValue 
                                   ? (decimal)(c.ThoiGianGiaoCa.Value - c.ThoiGianNhanCa.Value).TotalHours 
                                   : 0,
                        trangThaiCa = c.TrangThaiCa ?? 0
                    })
                    .ToListAsync();

                return Json(new { success = true, data = schedule });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: In báo cáo ca làm việc
        [HttpGet]
        public async Task<IActionResult> PrintShiftReport(int id)
        {
            var shift = await _context.CaLamViecs
                .Include(c => c.MaNhanVienNavigation)
                .Include(c => c.LuotGuiMaCaVaoNavigations)
                .Include(c => c.LuotGuiMaCaRaNavigations)
                .FirstOrDefaultAsync(c => c.MaCa == id);

            if (shift == null)
            {
                return NotFound();
            }

            // TODO: Implement print view
            return View("PrintShiftReport", shift);
        }

        // API: Cập nhật thông tin nhân viên
        [HttpPost]
        public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeRequest request)
        {
            try
            {
                var employee = await _context.NhanViens.FindAsync(request.MaNhanVien);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });
                }

                // Cập nhật thông tin
                employee.HoTen = request.HoTen;
                employee.GioiTinh = request.GioiTinh;
                employee.SoDienThoai = request.SoDienThoai;
                employee.DiaChi = request.DiaChi;
                employee.ChucVu = request.ChucVu;
                employee.TrangThaiLamViec = request.TrangThaiLamViec;

                // Parse dates if provided
                if (!string.IsNullOrEmpty(request.NgaySinh))
                {
                    if (DateOnly.TryParse(request.NgaySinh, out var ngaySinh))
                    {
                        employee.NgaySinh = ngaySinh;
                    }
                }

                if (!string.IsNullOrEmpty(request.NgayVaoLam))
                {
                    if (DateOnly.TryParse(request.NgayVaoLam, out var ngayVaoLam))
                    {
                        employee.NgayVaoLam = ngayVaoLam;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thông tin nhân viên thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Thêm nhân viên mới
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.HoTen))
                {
                    return Json(new { success = false, message = "Họ tên không được để trống" });
                }

                // Check duplicate phone number
                if (!string.IsNullOrEmpty(request.SoDienThoai))
                {
                    var existingPhone = await _context.NhanViens
                        .AnyAsync(nv => nv.SoDienThoai == request.SoDienThoai);
                    
                    if (existingPhone)
                    {
                        return Json(new { success = false, message = "Số điện thoại đã được sử dụng" });
                    }
                }

                // Create new employee
                var employee = new NhanVien
                {
                    HoTen = request.HoTen.Trim(),
                    GioiTinh = request.GioiTinh,
                    SoDienThoai = request.SoDienThoai,
                    DiaChi = request.DiaChi,
                    ChucVu = request.ChucVu,
                    TrangThaiLamViec = request.TrangThaiLamViec,
                    NgayVaoLam = DateOnly.FromDateTime(DateTime.Today) // Default: Hôm nay
                };

                // Parse dates if provided
                if (!string.IsNullOrEmpty(request.NgaySinh))
                {
                    if (DateOnly.TryParse(request.NgaySinh, out var ngaySinh))
                    {
                        employee.NgaySinh = ngaySinh;
                    }
                }

                if (!string.IsNullOrEmpty(request.NgayVaoLam))
                {
                    if (DateOnly.TryParse(request.NgayVaoLam, out var ngayVaoLam))
                    {
                        employee.NgayVaoLam = ngayVaoLam;
                    }
                }

                _context.NhanViens.Add(employee);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Thêm nhân viên thành công",
                    employeeId = employee.MaNhanVien 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Lấy danh sách nhân viên đang làm việc cho phân công quầy
        [HttpGet]
        public async Task<IActionResult> GetActiveEmployees()
        {
            try
            {
                var employees = await _context.NhanViens
                    .Where(nv => nv.TrangThaiLamViec == true)
                    .OrderBy(nv => nv.HoTen)
                    .Select(nv => new
                    {
                        maNhanVien = nv.MaNhanVien,
                        hoTen = nv.HoTen,
                        chucVu = nv.ChucVu == 0 ? "Admin" :
                                 nv.ChucVu == 1 ? "Quản lý" :
                                 nv.ChucVu == 2 ? "Bảo vệ" :
                                 nv.ChucVu == 3 ? "Kỹ thuật" : "Nhân viên",
                        soDienThoai = nv.SoDienThoai,
                        trangThaiLamViec = nv.TrangThaiLamViec ?? false
                    })
                    .ToListAsync();

                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Lấy thông tin phân công quầy hiện tại
        [HttpGet]
        public async Task<IActionResult> GetCounterAssignments()
        {
            try
            {
                // Lấy các ca đang trực (TrangThaiCa = 0)
                var activeShift = await _context.CaLamViecs
                    .Include(c => c.MaNhanVienNavigation)
                    .Where(c => c.TrangThaiCa == 0)
                    .OrderByDescending(c => c.ThoiGianNhanCa)
                    .Select(c => new
                    {
                        counter = GetCounterNumber(c.MaCa), // Sẽ map ca ID sang số quầy 1, 2, 3
                        employee = new
                        {
                            maNhanVien = c.MaNhanVien,
                            hoTen = c.MaNhanVienNavigation != null ? c.MaNhanVienNavigation.HoTen : null,
                            maNhanVienFormatted = $"NV{c.MaNhanVien:D4}",
                            chucVu = c.MaNhanVienNavigation != null && c.MaNhanVienNavigation.ChucVu == 1 ? "Quản lý" : "Nhân viên"
                        },
                        maCa = c.MaCa
                    })
                    .ToListAsync();

                return Json(new { success = true, data = activeShift });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Helper method để map ca ID sang số quầy
        private int GetCounterNumber(int shiftId)
        {
            // Logic đơn giản: chia dư cho 3 để có số quầy từ 1-3
            var counter = (shiftId % 3) + 1;
            return counter > 3 ? 1 : counter;
        }

        // API: Lưu phân công quầy và khởi động ca
        [HttpPost]
        public async Task<IActionResult> SaveCounterAssignments([FromBody] CounterAssignmentRequest request)
        {
            try
            {
                if (request.Assignments == null || !request.Assignments.Any())
                {
                    return Json(new { success = false, message = "Không có phân công nào được chọn" });
                }

                var createdShifts = new List<int>();

                foreach (var assignment in request.Assignments)
                {
                    // Kiểm tra nhân viên có đang trực ca nào không
                    var existingShift = await _context.CaLamViecs
                        .FirstOrDefaultAsync(c => c.MaNhanVien == assignment.MaNhanVien && c.TrangThaiCa == 0);

                    if (existingShift != null)
                    {
                        return Json(new 
                        { 
                            success = false, 
                            message = $"Nhân viên đã đang trực ca. Vui lòng chốt ca trước khi phân công mới." 
                        });
                    }

                    // Tạo ca làm việc mới
                    var newShift = new CaLamViec
                    {
                        MaNhanVien = assignment.MaNhanVien,
                        ThoiGianNhanCa = DateTime.Now,
                        TienDauCa = 0,
                        TongTienHeThong = 0,
                        TienMatBanGiao = 0,
                        TrangThaiCa = 0, // Đang trực
                        GhiChuBanGiao = $"Phân công quầy {assignment.Counter}"
                    };

                    _context.CaLamViecs.Add(newShift);
                    await _context.SaveChangesAsync();

                    createdShifts.Add(newShift.MaCa);
                }

                // Lưu thông tin phân công vào session hoặc cache để VehicleVision sử dụng
                // Có thể lưu vào database hoặc cache tạm thời
                SaveCounterAssignmentToCache(request.Assignments);

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã phân công {request.Assignments.Count} quầy thành công và khởi động ca làm việc!",
                    shiftIds = createdShifts
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Helper method để lưu thông tin phân công vào cache/session
        private void SaveCounterAssignmentToCache(List<CounterAssignment> assignments)
        {
            // Lưu vào HttpContext.Session hoặc distributed cache
            // Ở đây đơn giản hóa bằng cách lưu vào static dictionary (chỉ để demo)
            // Trong production, nên dùng IMemoryCache hoặc Redis
            foreach (var assignment in assignments)
            {
                HttpContext.Session.SetInt32($"Counter_{assignment.Counter}_Employee", assignment.MaNhanVien);
            }
        }

        // API: Lấy danh sách nhân viên có thể phân vào quầy (không đang trực ca)
        [HttpGet]
        public async Task<IActionResult> GetAvailableEmployeesForCounter()
        {
            try
            {
                // Lấy các nhân viên đang trực ca
                var activeShiftEmployeeIds = await _context.CaLamViecs
                    .Where(c => c.TrangThaiCa == 0) // Đang trực
                    .Select(c => c.MaNhanVien)
                    .ToListAsync();

                // Lấy TẤT CẢ nhân viên đang làm việc và không trong danh sách đang trực ca
                var availableEmployees = await _context.NhanViens
                    .Where(nv => nv.TrangThaiLamViec == true 
                              && !activeShiftEmployeeIds.Contains(nv.MaNhanVien))
                    .OrderBy(nv => nv.HoTen)
                    .Select(nv => new
                    {
                        maNhanVien = nv.MaNhanVien,
                        hoTen = nv.HoTen,
                        chucVu = nv.ChucVu == 1 ? "Quản lý" : 
                                 nv.ChucVu == 2 ? "Bảo vệ" :
                                 nv.ChucVu == 3 ? "Kỹ thuật" : 
                                 nv.ChucVu == 4 ? "Nhân viên" : "Khác",
                        chucVuCode = nv.ChucVu ?? 0,
                        maNhanVienFormatted = $"NV{nv.MaNhanVien:D4}",
                        soDienThoai = nv.SoDienThoai,
                        avatar = nv.HoTen != null ? nv.HoTen.Substring(0, 1).ToUpper() : "NV"
                    })
                    .ToListAsync();

                return Json(new 
                { 
                    success = true, 
                    data = availableEmployees,
                    count = availableEmployees.Count,
                    message = availableEmployees.Count > 0 
                        ? $"Có {availableEmployees.Count} nhân viên sẵn sàng" 
                        : "Không có nhân viên rảnh. Tất cả đang trực ca."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Lấy trạng thái tất cả các quầy
        [HttpGet]
        public async Task<IActionResult> GetAllCountersStatus()
        {
            try
            {
                var counters = new List<object>();

                for (int i = 1; i <= 3; i++)
                {
                    // Lấy thông tin ca làm việc hiện tại cho quầy này
                    // Dùng GhiChuBanGiao để xác định quầy (chứa text "Phân công quầy X")
                    var activeShift = await _context.CaLamViecs
                        .Include(c => c.MaNhanVienNavigation)
                        .Where(c => c.TrangThaiCa == 0 && 
                                    c.GhiChuBanGiao != null && 
                                    c.GhiChuBanGiao.Contains($"Phân công quầy {i}"))
                        .OrderByDescending(c => c.ThoiGianNhanCa)
                        .FirstOrDefaultAsync();

                    // Tính doanh thu REAL-TIME từ LuotGuis
                    decimal revenue = 0;
                    if (activeShift != null)
                    {
                        revenue = await _context.LuotGuis
                            .Where(l => l.MaCaVao == activeShift.MaCa || l.MaCaRa == activeShift.MaCa)
                            .SumAsync(l => l.TongTien ?? 0);
                    }

                    var counterStatus = new
                    {
                        counter = i,
                        isActive = activeShift != null,
                        employee = activeShift != null ? new
                        {
                            maNhanVien = activeShift.MaNhanVien,
                            hoTen = activeShift.MaNhanVienNavigation?.HoTen,
                            maNhanVienFormatted = $"NV{activeShift.MaNhanVien:D4}",
                            chucVu = activeShift.MaNhanVienNavigation?.ChucVu == 1 ? "Quản lý" : "Nhân viên"
                        } : null,
                        shift = activeShift != null ? new
                        {
                            maCa = activeShift.MaCa,
                            thoiGianNhanCa = activeShift.ThoiGianNhanCa,
                            soGioLam = activeShift.ThoiGianNhanCa.HasValue 
                                ? (DateTime.Now - activeShift.ThoiGianNhanCa.Value).TotalHours 
                                : 0
                        } : null,
                        revenue = revenue,
                        revenueFormatted = revenue >= 1000000 
                            ? $"{(revenue / 1000000):F1}M VNĐ" 
                            : $"{revenue:N0} VNĐ"
                    };

                    counters.Add(counterStatus);
                }

                return Json(new { success = true, data = counters });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Phân công nhân viên vào quầy cụ thể
        [HttpPost]
        public async Task<IActionResult> AssignEmployeeToCounter([FromBody] SingleCounterAssignmentRequest request)
        {
            try
            {
                // ✅ VALIDATE input parameters
                if (request == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Kiểm tra nhân viên có tồn tại không
                var employee = await _context.NhanViens.FindAsync(request.MaNhanVien);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });
                }

                // Kiểm tra nhân viên có đang làm việc không
                if (employee.TrangThaiLamViec != true)
                {
                    return Json(new { success = false, message = "Nhân viên không còn làm việc" });
                }

                // Kiểm tra nhân viên có đang trực ca nào không
                var existingShift = await _context.CaLamViecs
                    .FirstOrDefaultAsync(c => c.MaNhanVien == request.MaNhanVien && c.TrangThaiCa == 0);

                if (existingShift != null)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "Nhân viên đã đang trực ca. Vui lòng chốt ca trước khi phân công mới." 
                    });
                }

                // Kiểm tra quầy có nhân viên đang trực không
                var existingCounterShift = await _context.CaLamViecs
                    .Where(c => c.TrangThaiCa == 0 && c.GhiChuBanGiao != null 
                             && c.GhiChuBanGiao.Contains($"Phân công quầy {request.Counter}"))
                    .FirstOrDefaultAsync();

                if (existingCounterShift != null)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = $"Quầy {request.Counter} đã có nhân viên đang trực. Vui lòng đóng quầy trước." 
                    });
                }

                // Tạo ca làm việc mới
                var newShift = new CaLamViec
                {
                    MaNhanVien = request.MaNhanVien,
                    ThoiGianNhanCa = DateTime.Now,
                    TienDauCa = 0,
                    TongTienHeThong = 0,
                    TienMatBanGiao = 0,
                    TrangThaiCa = 0, // Đang trực
                    GhiChuBanGiao = $"Phân công quầy {request.Counter}"
                };

                _context.CaLamViecs.Add(newShift);
                await _context.SaveChangesAsync();

                // Dùng lại employee đã fetch ở trên để trả về
                return Json(new 
                { 
                    success = true, 
                    message = $"Đã phân công quầy {request.Counter} thành công!",
                    shift = new
                    {
                        maCa = newShift.MaCa,
                        counter = request.Counter,
                        employee = new
                        {
                            maNhanVien = employee?.MaNhanVien,
                            hoTen = employee?.HoTen,
                            maNhanVienFormatted = $"NV{employee?.MaNhanVien:D4}"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Đóng quầy (kết thúc ca của quầy cụ thể)
        [HttpPost]
        public async Task<IActionResult> CloseCounter([FromBody] CloseCounterRequest request)
        {
            try
            {
                // ✅ VALIDATE input parameters
                if (request == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                if (request.Counter < 1 || request.Counter > 3)
                {
                    return Json(new { success = false, message = "Số quầy phải từ 1 đến 3" });
                }

                if (request.TienMatBanGiao < 0)
                {
                    return Json(new { success = false, message = "Tiền bàn giao không được âm" });
                }

                // Tìm ca đang trực ở quầy này
                var activeShift = await _context.CaLamViecs
                    .Where(c => c.TrangThaiCa == 0 && c.GhiChuBanGiao != null 
                             && c.GhiChuBanGiao.Contains($"Phân công quầy {request.Counter}"))
                    .OrderByDescending(c => c.ThoiGianNhanCa)
                    .FirstOrDefaultAsync();

                if (activeShift == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy ca đang trực ở quầy {request.Counter}" });
                }

                // Cập nhật thông tin kết thúc ca
                activeShift.ThoiGianGiaoCa = DateTime.Now;
                activeShift.TienMatBanGiao = request.TienMatBanGiao;
                activeShift.GhiChuBanGiao = $"Phân công quầy {request.Counter} - {request.GhiChu}";
                activeShift.TrangThaiCa = 1; // Đã chốt

                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã đóng quầy {request.Counter} thành công!",
                    revenue = activeShift.TongTienHeThong
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Lịch cá nhân - Xem lịch làm việc của nhân viên trong tuần
        public async Task<IActionResult> PersonalSchedule(int employeeId, int? year, int? month, int? day)
        {
            // Lấy thông tin nhân viên
            var employee = await _context.NhanViens
                .Where(nv => nv.MaNhanVien == employeeId)
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
                    TrangThaiLamViec = nv.TrangThaiLamViec ?? true
                })
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên");
            }

            // Xác định tuần bắt đầu (Thứ 2)
            DateTime selectedDate = year.HasValue && month.HasValue && day.HasValue
                ? new DateTime(year.Value, month.Value, day.Value)
                : DateTime.Today;

            // Tìm thứ 2 của tuần
            int daysToMonday = ((int)selectedDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            DateTime weekStart = selectedDate.AddDays(-daysToMonday).Date;
            DateTime weekEnd = weekStart.AddDays(6);

            // Lấy lịch làm việc đã xếp trong tuần
            var scheduledShifts = await _context.LichLamViecs
                .Where(l => l.MaNhanVien == employeeId
                         && l.NgayLamViec >= DateOnly.FromDateTime(weekStart)
                         && l.NgayLamViec <= DateOnly.FromDateTime(weekEnd))
                .Select(l => new ScheduleViewModel
                {
                    MaLich = l.MaLich,
                    MaNhanVien = l.MaNhanVien,
                    TenNhanVien = employee.HoTen,
                    NgayLamViec = l.NgayLamViec,
                    CaLamViec = l.LoaiCa,
                    GhiChu = l.GhiChu
                })
                .ToListAsync();

            // Lấy các ca đã làm thực tế trong tuần (từ bảng CaLamViec)
            var actualShifts = await _context.CaLamViecs
                .Where(c => c.MaNhanVien == employeeId
                         && c.ThoiGianNhanCa.HasValue
                         && c.ThoiGianNhanCa.Value.Date >= weekStart
                         && c.ThoiGianNhanCa.Value.Date <= weekEnd)
                .Select(c => new ShiftViewModel
                {
                    MaCa = c.MaCa,
                    MaNhanVien = c.MaNhanVien,
                    TenNhanVien = employee.HoTen,
                    ThoiGianNhanCa = c.ThoiGianNhanCa,
                    ThoiGianGiaoCa = c.ThoiGianGiaoCa,
                    TienDauCa = c.TienDauCa ?? 0,
                    TongTienHeThong = c.TongTienHeThong ?? 0,
                    TienMatBanGiao = c.TienMatBanGiao ?? 0,
                    GhiChuBanGiao = c.GhiChuBanGiao,
                    TrangThaiCa = c.TrangThaiCa ?? 0
                })
                .OrderBy(c => c.ThoiGianNhanCa)
                .ToListAsync();

            var viewModel = new PersonalScheduleViewModel
            {
                Employee = employee,
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                ScheduledShifts = scheduledShifts,
                ActualShifts = actualShifts
            };

            return View(viewModel);
        }
    }

    // Request models
    public class SingleCounterAssignmentRequest
    {
        [Required(ErrorMessage = "Số quầy không được để trống")]
        [Range(1, 3, ErrorMessage = "Số quầy phải từ 1 đến 3")]
        public int Counter { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }
    }

    public class CloseCounterRequest
    {
        [Required(ErrorMessage = "Số quầy không được để trống")]
        [Range(1, 3, ErrorMessage = "Số quầy phải từ 1 đến 3")]
        public int Counter { get; set; }

        [Required(ErrorMessage = "Tiền bàn giao không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền bàn giao không được âm")]
        public decimal TienMatBanGiao { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChu { get; set; } = string.Empty;
    }

    // Request models
    public class CounterAssignmentRequest
    {
        [Required(ErrorMessage = "Danh sách phân công không được trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 phân công")]
        public List<CounterAssignment> Assignments { get; set; } = new List<CounterAssignment>();
    }

    public class CounterAssignment
    {
        [Required(ErrorMessage = "Số quầy không được để trống")]
        [Range(1, 3, ErrorMessage = "Số quầy phải từ 1 đến 3")]
        public int Counter { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }
    }

    public class CreateShiftRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Tiền đầu ca không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền đầu ca không được âm")]
        public decimal TienDauCa { get; set; }
    }

    public class EndShiftRequest
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int MaCa { get; set; }

        [Required(ErrorMessage = "Tiền bàn giao không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền bàn giao không được âm")]
        public decimal TienMatBanGiao { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChuBanGiao { get; set; }
    }

    public class AddScheduleRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        public DateOnly NgayLamViec { get; set; }

        [Required(ErrorMessage = "Ca làm việc không được để trống")]
        [Range(1, 3, ErrorMessage = "Ca làm việc phải từ 1 đến 3 (1: Sáng, 2: Chiều, 3: Đêm)")]
        public int CaLamViec { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChu { get; set; }
    }

    public class AdjustShiftRequest
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int ShiftId { get; set; }

        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ vào ca không đúng định dạng (HH:mm)")]
        public string? CheckIn { get; set; }

        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ ra ca không đúng định dạng (HH:mm)")]
        public string? CheckOut { get; set; }

        [Required(ErrorMessage = "Lý do điều chỉnh không được để trống")]
        [StringLength(500, ErrorMessage = "Lý do không được quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }

    public class OvertimeRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Ngày làm thêm không được để trống")]
        public string Date { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ bắt đầu không được để trống")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ bắt đầu không đúng định dạng (HH:mm)")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ kết thúc không được để trống")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ kết thúc không đúng định dạng (HH:mm)")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại làm thêm không được để trống")]
        [Range(1, 3, ErrorMessage = "Loại làm thêm phải từ 1 đến 3 (1: Ngày thường, 2: Ngày nghỉ, 3: Lễ)")]
        public int Type { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? Note { get; set; }
    }

    public class BreakShiftRequest
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int ShiftId { get; set; }

        [Required(ErrorMessage = "Loại nghỉ không được để trống")]
        [Range(1, 3, ErrorMessage = "Loại nghỉ phải từ 1 đến 3 (1: Nghỉ phép, 2: Nghỉ ốm, 3: Khác)")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Lý do nghỉ không được để trống")]
        [StringLength(500, ErrorMessage = "Lý do không được quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;

        public bool NeedReplacement { get; set; }

        public int? ReplacementEmployeeId { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Giới tính không được quá 10 ký tự")]
        public string? GioiTinh { get; set; }

        public string? NgaySinh { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        public string? SoDienThoai { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [Range(0, 4, ErrorMessage = "Chức vụ phải từ 0 đến 4 (0: Admin, 1: Quản lý, 2: Bảo vệ, 3: Kỹ thuật, 4: Nhân viên)")]
        public int ChucVu { get; set; }

        public string? NgayVaoLam { get; set; }

        public bool TrangThaiLamViec { get; set; }
    }

    public class CreateEmployeeRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Giới tính không được quá 10 ký tự")]
        public string? GioiTinh { get; set; }

        public string? NgaySinh { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        public string? SoDienThoai { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [Range(0, 4, ErrorMessage = "Chức vụ phải từ 0 đến 4 (0: Admin, 1: Quản lý, 2: Bảo vệ, 3: Kỹ thuật, 4: Nhân viên)")]
        public int ChucVu { get; set; }

        public string? NgayVaoLam { get; set; }

        public bool TrangThaiLamViec { get; set; } = true;
    }

    public class SaveScheduleRequest
    {
        public int? MaLich { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        public string NgayLamViec { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ca làm việc không được để trống")]
        [Range(1, 3, ErrorMessage = "Ca làm việc phải từ 1 đến 3 (1: Sáng, 2: Chiều, 3: Đêm)")]
        public int CaLamViec { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChu { get; set; }
    }

    public class CreateMultipleShiftsRequest
    {
        [Required(ErrorMessage = "Danh sách ca không được trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 ca")]
        public List<ShiftCreationData> Shifts { get; set; } = new List<ShiftCreationData>();
    }

    public class ShiftCreationData
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Thời gian nhận ca không được để trống")]
        public string ThoiGianNhanCa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiền đầu ca không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền đầu ca không được âm")]
        public decimal TienDauCa { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChuBanGiao { get; set; }
    }

    public class UpdateDayShiftsRequest
    {
        [Required(ErrorMessage = "Danh sách cập nhật không được trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 cập nhật")]
        public List<ShiftUpdateData> Updates { get; set; } = new List<ShiftUpdateData>();
    }

    public class ShiftUpdateData
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int MaCa { get; set; }

        public int? MaNhanVien { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tiền đầu ca không được âm")]
        public decimal TienDauCa { get; set; }
        public string? GhiChuBanGiao { get; set; }
    }
}


