using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class DashboardController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public DashboardController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // Xe trong bãi (TrangThai = 0: đang gửi)
            var xeTrongBai = await _context.LuotGuis.CountAsync(l => l.TrangThai == 0);
            var xeTrongBaiHomQua = await _context.LuotGuis
                .CountAsync(l => l.ThoiGianVao.Date == yesterday && l.TrangThai == 0);

            // Xe vào hôm nay
            var xeVaoHomNay = await _context.LuotGuis.CountAsync(l => l.ThoiGianVao.Date == today);
            var xeVaoHomQua = await _context.LuotGuis.CountAsync(l => l.ThoiGianVao.Date == yesterday);

            // Xe ra hôm nay
            var xeRaHomNay = await _context.LuotGuis.CountAsync(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == today);
            var xeRaHomQua = await _context.LuotGuis.CountAsync(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == yesterday);

            // Doanh thu hôm nay
            var doanhThuHomNay = await _context.LuotGuis
                .Where(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == today && l.TongTien.HasValue)
                .SumAsync(l => l.TongTien ?? 0);
            var doanhThuHomQua = await _context.LuotGuis
                .Where(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == yesterday && l.TongTien.HasValue)
                .SumAsync(l => l.TongTien ?? 0);

            // Đặt chỗ chờ duyệt
            var pendingBookings = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 0);

            // Tính tỷ lệ thay đổi
            decimal tyLeXeTrongBai = xeTrongBaiHomQua > 0 ? ((decimal)(xeTrongBai - xeTrongBaiHomQua) / xeTrongBaiHomQua * 100) : 0;
            decimal tyLeXeVao = xeVaoHomQua > 0 ? ((decimal)(xeVaoHomNay - xeVaoHomQua) / xeVaoHomQua * 100) : 0;
            decimal tyLeXeRa = xeRaHomQua > 0 ? ((decimal)(xeRaHomNay - xeRaHomQua) / xeRaHomQua * 100) : 0;
            decimal tyLeDoanhThu = doanhThuHomQua > 0 ? ((doanhThuHomNay - doanhThuHomQua) / doanhThuHomQua * 100) : 0;

            // Tình trạng bãi đỗ theo loại xe
            var loaiXes = await _context.LoaiXes.ToListAsync();
            var tinhTrangBaiDo = new List<ParkingStatusByType>();
            var mauSac = new[] { "#21A691", "#87DF2C", "#ff9800", "#e74c3c", "#9b59b6", "#3498db" };
            int colorIndex = 0;

            foreach (var loaiXe in loaiXes)
            {
                var soLuongDangGui = await _context.LuotGuis
                    .Include(l => l.MaTheNavigation)
                    .CountAsync(l => l.TrangThai == 0 && l.MaTheNavigation != null && l.MaTheNavigation.MaLoaiXe == loaiXe.MaLoaiXe);

                var tongSoViTri = await _context.ViTriDos.CountAsync();
                var viTriTheoLoai = tongSoViTri > 0 ? tongSoViTri / loaiXes.Count : 50; // Ước tính

                tinhTrangBaiDo.Add(new ParkingStatusByType
                {
                    MaLoaiXe = loaiXe.MaLoaiXe,
                    TenLoaiXe = loaiXe.TenLoaiXe ?? "Không xác định",
                    SoLuongDangGui = soLuongDangGui,
                    TongSoViTri = viTriTheoLoai,
                    MauSac = mauSac[colorIndex % mauSac.Length]
                });
                colorIndex++;
            }

            // Vé tháng sắp hết hạn (trong 14 ngày tới)
            var ngayHetHan = DateOnly.FromDateTime(today.AddDays(14));
            var veThangSapHetHan = await _context.TheThangs
                .Include(v => v.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Include(v => v.MaKhachHangNavigation)
                .Where(v => v.TrangThai == true && v.NgayHetHan.HasValue && v.NgayHetHan <= ngayHetHan)
                .OrderBy(v => v.NgayHetHan)
                .Take(5)
                .Select(v => new ExpiringMonthlyTicket
                {
                    MaVeThang = v.MaTheThang,
                    BienSo = v.MaKhachHangNavigation != null ? v.MaKhachHangNavigation.BienSoXeMacDinh : null,
                    TenLoaiXe = v.MaTheNavigation != null && v.MaTheNavigation.MaLoaiXeNavigation != null 
                        ? v.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    NgayHetHan = v.NgayHetHan,
                    SoNgayConLai = v.NgayHetHan.HasValue 
                        ? (v.NgayHetHan.Value.ToDateTime(TimeOnly.MinValue) - today).Days 
                        : 0
                })
                .ToListAsync();

            // Hoạt động gần đây (20 bản ghi mới nhất)
            var hoatDongGanDay = await _context.LuotGuis
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .OrderByDescending(l => l.ThoiGianVao)
                .Take(20)
                .Select(l => new RecentActivityDto
                {
                    MaLuotGui = l.MaLuotGui,
                    BienSoVao = l.BienSoVao,
                    BienSoRa = l.BienSoRa,
                    TenLoaiXe = l.MaTheNavigation != null && l.MaTheNavigation.MaLoaiXeNavigation != null 
                        ? l.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    ThoiGianVao = l.ThoiGianVao,
                    ThoiGianRa = l.ThoiGianRa,
                    TongTien = l.TongTien,
                    TrangThai = l.TrangThai
                })
                .ToListAsync();

            // Doanh thu 7 ngày gần nhất
            var doanhThuTheoNgay = new List<DailyRevenueDto>();
            for (int i = 6; i >= 0; i--)
            {
                var ngay = today.AddDays(-i);
                var doanhThu = await _context.LuotGuis
                    .Where(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == ngay && l.TongTien.HasValue)
                    .SumAsync(l => l.TongTien ?? 0);
                var soLuot = await _context.LuotGuis
                    .CountAsync(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == ngay);

                doanhThuTheoNgay.Add(new DailyRevenueDto
                {
                    Ngay = ngay,
                    DoanhThu = doanhThu,
                    SoLuotGui = soLuot
                });
            }

            // Doanh thu 6 tháng gần nhất
            var doanhThuTheoThang = new List<MonthlyRevenueDto>();
            for (int i = 5; i >= 0; i--)
            {
                var thang = today.AddMonths(-i);
                var doanhThu = await _context.LuotGuis
                    .Where(l => l.ThoiGianRa.HasValue 
                        && l.ThoiGianRa.Value.Month == thang.Month 
                        && l.ThoiGianRa.Value.Year == thang.Year 
                        && l.TongTien.HasValue)
                    .SumAsync(l => l.TongTien ?? 0);
                var soLuot = await _context.LuotGuis
                    .CountAsync(l => l.ThoiGianRa.HasValue 
                        && l.ThoiGianRa.Value.Month == thang.Month 
                        && l.ThoiGianRa.Value.Year == thang.Year);

                doanhThuTheoThang.Add(new MonthlyRevenueDto
                {
                    Thang = thang.Month,
                    Nam = thang.Year,
                    DoanhThu = doanhThu,
                    SoLuotGui = soLuot
                });
            }

            var model = new DashboardViewModel
            {
                XeTrongBai = xeTrongBai,
                XeVaoHomNay = xeVaoHomNay,
                XeRaHomNay = xeRaHomNay,
                DoanhThuHomNay = doanhThuHomNay,
                PendingBookings = pendingBookings,
                TyLeXeTrongBai = Math.Round(tyLeXeTrongBai, 1),
                TyLeXeVao = Math.Round(tyLeXeVao, 1),
                TyLeXeRa = Math.Round(tyLeXeRa, 1),
                TyLeDoanhThu = Math.Round(tyLeDoanhThu, 1),
                TinhTrangBaiDo = tinhTrangBaiDo,
                VeThangSapHetHan = veThangSapHetHan,
                HoatDongGanDay = hoatDongGanDay,
                DoanhThuTheoNgay = doanhThuTheoNgay,
                DoanhThuTheoThang = doanhThuTheoThang
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            var today = DateTime.Today;

            var xeTrongBai = await _context.LuotGuis.CountAsync(l => l.TrangThai == 0);
            var xeVaoHomNay = await _context.LuotGuis.CountAsync(l => l.ThoiGianVao.Date == today);
            var xeRaHomNay = await _context.LuotGuis.CountAsync(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == today);
            var doanhThuHomNay = await _context.LuotGuis
                .Where(l => l.ThoiGianRa.HasValue && l.ThoiGianRa.Value.Date == today && l.TongTien.HasValue)
                .SumAsync(l => l.TongTien ?? 0);

            return Json(new
            {
                xeTrongBai,
                xeVaoHomNay,
                xeRaHomNay,
                doanhThuHomNay
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentActivities(int count = 10)
        {
            var activities = await _context.LuotGuis
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .OrderByDescending(l => l.ThoiGianVao)
                .Take(count)
                .Select(l => new
                {
                    l.MaLuotGui,
                    l.BienSoVao,
                    l.BienSoRa,
                    TenLoaiXe = l.MaTheNavigation != null && l.MaTheNavigation.MaLoaiXeNavigation != null
                        ? l.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                    l.ThoiGianVao,
                    l.ThoiGianRa,
                    l.TongTien,
                    l.TrangThai
                })
                .ToListAsync();

            return Json(new { data = activities });
        }
    }
}
