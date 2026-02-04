using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Services;

namespace QuanLyBaiDoXe.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Customer")]
    public class ParkingMapController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;
        private readonly IReservationService _reservationService;

        public ParkingMapController(
            QuanLyBaiDoXeContext context,
            IReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetParkingSpots(int? maLoaiXe = null, DateTime? thoiGianDenDuKien = null)
        {
            var query = _context.ViTriDos
                .Include(vt => vt.MaKhuVucNavigation)
                .ThenInclude(kv => kv!.MaLoaiXeNavigation)
                .AsQueryable();

            // Lọc theo loại xe nếu có
            if (maLoaiXe.HasValue)
            {
                query = query.Where(vt =>
                    vt.MaKhuVucNavigation!.MaLoaiXe == null ||
                    vt.MaKhuVucNavigation.MaLoaiXe == maLoaiXe);
            }

            var spots = await query
                .Select(vt => new
                {
                    maViTri = vt.MaViTri,
                    tenViTri = vt.TenViTri,
                    maKhuVuc = vt.MaKhuVuc,
                    tenKhuVuc = vt.MaKhuVucNavigation!.TenKhuVuc,
                    maLoaiXe = vt.MaKhuVucNavigation.MaLoaiXe,
                    tenLoaiXe = vt.MaKhuVucNavigation.MaLoaiXeNavigation != null
                        ? vt.MaKhuVucNavigation.MaLoaiXeNavigation.TenLoaiXe
                        : null,
                    trangThai = vt.TrangThai,
                    trangThaiText = vt.TrangThai == 0 ? "Trống" :
                                    vt.TrangThai == 1 ? "Đã đỗ" :
                                    vt.TrangThai == 2 ? "Đã đặt" : "Không xác định"
                })
                .ToListAsync();

            // Kiểm tra vị trí đã đặt chỗ nếu có thời gian
            if (thoiGianDenDuKien.HasValue)
            {
                var viTriDaDat = await _context.DatChos
                    .Where(dc =>
                        dc.ThoiGianDenDuKien <= thoiGianDenDuKien &&
                        dc.ThoiGianHetHan >= thoiGianDenDuKien &&
                        dc.TrangThaiDatCho != 3 && // Không bị từ chối
                        dc.TrangThaiDatCho != 4 && // Không bị hủy
                        dc.TrangThaiDatCho != 5)   // Không hết hạn
                    .Select(dc => dc.MaViTri)
                    .ToListAsync();

                spots = spots.Select(s => new
                {
                    s.maViTri,
                    s.tenViTri,
                    s.maKhuVuc,
                    s.tenKhuVuc,
                    s.maLoaiXe,
                    s.tenLoaiXe,
                    trangThai = viTriDaDat.Contains(s.maViTri) ? 2 : s.trangThai,
                    trangThaiText = viTriDaDat.Contains(s.maViTri) ? "Đã đặt" : s.trangThaiText
                }).ToList();
            }

            var khuVucs = await _context.KhuVucs
                .Include(kv => kv.MaLoaiXeNavigation)
                .Select(kv => new
                {
                    maKhuVuc = kv.MaKhuVuc,
                    tenKhuVuc = kv.TenKhuVuc,
                    maLoaiXe = kv.MaLoaiXe,
                    tenLoaiXe = kv.MaLoaiXeNavigation != null ? kv.MaLoaiXeNavigation.TenLoaiXe : null
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

        [HttpGet]
        public async Task<IActionResult> GetLoaiXes()
        {
            var loaiXes = await _context.LoaiXes
                .Select(lx => new
                {
                    maLoaiXe = lx.MaLoaiXe,
                    tenLoaiXe = lx.TenLoaiXe
                })
                .ToListAsync();

            return Json(new { success = true, loaiXes });
        }
    }
}
