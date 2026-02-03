using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;

namespace QuanLyBaiDoXe.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Customer")]
    public class ParkingMapController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public ParkingMapController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetParkingSpots()
        {
            var spots = await _context.ViTriDos
                .Include(vt => vt.MaKhuVucNavigation)
                .Select(vt => new
                {
                    maViTri = vt.MaViTri,
                    tenViTri = vt.TenViTri,
                    maKhuVuc = vt.MaKhuVuc,
                    tenKhuVuc = vt.MaKhuVucNavigation!.TenKhuVuc,
                    trangThai = vt.TrangThai
                })
                .ToListAsync();

            var khuVucs = await _context.KhuVucs
                .Select(kv => new
                {
                    maKhuVuc = kv.MaKhuVuc,
                    tenKhuVuc = kv.TenKhuVuc
                })
                .ToListAsync();

            return Json(new { success = true, spots, khuVucs });
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var totalSpots = await _context.ViTriDos.CountAsync();
            var availableSpots = await _context.ViTriDos.CountAsync(vt => vt.TrangThai == 0);
            var occupiedSpots = await _context.ViTriDos.CountAsync(vt => vt.TrangThai == 1);
            var bookedSpots = await _context.ViTriDos.CountAsync(vt => vt.TrangThai == 2);

            return Json(new
            {
                success = true,
                totalSpots,
                availableSpots,
                occupiedSpots,
                bookedSpots,
                occupancyRate = totalSpots > 0 ? Math.Round((double)occupiedSpots / totalSpots * 100, 1) : 0
            });
        }
    }
}
