using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleAnomalyController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleAnomalyController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(AnomalyFilterViewModel filter)
        {
            // Thống kê tổng quan
            var stats = new AnomalyStatisticsViewModel
            {
                TongSuCo = await _context.SuCos.CountAsync(),
                ChuaXuLy = await _context.SuCos.CountAsync(s => s.TrangThaiXuLy == 0),
                DangXuLy = await _context.SuCos.CountAsync(s => s.TrangThaiXuLy == 1),
                DaXuLy = await _context.SuCos.CountAsync(s => s.TrangThaiXuLy == 2),
                KhanCap = await _context.SuCos.CountAsync(s => s.LoaiSuCo == "Khẩn cấp" && s.TrangThaiXuLy != 2)
            };
            ViewBag.Statistics = stats;

            // Lấy danh sách sự cố
            var query = _context.SuCos
                .Include(s => s.MaNhanVienNavigation)
                .AsQueryable();

            // Áp dụng bộ lọc
            if (filter.TuNgay.HasValue)
                query = query.Where(s => s.ThoiGianGhiNhan >= filter.TuNgay.Value);

            if (filter.DenNgay.HasValue)
                query = query.Where(s => s.ThoiGianGhiNhan <= filter.DenNgay.Value);

            if (!string.IsNullOrEmpty(filter.LoaiSuCo))
                query = query.Where(s => s.LoaiSuCo == filter.LoaiSuCo);

            if (filter.TrangThaiXuLy.HasValue)
                query = query.Where(s => s.TrangThaiXuLy == filter.TrangThaiXuLy.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(s => s.MaThe.Contains(filter.SearchTerm) || 
                                        s.MoTaChiTiet.Contains(filter.SearchTerm));

            var anomalies = await query
                .OrderByDescending(s => s.ThoiGianGhiNhan)
                .Select(s => new VehicleAnomalyViewModel
                {
                    MaSuCo = s.MaSuCo,
                    ThoiGianGhiNhan = s.ThoiGianGhiNhan,
                    MaNhanVien = s.MaNhanVien,
                    TenNhanVien = s.MaNhanVienNavigation != null ? s.MaNhanVienNavigation.HoTen : "Chưa xác định",
                    LoaiSuCo = s.LoaiSuCo,
                    MaThe = s.MaThe,
                    MaViTri = s.MaViTri,
                    MoTaChiTiet = s.MoTaChiTiet,
                    TrangThaiXuLy = s.TrangThaiXuLy,
                    TrangThaiXuLyText = s.TrangThaiXuLy == 0 ? "Chưa xử lý" : 
                                       s.TrangThaiXuLy == 1 ? "Đang xử lý" : "Đã xử lý"
                })
                .ToListAsync();

            ViewBag.Filter = filter;
            return View(anomalies);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var anomaly = await _context.SuCos
                .Include(s => s.MaNhanVienNavigation)
                .Where(s => s.MaSuCo == id)
                .Select(s => new VehicleAnomalyViewModel
                {
                    MaSuCo = s.MaSuCo,
                    ThoiGianGhiNhan = s.ThoiGianGhiNhan,
                    MaNhanVien = s.MaNhanVien,
                    TenNhanVien = s.MaNhanVienNavigation != null ? s.MaNhanVienNavigation.HoTen : "Chưa xác định",
                    LoaiSuCo = s.LoaiSuCo,
                    MaThe = s.MaThe,
                    MaViTri = s.MaViTri,
                    MoTaChiTiet = s.MoTaChiTiet,
                    TrangThaiXuLy = s.TrangThaiXuLy,
                    TrangThaiXuLyText = s.TrangThaiXuLy == 0 ? "Chưa xử lý" : 
                                       s.TrangThaiXuLy == 1 ? "Đang xử lý" : "Đã xử lý"
                })
                .FirstOrDefaultAsync();

            if (anomaly == null)
            {
                return NotFound();
            }

            return PartialView("_AnomalyDetailsModal", anomaly);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int status, string note)
        {
            var anomaly = await _context.SuCos.FindAsync(id);
            if (anomaly == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sự cố" });
            }

            anomaly.TrangThaiXuLy = status;
            
            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignStaff(int id, int staffId)
        {
            var anomaly = await _context.SuCos.FindAsync(id);
            if (anomaly == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sự cố" });
            }

            anomaly.MaNhanVien = staffId;
            anomaly.TrangThaiXuLy = 1; // Đang xử lý

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Điều nhân viên thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffList()
        {
            var staff = await _context.NhanViens
                .Where(n => n.TrangThaiLamViec == true)
                .Select(n => new { 
                    id = n.MaNhanVien, 
                    name = n.HoTen,
                    phone = n.SoDienThoai
                })
                .ToListAsync();

            return Json(staff);
        }

        // Tạo dữ liệu mẫu cho sự cố
        [HttpPost]
        public async Task<IActionResult> CreateSampleAnomalies()
        {
            try
            {
                // Kiểm tra xem đã có dữ liệu mẫu chưa
                var existingCount = await _context.SuCos.CountAsync();
                
                // Tạo 3 sự cố mẫu
                var sampleAnomalies = new[]
                {
                    new Models.Entities.SuCo
                    {
                        ThoiGianGhiNhan = DateTime.Now.AddMinutes(-30),
                        LoaiSuCo = "Khẩn cấp",
                        MaThe = "59A1-12345",
                        MaViTri = 15,
                        MoTaChiTiet = "Barrier bị kẹt không mở được, xe đang chờ ở cổng vào. Cần xử lý ngay lập tức để tránh ùn tắc.",
                        TrangThaiXuLy = 0,
                        MaNhanVien = null
                    },
                    new Models.Entities.SuCo
                    {
                        ThoiGianGhiNhan = DateTime.Now.AddMinutes(-15),
                        LoaiSuCo = "Xe mất thẻ",
                        MaThe = "30A-56789",
                        MaViTri = 8,
                        MoTaChiTiet = "Khách hàng báo mất thẻ gửi xe. Xe ô tô màu trắng đã vào lúc 8h sáng nhưng không tìm thấy thẻ khi ra.",
                        TrangThaiXuLy = 0,
                        MaNhanVien = null
                    },
                    new Models.Entities.SuCo
                    {
                        ThoiGianGhiNhan = DateTime.Now.AddMinutes(-5),
                        LoaiSuCo = "Lỗi camera",
                        MaThe = null,
                        MaViTri = 3,
                        MoTaChiTiet = "Camera khu vực A3 bị mất tín hiệu, không nhận diện được biển số xe. Cần kiểm tra kết nối mạng.",
                        TrangThaiXuLy = 0,
                        MaNhanVien = null
                    }
                };

                await _context.SuCos.AddRangeAsync(sampleAnomalies);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Đã tạo 3 sự cố mẫu thành công. Tổng số sự cố hiện tại: {existingCount + 3}" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Lỗi khi tạo dữ liệu mẫu: " + ex.Message 
                });
            }
        }

        // Giao nhiều sự cố cho một nhân viên
        [HttpPost]
        public async Task<IActionResult> AssignMultipleAnomalies(int staffId, int[] anomalyIds)
        {
            try
            {
                if (anomalyIds == null || anomalyIds.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một sự cố" });
                }

                var anomalies = await _context.SuCos
                    .Where(s => anomalyIds.Contains(s.MaSuCo))
                    .ToListAsync();

                if (!anomalies.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy sự cố nào" });
                }

                // Kiểm tra nhân viên có tồn tại không
                var staff = await _context.NhanViens.FindAsync(staffId);
                if (staff == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });
                }

                foreach (var anomaly in anomalies)
                {
                    anomaly.MaNhanVien = staffId;
                    anomaly.TrangThaiXuLy = 1; // Đang xử lý
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Đã giao {anomalies.Count} sự cố cho {staff.HoTen} thành công" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Xóa tất cả sự cố (dùng để reset dữ liệu mẫu)
        [HttpPost]
        public async Task<IActionResult> ClearAllAnomalies()
        {
            try
            {
                var allAnomalies = await _context.SuCos.ToListAsync();
                _context.SuCos.RemoveRange(allAnomalies);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa tất cả sự cố" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
