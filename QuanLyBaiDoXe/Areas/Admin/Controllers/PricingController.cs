using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class PricingController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public PricingController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pricingConfigs = await _context.CauHinhGia
                .Include(c => c.MaLoaiXeNavigation)
                .Include(c => c.ChiTietGia)
                .Select(c => new PricingConfigDto
                {
                    MaCauHinh = c.MaCauHinh,
                    TenCauHinh = c.TenCauHinh,
                    MaLoaiXe = c.MaLoaiXe,
                    TenLoaiXe = c.MaLoaiXeNavigation != null ? c.MaLoaiXeNavigation.TenLoaiXe : null,
                    GioBatDau = c.GioBatDau != null ? c.GioBatDau.ToString("HH:mm") : null,
                    GioKetThuc = c.GioKetThuc != null ? c.GioKetThuc.ToString("HH:mm") : null,
                    IsUuTien = c.IsUuTien ?? false,
                    SoBlock = c.ChiTietGia.Count,
                    ChiTietGia = c.ChiTietGia.OrderBy(d => d.ThuTuBlock).Select(d => new PricingDetailDto
                    {
                        MaChiTiet = d.MaChiTiet,
                        MaCauHinh = d.MaCauHinh,
                        ThuTuBlock = d.ThuTuBlock,
                        SoPhutCuaBlock = d.SoPhutCuaBlock,
                        GiaTien = d.GiaTien,
                        IsLuyTien = d.IsLuyTien ?? false
                    }).ToList()
                })
                .ToListAsync();

            var vehicleTypes = await _context.LoaiXes
                .Select(l => new VehicleTypeSelectDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe
                })
                .ToListAsync();

            var model = new PricingViewModel
            {
                PricingConfigs = pricingConfigs,
                VehicleTypes = vehicleTypes,
                TotalConfigs = pricingConfigs.Count,
                TotalActiveConfigs = pricingConfigs.Count(c => c.IsUuTien),
                TotalVehicleTypes = vehicleTypes.Count
            };

            return View(model);
        }

        public async Task<IActionResult> TimeBlocks()
        {
            var pricingConfigs = await _context.CauHinhGia
                .Include(c => c.MaLoaiXeNavigation)
                .Include(c => c.ChiTietGia)
                .Select(c => new PricingConfigDto
                {
                    MaCauHinh = c.MaCauHinh,
                    TenCauHinh = c.TenCauHinh,
                    MaLoaiXe = c.MaLoaiXe,
                    TenLoaiXe = c.MaLoaiXeNavigation != null ? c.MaLoaiXeNavigation.TenLoaiXe : null,
                    GioBatDau = c.GioBatDau != null ? c.GioBatDau.ToString("HH:mm") : null,
                    GioKetThuc = c.GioKetThuc != null ? c.GioKetThuc.ToString("HH:mm") : null,
                    IsUuTien = c.IsUuTien ?? false,
                    SoBlock = c.ChiTietGia.Count,
                    ChiTietGia = c.ChiTietGia.OrderBy(d => d.ThuTuBlock).Select(d => new PricingDetailDto
                    {
                        MaChiTiet = d.MaChiTiet,
                        MaCauHinh = d.MaCauHinh,
                        ThuTuBlock = d.ThuTuBlock,
                        SoPhutCuaBlock = d.SoPhutCuaBlock,
                        GiaTien = d.GiaTien,
                        IsLuyTien = d.IsLuyTien ?? false
                    }).ToList()
                })
                .ToListAsync();

            var vehicleTypes = await _context.LoaiXes
                .Select(l => new VehicleTypeSelectDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe
                })
                .ToListAsync();

            var model = new PricingViewModel
            {
                PricingConfigs = pricingConfigs,
                VehicleTypes = vehicleTypes,
                TotalConfigs = pricingConfigs.Count,
                TotalActiveConfigs = pricingConfigs.Count(c => c.IsUuTien),
                TotalVehicleTypes = vehicleTypes.Count
            };

            return View("TimeBlocks", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPricingConfigs()
        {
            var pricingConfigs = await _context.CauHinhGia
                .Include(c => c.MaLoaiXeNavigation)
                .Include(c => c.ChiTietGia)
                .Select(c => new PricingConfigDto
                {
                    MaCauHinh = c.MaCauHinh,
                    TenCauHinh = c.TenCauHinh,
                    MaLoaiXe = c.MaLoaiXe,
                    TenLoaiXe = c.MaLoaiXeNavigation != null ? c.MaLoaiXeNavigation.TenLoaiXe : null,
                    GioBatDau = c.GioBatDau != null ? c.GioBatDau.ToString("HH:mm") : null,
                    GioKetThuc = c.GioKetThuc != null ? c.GioKetThuc.ToString("HH:mm") : null,
                    IsUuTien = c.IsUuTien ?? false,
                    SoBlock = c.ChiTietGia.Count,
                    ChiTietGia = c.ChiTietGia.OrderBy(d => d.ThuTuBlock).Select(d => new PricingDetailDto
                    {
                        MaChiTiet = d.MaChiTiet,
                        MaCauHinh = d.MaCauHinh,
                        ThuTuBlock = d.ThuTuBlock,
                        SoPhutCuaBlock = d.SoPhutCuaBlock,
                        GiaTien = d.GiaTien,
                        IsLuyTien = d.IsLuyTien ?? false
                    }).ToList()
                })
                .ToListAsync();

            return Json(new { data = pricingConfigs });
        }

        [HttpGet]
        public async Task<IActionResult> GetPricingConfig(int id)
        {
            var config = await _context.CauHinhGia
                .Include(c => c.MaLoaiXeNavigation)
                .Include(c => c.ChiTietGia)
                .Where(c => c.MaCauHinh == id)
                .Select(c => new PricingConfigDto
                {
                    MaCauHinh = c.MaCauHinh,
                    TenCauHinh = c.TenCauHinh,
                    MaLoaiXe = c.MaLoaiXe,
                    TenLoaiXe = c.MaLoaiXeNavigation != null ? c.MaLoaiXeNavigation.TenLoaiXe : null,
                    GioBatDau = c.GioBatDau != null ? c.GioBatDau.ToString("HH:mm") : null,
                    GioKetThuc = c.GioKetThuc != null ? c.GioKetThuc.ToString("HH:mm") : null,
                    IsUuTien = c.IsUuTien ?? false,
                    SoBlock = c.ChiTietGia.Count,
                    ChiTietGia = c.ChiTietGia.OrderBy(d => d.ThuTuBlock).Select(d => new PricingDetailDto
                    {
                        MaChiTiet = d.MaChiTiet,
                        MaCauHinh = d.MaCauHinh,
                        ThuTuBlock = d.ThuTuBlock,
                        SoPhutCuaBlock = d.SoPhutCuaBlock,
                        GiaTien = d.GiaTien,
                        IsLuyTien = d.IsLuyTien ?? false
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (config == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy cấu hình giá" });
            }

            return Json(new { success = true, data = config });
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleTypes()
        {
            var vehicleTypes = await _context.LoaiXes
                .Select(l => new VehicleTypeSelectDto
                {
                    MaLoaiXe = l.MaLoaiXe,
                    TenLoaiXe = l.TenLoaiXe
                })
                .ToListAsync();

            return Json(new { success = true, data = vehicleTypes });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PricingConfigRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenCauHinh))
                {
                    return Json(new { success = false, message = "Tên cấu hình không được để trống!" });
                }

                if (!request.MaLoaiXe.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng chọn loại xe!" });
                }

                var config = new CauHinhGium
                {
                    TenCauHinh = request.TenCauHinh,
                    MaLoaiXe = request.MaLoaiXe.Value, // <-- Fix: use .Value to convert int? to int
                    GioBatDau = !string.IsNullOrEmpty(request.GioBatDau) ? TimeOnly.Parse(request.GioBatDau) : default,
                    GioKetThuc = !string.IsNullOrEmpty(request.GioKetThuc) ? TimeOnly.Parse(request.GioKetThuc) : default,
                    IsUuTien = request.IsUuTien
                };

                _context.CauHinhGia.Add(config);
                await _context.SaveChangesAsync();

                // Thêm chi tiết giá
                if (request.ChiTietGia != null && request.ChiTietGia.Any())
                {
                    foreach (var detail in request.ChiTietGia)
                    {
                        var chiTiet = new ChiTietGium
                        {
                            MaCauHinh = config.MaCauHinh,
                            ThuTuBlock = detail.ThuTuBlock,
                            SoPhutCuaBlock = detail.SoPhutCuaBlock,
                            GiaTien = detail.GiaTien,
                            IsLuyTien = detail.IsLuyTien
                        };
                        _context.ChiTietGia.Add(chiTiet);
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Thêm cấu hình giá thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PricingConfigRequest request)
        {
            try
            {
                if (!request.MaCauHinh.HasValue)
                {
                    return Json(new { success = false, message = "Mã cấu hình không hợp lệ!" });
                }

                var config = await _context.CauHinhGia
                    .Include(c => c.ChiTietGia)
                    .FirstOrDefaultAsync(c => c.MaCauHinh == request.MaCauHinh.Value);

                if (config == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cấu hình giá" });
                }

                config.TenCauHinh = request.TenCauHinh;
                config.MaLoaiXe = request.MaLoaiXe.Value;
                config.GioBatDau = !string.IsNullOrEmpty(request.GioBatDau) ? TimeOnly.Parse(request.GioBatDau) : default;
                config.GioKetThuc = !string.IsNullOrEmpty(request.GioKetThuc) ? TimeOnly.Parse(request.GioKetThuc) : default;
                config.IsUuTien = request.IsUuTien;

                // Xóa chi tiết cũ
                _context.ChiTietGia.RemoveRange(config.ChiTietGia);

                // Thêm chi tiết mới
                if (request.ChiTietGia != null && request.ChiTietGia.Any())
                {
                    foreach (var detail in request.ChiTietGia)
                    {
                        var chiTiet = new ChiTietGium
                        {
                            MaCauHinh = config.MaCauHinh,
                            ThuTuBlock = detail.ThuTuBlock,
                            SoPhutCuaBlock = detail.SoPhutCuaBlock,
                            GiaTien = detail.GiaTien,
                            IsLuyTien = detail.IsLuyTien
                        };
                        _context.ChiTietGia.Add(chiTiet);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật cấu hình giá thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var config = await _context.CauHinhGia
                    .Include(c => c.ChiTietGia)
                    .FirstOrDefaultAsync(c => c.MaCauHinh == id);

                if (config == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cấu hình giá" });
                }

                // Xóa chi tiết trước
                _context.ChiTietGia.RemoveRange(config.ChiTietGia);
                _context.CauHinhGia.Remove(config);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa cấu hình giá thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TogglePriority(int id)
        {
            try
            {
                var config = await _context.CauHinhGia.FindAsync(id);

                if (config == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cấu hình giá" });
                }

                config.IsUuTien = !(config.IsUuTien ?? false);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = config.IsUuTien == true ? "Đã bật ưu tiên!" : "Đã tắt ưu tiên!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
