using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
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

        [HttpGet]
        public async Task<IActionResult> ExportExcel([FromQuery] VehicleHistorySearchRequest request)
        {
            try
            {
                // Set EPPlus License Context
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var query = _context.LuotGuis
                    .Include(l => l.MaViTriNavigation)
                        .ThenInclude(v => v!.MaKhuVucNavigation)
                    .Include(l => l.MaTheNavigation)
                        .ThenInclude(t => t!.MaLoaiXeNavigation)
                    .AsQueryable();

                // Apply filters
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

                var vehicles = await query
                    .OrderByDescending(l => l.ThoiGianVao)
                    .Select(l => new
                    {
                        l.MaLuotGui,
                        l.MaThe,
                        l.BienSoVao,
                        l.BienSoRa,
                        l.ThoiGianVao,
                        l.ThoiGianRa,
                        TenViTri = l.MaViTriNavigation != null ? l.MaViTriNavigation.TenViTri : null,
                        TenKhuVuc = l.MaViTriNavigation != null && l.MaViTriNavigation.MaKhuVucNavigation != null
                            ? l.MaViTriNavigation.MaKhuVucNavigation.TenKhuVuc : null,
                        TenLoaiXe = l.MaTheNavigation != null && l.MaTheNavigation.MaLoaiXeNavigation != null
                            ? l.MaTheNavigation.MaLoaiXeNavigation.TenLoaiXe : null,
                        l.TongTien,
                        l.TrangThai
                    })
                    .ToListAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lịch sử gửi xe");

                    // Header
                    worksheet.Cells[1, 1].Value = "STT";
                    worksheet.Cells[1, 2].Value = "Mã lượt gửi";
                    worksheet.Cells[1, 3].Value = "Mã thẻ";
                    worksheet.Cells[1, 4].Value = "Biển số vào";
                    worksheet.Cells[1, 5].Value = "Biển số ra";
                    worksheet.Cells[1, 6].Value = "Loại xe";
                    worksheet.Cells[1, 7].Value = "Vị trí";
                    worksheet.Cells[1, 8].Value = "Khu vực";
                    worksheet.Cells[1, 9].Value = "Thời gian vào";
                    worksheet.Cells[1, 10].Value = "Thời gian ra";
                    worksheet.Cells[1, 11].Value = "Thời gian gửi";
                    worksheet.Cells[1, 12].Value = "Tổng tiền";
                    worksheet.Cells[1, 13].Value = "Trạng thái";

                    // Style header
                    using (var range = worksheet.Cells[1, 1, 1, 13])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    // Data
                    int row = 2;
                    foreach (var vehicle in vehicles)
                    {
                        worksheet.Cells[row, 1].Value = row - 1;
                        worksheet.Cells[row, 2].Value = vehicle.MaLuotGui;
                        worksheet.Cells[row, 3].Value = vehicle.MaThe ?? "--";
                        worksheet.Cells[row, 4].Value = vehicle.BienSoVao ?? "--";
                        worksheet.Cells[row, 5].Value = vehicle.BienSoRa ?? "--";
                        worksheet.Cells[row, 6].Value = vehicle.TenLoaiXe ?? "--";
                        worksheet.Cells[row, 7].Value = vehicle.TenViTri ?? "--";
                        worksheet.Cells[row, 8].Value = vehicle.TenKhuVuc ?? "--";
                        
                        // ThoiGianVao is DateTime (not nullable)
                        worksheet.Cells[row, 9].Value = vehicle.ThoiGianVao.ToString("dd/MM/yyyy HH:mm:ss");

                        if (vehicle.ThoiGianRa.HasValue)
                        {
                            worksheet.Cells[row, 10].Value = vehicle.ThoiGianRa.Value.ToString("dd/MM/yyyy HH:mm:ss");
                        }
                        else
                        {
                            worksheet.Cells[row, 10].Value = "--";
                        }

                        // Calculate duration
                        var endTime = vehicle.ThoiGianRa ?? DateTime.Now;
                        var duration = endTime - vehicle.ThoiGianVao;
                        if (duration.TotalDays >= 1)
                        {
                            worksheet.Cells[row, 11].Value = $"{(int)duration.TotalDays} ngày {duration.Hours:D2}:{duration.Minutes:D2}";
                        }
                        else
                        {
                            worksheet.Cells[row, 11].Value = $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
                        }

                        if (vehicle.TongTien.HasValue)
                        {
                            worksheet.Cells[row, 12].Value = vehicle.TongTien.Value;
                            worksheet.Cells[row, 12].Style.Numberformat.Format = "#,##0 ₫";
                        }
                        else
                        {
                            worksheet.Cells[row, 12].Value = "--";
                        }

                        worksheet.Cells[row, 13].Value = vehicle.TrangThai == 0 ? "Đang gửi" : "Đã lấy xe";

                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Add borders
                    using (var range = worksheet.Cells[1, 1, row - 1, 13])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    var fileName = $"LichSuGuiXe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi xuất file: " + ex.Message });
            }
        }
    }
}
