using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using System.Globalization;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleVisionController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleVisionController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Load tr?ng thái 3 qu?y t? CaLamViec
            var countersStatus = new List<dynamic>();

            for (int i = 1; i <= 3; i++)
            {
                // Tìm ca làm vi?c ?ang ho?t ??ng cho qu?y này
                var activeShift = await _context.CaLamViecs
                    .Include(c => c.MaNhanVienNavigation)
                    .Where(c => c.TrangThaiCa == 0 && 
                                c.GhiChuBanGiao != null && 
                                c.GhiChuBanGiao.Contains($"Phân công qu?y {i}"))
                    .OrderByDescending(c => c.ThoiGianNhanCa)
                    .FirstOrDefaultAsync();

                if (activeShift != null && activeShift.MaNhanVienNavigation != null)
                {
                    // Qu?y ?ang ho?t ??ng
                    var soGioLam = activeShift.ThoiGianNhanCa.HasValue 
                        ? (DateTime.Now - activeShift.ThoiGianNhanCa.Value).TotalHours 
                        : 0;

                    // Tính doanh thu t? LuotGuis trong ca
                    var revenue = await _context.LuotGuis
                        .Where(l => (l.MaCaVao == activeShift.MaCa || l.MaCaRa == activeShift.MaCa) 
                                 && l.TongTien.HasValue)
                        .SumAsync(l => l.TongTien ?? 0);

                    countersStatus.Add(new
                    {
                        Counter = i,
                        IsActive = true,
                        EmployeeName = activeShift.MaNhanVienNavigation.HoTen,
                        EmployeeCode = $"NV{activeShift.MaNhanVien:D4}",
                        ShiftHours = Math.Round(soGioLam, 1),
                        Revenue = revenue,
                        RevenueFormatted = revenue.ToString("N0", new CultureInfo("vi-VN")) + " VN?"
                    });
                }
                else
                {
                    // Qu?y không ho?t ??ng
                    countersStatus.Add(new
                    {
                        Counter = i,
                        IsActive = false,
                        EmployeeName = (string?)null,
                        EmployeeCode = (string?)null,
                        ShiftHours = (double?)null,
                        Revenue = 0m,
                        RevenueFormatted = "0 VN?"
                    });
                }
            }

            // Truy?n data xu?ng View
            ViewBag.CountersStatus = countersStatus;

            return View();
        }
    }
}
