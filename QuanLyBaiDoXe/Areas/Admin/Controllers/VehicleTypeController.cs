using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleTypeController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleTypeController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vehicleTypes = await _context.LoaiXes
                .Select(l => new VehicleTypeDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe,
                    MoTa = l.MoTa,
                    SoLuongThe = l.TheXes.Count,
                    SoLuongTheHoatDong = l.TheXes.Count(t => t.TrangThai == 1),
                    SoLuongCauHinhGia = l.CauHinhGia.Count
                })
                .ToListAsync();

            var totalCards = await _context.TheXes.CountAsync();
            var activeCards = await _context.TheXes.CountAsync(t => t.TrangThai == 1);

            var model = new VehicleTypeViewModel
            {
                VehicleTypes = vehicleTypes,
                TotalVehicleTypes = vehicleTypes.Count,
                TotalCards = totalCards,
                ActiveCards = activeCards
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleTypes()
        {
            var vehicleTypes = await _context.LoaiXes
                .Select(l => new VehicleTypeDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe,
                    MoTa = l.MoTa,
                    SoLuongThe = l.TheXes.Count,
                    SoLuongTheHoatDong = l.TheXes.Count(t => t.TrangThai == 1),
                    SoLuongCauHinhGia = l.CauHinhGia.Count
                })
                .ToListAsync();

            return Json(new { data = vehicleTypes });
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleType(int id)
        {
            var vehicleType = await _context.LoaiXes
                .Where(l => l.MaLoaiXe == id)
                .Select(l => new VehicleTypeDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe,
                    MoTa = l.MoTa,
                    SoLuongThe = l.TheXes.Count,
                    SoLuongTheHoatDong = l.TheXes.Count(t => t.TrangThai == 1),
                    SoLuongCauHinhGia = l.CauHinhGia.Count
                })
                .FirstOrDefaultAsync();

            if (vehicleType == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy loại xe" });
            }

            return Json(new { success = true, data = vehicleType });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleTypeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenLoaiXe))
                {
                    return Json(new { success = false, message = "Tên loại xe không được để trống!" });
                }

                // Kiểm tra trùng tên
                var exists = await _context.LoaiXes
                    .AnyAsync(l => l.TenLoaiXe == request.TenLoaiXe.Trim());

                if (exists)
                {
                    return Json(new { success = false, message = "Tên loại xe đã tồn tại!" });
                }

                var loaiXe = new LoaiXe
                {
                    TenLoaiXe = request.TenLoaiXe.Trim(),
                    MoTa = request.MoTa?.Trim()
                };

                _context.LoaiXes.Add(loaiXe);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm loại xe thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] VehicleTypeRequest request)
        {
            try
            {
                if (!request.MaLoaiXe.HasValue)
                {
                    return Json(new { success = false, message = "Mã loại xe không hợp lệ!" });
                }

                if (string.IsNullOrWhiteSpace(request.TenLoaiXe))
                {
                    return Json(new { success = false, message = "Tên loại xe không được để trống!" });
                }

                var loaiXe = await _context.LoaiXes.FindAsync(request.MaLoaiXe.Value);
                if (loaiXe == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy loại xe!" });
                }

                // Kiểm tra trùng tên (trừ chính nó)
                var exists = await _context.LoaiXes
                    .AnyAsync(l => l.TenLoaiXe == request.TenLoaiXe.Trim() && l.MaLoaiXe != request.MaLoaiXe.Value);

                if (exists)
                {
                    return Json(new { success = false, message = "Tên loại xe đã tồn tại!" });
                }

                loaiXe.TenLoaiXe = request.TenLoaiXe.Trim();
                loaiXe.MoTa = request.MoTa?.Trim();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật loại xe thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var loaiXe = await _context.LoaiXes
                    .Include(l => l.TheXes)
                    .Include(l => l.CauHinhGia)
                    .FirstOrDefaultAsync(l => l.MaLoaiXe == id);

                if (loaiXe == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy loại xe!" });
                }

                // Kiểm tra có thẻ xe nào đang sử dụng không
                if (loaiXe.TheXes.Any())
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = $"Không thể xóa! Loại xe này đang có {loaiXe.TheXes.Count} thẻ xe liên kết." 
                    });
                }

                // Kiểm tra có cấu hình giá nào không
                if (loaiXe.CauHinhGia.Any())
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = $"Không thể xóa! Loại xe này đang có {loaiXe.CauHinhGia.Count} cấu hình giá liên kết." 
                    });
                }

                _context.LoaiXes.Remove(loaiXe);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa loại xe thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var totalVehicleTypes = await _context.LoaiXes.CountAsync();
            var totalCards = await _context.TheXes.CountAsync();
            var activeCards = await _context.TheXes.CountAsync(t => t.TrangThai == 1);

            return Json(new
            {
                totalVehicleTypes,
                totalCards,
                activeCards
            });
        }
    }
}
