using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class VehicleMoneyController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleMoneyController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, string? loaiDoanhThu)
        {
            // Mặc định lấy dữ liệu 30 ngày gần nhất
            var startDate = tuNgay ?? DateTime.Now.AddDays(-30);
            var endDate = denNgay ?? DateTime.Now;

            var viewModel = new VehicleMoneyViewModel();

            // Lấy dữ liệu lượt gửi
            var luotGuiQuery = await _context.LuotGuis
                .Where(lg => lg.ThoiGianRa != null && 
                           lg.ThoiGianRa >= startDate && 
                           lg.ThoiGianRa <= endDate &&
                           lg.TongTien != null)
                .ToListAsync();

            // Lấy dữ liệu thẻ tháng
            var theThangQuery = await _context.TheThangs
                .Include(tt => tt.MaKhachHangNavigation)
                .Include(tt => tt.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(tt => tt.NgayBatDau != null &&
                           tt.NgayBatDau >= DateOnly.FromDateTime(startDate) &&
                           tt.NgayBatDau <= DateOnly.FromDateTime(endDate) &&
                           tt.SoTienDong != null)
                .ToListAsync();

            // Tính tổng doanh thu
            viewModel.DoanhThuLuotGui = luotGuiQuery.Sum(lg => lg.TongTien ?? 0);
            viewModel.DoanhThuTheThang = theThangQuery.Sum(tt => tt.SoTienDong ?? 0);
            viewModel.TongDoanhThu = viewModel.DoanhThuLuotGui + viewModel.DoanhThuTheThang;
            viewModel.TongLuotGui = luotGuiQuery.Count;
            viewModel.TongTheThang = theThangQuery.Count;

            // Thống kê theo thời gian
            var today = DateTime.Now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            viewModel.DoanhThuHomNay = luotGuiQuery
                .Where(lg => lg.ThoiGianRa!.Value.Date == today)
                .Sum(lg => lg.TongTien ?? 0) +
                theThangQuery
                .Where(tt => tt.NgayBatDau == DateOnly.FromDateTime(today))
                .Sum(tt => tt.SoTienDong ?? 0);

            viewModel.DoanhThuTuanNay = luotGuiQuery
                .Where(lg => lg.ThoiGianRa!.Value >= startOfWeek)
                .Sum(lg => lg.TongTien ?? 0) +
                theThangQuery
                .Where(tt => tt.NgayBatDau >= DateOnly.FromDateTime(startOfWeek))
                .Sum(tt => tt.SoTienDong ?? 0);

            viewModel.DoanhThuThangNay = luotGuiQuery
                .Where(lg => lg.ThoiGianRa!.Value >= startOfMonth)
                .Sum(lg => lg.TongTien ?? 0) +
                theThangQuery
                .Where(tt => tt.NgayBatDau >= DateOnly.FromDateTime(startOfMonth))
                .Sum(tt => tt.SoTienDong ?? 0);

            viewModel.DoanhThuNamNay = luotGuiQuery
                .Where(lg => lg.ThoiGianRa!.Value >= startOfYear)
                .Sum(lg => lg.TongTien ?? 0) +
                theThangQuery
                .Where(tt => tt.NgayBatDau >= DateOnly.FromDateTime(startOfYear))
                .Sum(tt => tt.SoTienDong ?? 0);

            // Doanh thu lượt gửi theo ngày
            viewModel.DoanhThuLuotGuiTheoNgay = luotGuiQuery
                .GroupBy(lg => lg.ThoiGianRa!.Value.Date)
                .Select(g => new VehicleMoneyDailyDto
                {
                    Ngay = g.Key,
                    TongTien = g.Sum(lg => lg.TongTien ?? 0),
                    SoLuong = g.Count()
                })
                .OrderByDescending(d => d.Ngay)
                .ToList();

            // Doanh thu thẻ tháng theo ngày
            viewModel.DoanhThuTheThangTheoNgay = theThangQuery
                .GroupBy(tt => tt.NgayBatDau!.Value.ToDateTime(TimeOnly.MinValue))
                .Select(g => new VehicleMoneyDailyDto
                {
                    Ngay = g.Key,
                    TongTien = g.Sum(tt => tt.SoTienDong ?? 0),
                    SoLuong = g.Count()
                })
                .OrderByDescending(d => d.Ngay)
                .ToList();

            // Doanh thu theo loại xe (từ thẻ xe)
            var luotGuiByType = await _context.LuotGuis
                .Where(lg => lg.ThoiGianRa != null &&
                           lg.ThoiGianRa >= startDate &&
                           lg.ThoiGianRa <= endDate &&
                           lg.TongTien != null &&
                           lg.MaTheNavigation != null)
                .Include(lg => lg.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .ToListAsync();

            var doanhThuTheoLoaiXe = luotGuiByType
                .GroupBy(lg => lg.MaTheNavigation!.MaLoaiXeNavigation?.TenLoaiXe ?? "Chưa xác định")
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(lg => lg.TongTien ?? 0),
                    SoLuotGui = g.Count(),
                    SoTheThang = 0
                })
                .ToList();

            var doanhThuTheThangTheoLoaiXe = theThangQuery
                .GroupBy(tt => tt.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe ?? "Chưa xác định")
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(tt => tt.SoTienDong ?? 0),
                    SoLuotGui = 0,
                    SoTheThang = g.Count()
                })
                .ToList();

            // Gộp doanh thu theo loại xe
            viewModel.DoanhThuTheoLoaiXe = doanhThuTheoLoaiXe
                .Concat(doanhThuTheThangTheoLoaiXe)
                .GroupBy(d => d.TenLoaiXe)
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(d => d.TongDoanhThu),
                    SoLuotGui = g.Sum(d => d.SoLuotGui),
                    SoTheThang = g.Sum(d => d.SoTheThang),
                    TyLe = viewModel.TongDoanhThu > 0 ? (g.Sum(d => d.TongDoanhThu) / viewModel.TongDoanhThu * 100) : 0
                })
                .OrderByDescending(d => d.TongDoanhThu)
                .ToList();

            // Doanh thu theo tháng (12 tháng gần nhất)
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => today.AddMonths(-i))
                .Select(d => new { Month = d.Month, Year = d.Year })
                .Reverse()
                .ToList();

            viewModel.DoanhThuTheoThang = last12Months.Select(m =>
            {
                var monthStart = new DateTime(m.Year, m.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var doanhThuLuotGui = luotGuiQuery
                    .Where(lg => lg.ThoiGianRa!.Value >= monthStart && lg.ThoiGianRa.Value <= monthEnd)
                    .Sum(lg => lg.TongTien ?? 0);

                var doanhThuTheThang = theThangQuery
                    .Where(tt => tt.NgayBatDau >= DateOnly.FromDateTime(monthStart) && 
                               tt.NgayBatDau <= DateOnly.FromDateTime(monthEnd))
                    .Sum(tt => tt.SoTienDong ?? 0);

                return new VehicleMoneyMonthlyDto
                {
                    Thang = m.Month,
                    Nam = m.Year,
                    DoanhThuLuotGui = doanhThuLuotGui,
                    DoanhThuTheThang = doanhThuTheThang,
                    TongDoanhThu = doanhThuLuotGui + doanhThuTheThang
                };
            }).ToList();

            // Top 10 khách hàng đóng tiền nhiều nhất
            viewModel.TopKhachHang = theThangQuery
                .GroupBy(tt => new
                {
                    MaKhachHang = tt.MaKhachHang,
                    TenKhachHang = tt.MaKhachHangNavigation!.HoTen,
                    SoDienThoai = tt.MaKhachHangNavigation.SoDienThoai
                })
                .Select(g => new VehicleMoneyTopCustomerDto
                {
                    TenKhachHang = g.Key.TenKhachHang,
                    SoDienThoai = g.Key.SoDienThoai,
                    TongTienDong = g.Sum(tt => tt.SoTienDong ?? 0),
                    SoTheThang = g.Count()
                })
                .OrderByDescending(c => c.TongTienDong)
                .Take(10)
                .ToList();

            ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");
            ViewBag.LoaiDoanhThu = loaiDoanhThu ?? "all";

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay)
        {
            // TODO: Implement export to Excel functionality
            return RedirectToAction(nameof(Index), new { tuNgay, denNgay });
        }

        // Báo cáo theo ngày
        public async Task<IActionResult> Daily(DateTime? ngay)
        {
            var selectedDate = ngay ?? DateTime.Now.Date;
            var viewModel = new VehicleMoneyViewModel();

            // Lấy dữ liệu lượt gửi trong ngày
            var luotGuiQuery = await _context.LuotGuis
                .Include(lg => lg.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(lg => lg.ThoiGianRa != null &&
                           lg.ThoiGianRa.Value.Date == selectedDate &&
                           lg.TongTien != null)
                .ToListAsync();

            // Lấy dữ liệu thẻ tháng đăng ký trong ngày
            var theThangQuery = await _context.TheThangs
                .Include(tt => tt.MaKhachHangNavigation)
                .Include(tt => tt.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(tt => tt.NgayBatDau == DateOnly.FromDateTime(selectedDate) &&
                           tt.SoTienDong != null)
                .ToListAsync();

            // Tính toán thống kê
            viewModel.DoanhThuLuotGui = luotGuiQuery.Sum(lg => lg.TongTien ?? 0);
            viewModel.DoanhThuTheThang = theThangQuery.Sum(tt => tt.SoTienDong ?? 0);
            viewModel.TongDoanhThu = viewModel.DoanhThuLuotGui + viewModel.DoanhThuTheThang;
            viewModel.TongLuotGui = luotGuiQuery.Count;
            viewModel.TongTheThang = theThangQuery.Count;

            // Thống kê theo loại xe trong ngày
            var doanhThuTheoLoaiXe = luotGuiQuery
                .GroupBy(lg => lg.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe ?? "Chưa xác định")
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(lg => lg.TongTien ?? 0),
                    SoLuotGui = g.Count(),
                    SoTheThang = 0
                })
                .ToList();

            var doanhThuTheThangTheoLoaiXe = theThangQuery
                .GroupBy(tt => tt.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe ?? "Chưa xác định")
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(tt => tt.SoTienDong ?? 0),
                    SoLuotGui = 0,
                    SoTheThang = g.Count()
                })
                .ToList();

            viewModel.DoanhThuTheoLoaiXe = doanhThuTheoLoaiXe
                .Concat(doanhThuTheThangTheoLoaiXe)
                .GroupBy(d => d.TenLoaiXe)
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(d => d.TongDoanhThu),
                    SoLuotGui = g.Sum(d => d.SoLuotGui),
                    SoTheThang = g.Sum(d => d.SoTheThang)
                })
                .OrderByDescending(d => d.TongDoanhThu)
                .ToList();

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.PageTitle = "Báo cáo doanh thu theo ngày";
            
            return View("Daily", viewModel);
        }

        // Báo cáo theo tháng
        public async Task<IActionResult> Monthly(int? thang, int? nam)
        {
            var selectedMonth = thang ?? DateTime.Now.Month;
            var selectedYear = nam ?? DateTime.Now.Year;
            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var viewModel = new VehicleMoneyViewModel();

            // Lấy dữ liệu lượt gửi trong tháng
            var luotGuiQuery = await _context.LuotGuis
                .Where(lg => lg.ThoiGianRa != null &&
                           lg.ThoiGianRa >= startDate &&
                           lg.ThoiGianRa <= endDate &&
                           lg.TongTien != null)
                .ToListAsync();

            // Lấy dữ liệu thẻ tháng
            var theThangQuery = await _context.TheThangs
                .Include(tt => tt.MaKhachHangNavigation)
                .Include(tt => tt.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(tt => tt.NgayBatDau >= DateOnly.FromDateTime(startDate) &&
                           tt.NgayBatDau <= DateOnly.FromDateTime(endDate) &&
                           tt.SoTienDong != null)
                .ToListAsync();

            // Tính toán thống kê
            viewModel.DoanhThuLuotGui = luotGuiQuery.Sum(lg => lg.TongTien ?? 0);
            viewModel.DoanhThuTheThang = theThangQuery.Sum(tt => tt.SoTienDong ?? 0);
            viewModel.TongDoanhThu = viewModel.DoanhThuLuotGui + viewModel.DoanhThuTheThang;
            viewModel.TongLuotGui = luotGuiQuery.Count;
            viewModel.TongTheThang = theThangQuery.Count;

            // Doanh thu theo ngày trong tháng
            viewModel.DoanhThuLuotGuiTheoNgay = luotGuiQuery
                .GroupBy(lg => lg.ThoiGianRa!.Value.Date)
                .Select(g => new VehicleMoneyDailyDto
                {
                    Ngay = g.Key,
                    TongTien = g.Sum(lg => lg.TongTien ?? 0),
                    SoLuong = g.Count()
                })
                .OrderBy(d => d.Ngay)
                .ToList();

            viewModel.DoanhThuTheThangTheoNgay = theThangQuery
                .GroupBy(tt => tt.NgayBatDau!.Value.ToDateTime(TimeOnly.MinValue))
                .Select(g => new VehicleMoneyDailyDto
                {
                    Ngay = g.Key,
                    TongTien = g.Sum(tt => tt.SoTienDong ?? 0),
                    SoLuong = g.Count()
                })
                .OrderBy(d => d.Ngay)
                .ToList();

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.PageTitle = "Báo cáo doanh thu theo tháng";

            return View("Monthly", viewModel);
        }

        // Báo cáo theo loại xe
        public async Task<IActionResult> ByVehicleType(DateTime? tuNgay, DateTime? denNgay)
        {
            var startDate = tuNgay ?? DateTime.Now.AddDays(-30);
            var endDate = denNgay ?? DateTime.Now;

            var viewModel = new VehicleMoneyViewModel();

            // Lấy dữ liệu lượt gửi theo loại xe
            var luotGuiByType = await _context.LuotGuis
                .Where(lg => lg.ThoiGianRa != null &&
                           lg.ThoiGianRa >= startDate &&
                           lg.ThoiGianRa <= endDate &&
                           lg.TongTien != null &&
                           lg.MaTheNavigation != null)
                .Include(lg => lg.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .ToListAsync();

            // Lấy dữ liệu thẻ tháng theo loại xe
            var theThangByType = await _context.TheThangs
                .Include(tt => tt.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(tt => tt.NgayBatDau >= DateOnly.FromDateTime(startDate) &&
                           tt.NgayBatDau <= DateOnly.FromDateTime(endDate) &&
                           tt.SoTienDong != null)
                .ToListAsync();

            // Tính toán theo loại xe
            var doanhThuTheoLoaiXe = luotGuiByType
                .GroupBy(lg => lg.MaTheNavigation!.MaLoaiXeNavigation?.TenLoaiXe ?? "Chưa xác định")
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(lg => lg.TongTien ?? 0),
                    SoLuotGui = g.Count(),
                    SoTheThang = 0
                })
                .ToList();

            var doanhThuTheThangTheoLoaiXe = theThangByType
                .GroupBy(tt => tt.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe ?? "Chưa xác định")
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(tt => tt.SoTienDong ?? 0),
                    SoLuotGui = 0,
                    SoTheThang = g.Count()
                })
                .ToList();

            viewModel.DoanhThuTheoLoaiXe = doanhThuTheoLoaiXe
                .Concat(doanhThuTheThangTheoLoaiXe)
                .GroupBy(d => d.TenLoaiXe)
                .Select(g => new VehicleMoneyByTypeDto
                {
                    TenLoaiXe = g.Key,
                    TongDoanhThu = g.Sum(d => d.TongDoanhThu),
                    SoLuotGui = g.Sum(d => d.SoLuotGui),
                    SoTheThang = g.Sum(d => d.SoTheThang)
                })
                .OrderByDescending(d => d.TongDoanhThu)
                .ToList();

            viewModel.TongDoanhThu = viewModel.DoanhThuTheoLoaiXe.Sum(d => d.TongDoanhThu);
            viewModel.DoanhThuLuotGui = viewModel.DoanhThuTheoLoaiXe.Sum(d => d.SoLuotGui);
            viewModel.DoanhThuTheThang = viewModel.DoanhThuTheoLoaiXe.Sum(d => d.SoTheThang);

            // Tính tỷ lệ
            foreach (var item in viewModel.DoanhThuTheoLoaiXe)
            {
                item.TyLe = viewModel.TongDoanhThu > 0 ? (item.TongDoanhThu / viewModel.TongDoanhThu * 100) : 0;
            }

            ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");
            ViewBag.PageTitle = "Báo cáo doanh thu theo loại xe";

            return View("ByVehicleType", viewModel);
        }

        // Báo cáo theo cổng (ca làm việc)
        public async Task<IActionResult> ByGate(DateTime? tuNgay, DateTime? denNgay)
        {
            var startDate = tuNgay ?? DateTime.Now.AddDays(-30);
            var endDate = denNgay ?? DateTime.Now;

            // Lấy dữ liệu lượt gửi theo ca vào
            var luotGuiData = await _context.LuotGuis
                .Where(lg => lg.ThoiGianRa != null &&
                           lg.ThoiGianRa >= startDate &&
                           lg.ThoiGianRa <= endDate &&
                           lg.TongTien != null)
                .Include(lg => lg.MaCaVaoNavigation)
                    .ThenInclude(ca => ca!.MaNhanVienNavigation)
                .Include(lg => lg.MaCaRaNavigation)
                    .ThenInclude(ca => ca!.MaNhanVienNavigation)
                .ToListAsync();

            // Hàm helper để lấy tên ca
            string GetTenCa(int? maCa)
            {
                if (maCa == null) return "Chưa xác định";
                return $"Ca #{maCa}";
            }

            // Nhóm theo ca vào
            var doanhThuTheoCaVao = luotGuiData
                .Where(lg => lg.MaCaVao != null)
                .GroupBy(lg => new
                {
                    MaCa = lg.MaCaVao,
                    TenCa = GetTenCa(lg.MaCaVao),
                    TenNhanVien = lg.MaCaVaoNavigation?.MaNhanVienNavigation?.HoTen ?? "N/A"
                })
                .Select(g => new
                {
                    TenCa = g.Key.TenCa + " - " + g.Key.TenNhanVien,
                    TongTien = g.Sum(lg => lg.TongTien ?? 0),
                    SoLuong = g.Count()
                })
                .OrderByDescending(d => d.TongTien)
                .ToList();

            // Nhóm theo ca ra
            var doanhThuTheoCaRa = luotGuiData
                .Where(lg => lg.MaCaRa != null)
                .GroupBy(lg => new
                {
                    MaCa = lg.MaCaRa,
                    TenCa = GetTenCa(lg.MaCaRa),
                    TenNhanVien = lg.MaCaRaNavigation?.MaNhanVienNavigation?.HoTen ?? "N/A"
                })
                .Select(g => new
                {
                    TenCa = g.Key.TenCa + " - " + g.Key.TenNhanVien,
                    TongTien = g.Sum(lg => lg.TongTien ?? 0),
                    SoLuong = g.Count()
                })
                .OrderByDescending(d => d.TongTien)
                .ToList();

            ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");
            ViewBag.PageTitle = "Báo cáo doanh thu theo cổng (Ca làm việc)";
            ViewBag.DoanhThuTheoCaVao = doanhThuTheoCaVao;
            ViewBag.DoanhThuTheoCaRa = doanhThuTheoCaRa;
            ViewBag.TongDoanhThu = luotGuiData.Sum(lg => lg.TongTien ?? 0);
            ViewBag.TongLuotGui = luotGuiData.Count;

            return View("ByGate");
        }
    }
}

