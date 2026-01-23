using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AnomalyMonitorController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public AnomalyMonitorController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        // Màn hình giám sát realtime
        public IActionResult RealTimeMonitor()
        {
            return View();
        }

        // API l?y s? c? realtime
        [HttpGet]
        public async Task<IActionResult> GetRealtimeAnomalies()
        {
            var recentAnomalies = await _context.SuCos
                .Include(s => s.MaNhanVienNavigation)
                .Where(s => s.TrangThaiXuLy != 2) // Ch? l?y ch?a x? lý và ?ang x? lý
                .OrderByDescending(s => s.ThoiGianGhiNhan)
                .Take(20)
                .Select(s => new
                {
                    maSuCo = s.MaSuCo,
                    thoiGianGhiNhan = s.ThoiGianGhiNhan,
                    loaiSuCo = s.LoaiSuCo,
                    maThe = s.MaThe,
                    maViTri = s.MaViTri,
                    moTaChiTiet = s.MoTaChiTiet,
                    trangThaiXuLy = s.TrangThaiXuLy,
                    tenNhanVien = s.MaNhanVienNavigation != null ? s.MaNhanVienNavigation.HoTen : null
                })
                .ToListAsync();

            return Json(recentAnomalies);
        }

        // Th?ng kê theo th?i gian
        [HttpGet]
        public async Task<IActionResult> GetStatisticsByTime(DateTime? fromDate, DateTime? toDate)
        {
            fromDate ??= DateTime.Today.AddDays(-30);
            toDate ??= DateTime.Today;

            var statistics = await _context.SuCos
                .Where(s => s.ThoiGianGhiNhan >= fromDate && s.ThoiGianGhiNhan <= toDate)
                .GroupBy(s => s.ThoiGianGhiNhan.Value.Date)
                .Select(g => new
                {
                    date = g.Key,
                    total = g.Count(),
                    resolved = g.Count(s => s.TrangThaiXuLy == 2),
                    pending = g.Count(s => s.TrangThaiXuLy == 0),
                    processing = g.Count(s => s.TrangThaiXuLy == 1)
                })
                .OrderBy(s => s.date)
                .ToListAsync();

            return Json(statistics);
        }

        // Th?ng kê theo lo?i s? c?
        [HttpGet]
        public async Task<IActionResult> GetStatisticsByType()
        {
            var statistics = await _context.SuCos
                .GroupBy(s => s.LoaiSuCo)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count(),
                    resolved = g.Count(s => s.TrangThaiXuLy == 2),
                    avgResolutionTime = g.Where(s => s.TrangThaiXuLy == 2).Count() // Simplified
                })
                .OrderByDescending(s => s.count)
                .ToListAsync();

            return Json(statistics);
        }
    }
}
