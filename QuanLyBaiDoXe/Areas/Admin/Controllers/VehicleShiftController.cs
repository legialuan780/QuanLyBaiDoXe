using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
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
                    TenNhanVien = l.MaNhanVienNavigation != null ? l.MaNhanVienNavigation.HoTen : "N/A",
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
                emp.TongDoanhThu = shifts.Sum(s => s.TongTienHeThong ?? 0);
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
                    CaLamViec = l.CaLamViec,
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
                    CaLamViec = l.CaLamViec,
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

                var (startHour, endHour) = schedule.CaLamViec switch
                {
                    1 => (6, 14),
                    2 => (14, 22),
                    3 => (22, 6),
                    _ => (0, 0)
                };

                var shiftName = schedule.CaLamViec switch
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
                    endTime = schedule.CaLamViec == 3 ? "06:00" : $"{endHour:D2}:00",
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

        // API: Lấy chi tiết ca làm việc theo ngày (cũ)

        // Lập lịch tuần (cũ)

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

                    schedule.CaLamViec = request.CaLamViec;
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
                        existing.CaLamViec = request.CaLamViec;
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
                            CaLamViec = request.CaLamViec,
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
                        CaLamViec = 1, // Ca sáng
                        GhiChu = "Tự động phân ca"
                    });

                    employeeIndex++;

                    // Ca chiều
                    _context.LichLamViecs.Add(new LichLamViec
                    {
                        MaNhanVien = employees[employeeIndex % employees.Count].MaNhanVien,
                        NgayLamViec = currentDate,
                        CaLamViec = 2, // Ca chiều
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
                    SoXeRa = c.LuotGuiMaCaRaNavigations.Count
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

        // API: Thêm lịch làm việc (Đã ẩn - Không dùng)
        /*
        [HttpPost]
        public async Task<IActionResult> AddSchedule([FromBody] AddScheduleRequest request)
        {
            try
            {
                var schedule = new LichLamViec
                {
                    MaNhanVien = request.MaNhanVien,
                    NgayLamViec = request.NgayLamViec,
                    CaLamViec = request.CaLamViec,
                    GhiChu = request.GhiChu
                };

                _context.LichLamViecs.Add(schedule);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm lịch thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // API: Xóa lịch làm việc (Đã ẩn - Không dùng)
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
                .Where(nv => nv.TrangThaiLamViec == true && (nv.ChucVu == 1 || nv.ChucVu == 2)) // Quản lý hoặc Bảo vệ
                .Select(nv => new
                {
                    nv.MaNhanVien,
                    nv.HoTen,
                    ChucVu = nv.ChucVu == 1 ? "Quản lý" : "Bảo vệ"
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
    }

    // Request models
    public class CreateShiftRequest
    {
        public int MaNhanVien { get; set; }
        public decimal TienDauCa { get; set; }
    }

    public class EndShiftRequest
    {
        public int MaCa { get; set; }
        public decimal TienMatBanGiao { get; set; }
        public string? GhiChuBanGiao { get; set; }
    }

    public class AddScheduleRequest
    {
        public int MaNhanVien { get; set; }
        public DateOnly NgayLamViec { get; set; }
        public int CaLamViec { get; set; }
        public string? GhiChu { get; set; }
    }

    public class AdjustShiftRequest
    {
        public int ShiftId { get; set; }
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class OvertimeRequest
    {
        public int EmployeeId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? Note { get; set; }
    }

    public class BreakShiftRequest
    {
        public int ShiftId { get; set; }
        public int Type { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool NeedReplacement { get; set; }
        public int? ReplacementEmployeeId { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public int MaNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? NgaySinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public int ChucVu { get; set; }
        public string? NgayVaoLam { get; set; }
        public bool TrangThaiLamViec { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? NgaySinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public int ChucVu { get; set; }
        public string? NgayVaoLam { get; set; }
        public bool TrangThaiLamViec { get; set; } = true;
    }

    public class SaveScheduleRequest
    {
        public int? MaLich { get; set; }
        public int MaNhanVien { get; set; }
        public string NgayLamViec { get; set; } = string.Empty;
        public int CaLamViec { get; set; }
        public string? GhiChu { get; set; }
    }
}


