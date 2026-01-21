using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleHistoryController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleHistoryController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vehicleTypes = await _context.LoaiXes
                .Select(l => new LoaiXeDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe
                })
                .ToListAsync();

            // Get statistics
            var totalVehicles = await _context.LuotGuis.CountAsync();
            var completedCount = await _context.LuotGuis.CountAsync(l => l.TrangThai == 1);
            var inProgressCount = await _context.LuotGuis.CountAsync(l => l.TrangThai == 0);
            var totalRevenue = await _context.LuotGuis
                .Where(l => l.TongTien.HasValue)
                .SumAsync(l => l.TongTien ?? 0);

            var model = new VehicleHistoryViewModel
            {
                VehicleTypes = vehicleTypes,
                TotalVehicles = totalVehicles,
                CompletedCount = completedCount,
                InProgressCount = inProgressCount,
                TotalRevenue = totalRevenue
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicles([FromQuery] VehicleHistorySearchRequest request)
        {
            var query = _context.LuotGuis
                .Include(l => l.MaViTriNavigation)
                    .ThenInclude(v => v!.MaKhuVucNavigation)
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .AsQueryable();

            // Filter by date range
            if (request.FromDate.HasValue)
            {
                query = query.Where(l => l.ThoiGianVao >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDateEnd = request.ToDate.Value.AddDays(1);
                query = query.Where(l => l.ThoiGianVao < toDateEnd);
            }

            // Filter by keyword (license plate or card code)
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(l =>
                    (l.BienSoVao != null && l.BienSoVao.ToLower().Contains(keyword)) ||
                    (l.BienSoRa != null && l.BienSoRa.ToLower().Contains(keyword)) ||
                    (l.MaThe != null && l.MaThe.ToLower().Contains(keyword)));
            }

            // Filter by vehicle type
            if (request.LoaiXe.HasValue)
            {
                query = query.Where(l => l.MaTheNavigation != null &&
                    l.MaTheNavigation.MaLoaiXe == request.LoaiXe.Value);
            }

            // Filter by status
            if (request.TrangThai.HasValue)
            {
                query = query.Where(l => l.TrangThai == request.TrangThai.Value);
            }

            var vehicles = await query
                .OrderByDescending(l => l.ThoiGianVao)
                .Select(l => new VehicleHistoryDto
                {
                    MaLuotGui = l.MaLuotGui,
                    MaThe = l.MaThe,
                    BienSoVao = l.BienSoVao,
                    BienSoRa = l.BienSoRa,
                    HinhAnhVao = l.HinhAnhVao,
                    HinhAnhRa = l.HinhAnhRa,
                    ThoiGianVao = l.ThoiGianVao,
                    ThoiGianRa = l.ThoiGianRa,
                    TenViTri = l.MaViTriNavigation != null ? l.MaViTriNavigation.TenViTri : null,
                    TenKhuVuc = l.MaViTriNavigation != null && l.MaViTriNavigation.MaKhuVucNavigation != null
                        ? l.MaViTriNavigation.MaKhuVucNavigation.TenKhuVuc : null,
                    TenLoaiXe = l.MaTheNavigation != null && l.MaTheNavigation.MaLoaiXeNavigation != null
                        ? l.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    TongTien = l.TongTien,
                    TrangThai = l.TrangThai
                })
                .ToListAsync();

            return Json(new { data = vehicles });
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleDetail(long id)
        {
            var vehicle = await _context.LuotGuis
                .Include(l => l.MaViTriNavigation)
                    .ThenInclude(v => v!.MaKhuVucNavigation)
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(l => l.MaLuotGui == id)
                .Select(l => new VehicleHistoryDto
                {
                    MaLuotGui = l.MaLuotGui,
                    MaThe = l.MaThe,
                    BienSoVao = l.BienSoVao,
                    BienSoRa = l.BienSoRa,
                    HinhAnhVao = l.HinhAnhVao,
                    HinhAnhRa = l.HinhAnhRa,
                    ThoiGianVao = l.ThoiGianVao,
                    ThoiGianRa = l.ThoiGianRa,
                    TenViTri = l.MaViTriNavigation != null ? l.MaViTriNavigation.TenViTri : null,
                    TenKhuVuc = l.MaViTriNavigation != null && l.MaViTriNavigation.MaKhuVucNavigation != null
                        ? l.MaViTriNavigation.MaKhuVucNavigation.TenKhuVuc : null,
                    TenLoaiXe = l.MaTheNavigation != null && l.MaTheNavigation.MaLoaiXeNavigation != null
                        ? l.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    TongTien = l.TongTien,
                    TrangThai = l.TrangThai
                })
                .FirstOrDefaultAsync();

            if (vehicle == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy thông tin lượt gửi" });
            }

            return Json(new { success = true, data = vehicle });
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics([FromQuery] VehicleHistorySearchRequest request)
        {
            var query = _context.LuotGuis.AsQueryable();

            // Apply same filters
            if (request.FromDate.HasValue)
            {
                query = query.Where(l => l.ThoiGianVao >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDateEnd = request.ToDate.Value.AddDays(1);
                query = query.Where(l => l.ThoiGianVao < toDateEnd);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(l =>
                    (l.BienSoVao != null && l.BienSoVao.ToLower().Contains(keyword)) ||
                    (l.BienSoRa != null && l.BienSoRa.ToLower().Contains(keyword)) ||
                    (l.MaThe != null && l.MaThe.ToLower().Contains(keyword)));
            }

            if (request.LoaiXe.HasValue)
            {
                query = query.Where(l => l.MaTheNavigation != null &&
                    l.MaTheNavigation.MaLoaiXe == request.LoaiXe.Value);
            }

            if (request.TrangThai.HasValue)
            {
                query = query.Where(l => l.TrangThai == request.TrangThai.Value);
            }

            var totalVehicles = await query.CountAsync();
            var completedCount = await query.CountAsync(l => l.TrangThai == 1);
            var inProgressCount = await query.CountAsync(l => l.TrangThai == 0);
            var totalRevenue = await query
                .Where(l => l.TongTien.HasValue)
                .SumAsync(l => l.TongTien ?? 0);

            return Json(new
            {
                totalVehicles,
                completedCount,
                inProgressCount,
                totalRevenue
            });
        }
    }
}
