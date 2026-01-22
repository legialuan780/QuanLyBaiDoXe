using Microsoft.AspNetCore.Mvc;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.Services;
using Microsoft.EntityFrameworkCore;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleEntryController : Controller
    {
        private readonly IVehicleEntryService _vehicleEntryService;
        private readonly QuanLyBaiDoXeContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VehicleEntryController(
            IVehicleEntryService vehicleEntryService,
            QuanLyBaiDoXeContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _vehicleEntryService = vehicleEntryService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new VehicleEntryViewModel
            {
                XeDangTrongBai = await _vehicleEntryService.GetXeDangTrongBaiAsync(),
                ViTriTrong = await _vehicleEntryService.GetAvailableViTriDoAsync(),
                LoaiXeList = await _vehicleEntryService.GetLoaiXeListAsync(),
                TongXeDangGui = await _context.LuotGuis.CountAsync(l => l.TrangThai == 0),
                TongViTriTrong = await _context.ViTriDos.CountAsync(v => v.TrangThai == 0),
                TongViTri = await _context.ViTriDos.CountAsync(),
                TongThuHomNay = await _context.LuotGuis
                    .Where(l => l.ThoiGianRa != null && l.ThoiGianRa.Value.Date == DateTime.Today)
                    .SumAsync(l => l.TongTien ?? 0)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> QuetThe([FromBody] QuetTheRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MaThe))
                {
                    return Json(new QuetTheResponse
                    {
                        Success = false,
                        Message = "Vui lòng nhập mã thẻ!"
                    });
                }

                // Kiểm tra thẻ tồn tại
                var theXe = await _vehicleEntryService.GetTheXeByMaTheAsync(request.MaThe);
                if (theXe == null)
                {
                    return Json(new QuetTheResponse
                    {
                        Success = false,
                        Message = "Mã thẻ không tồn tại trong hệ thống!"
                    });
                }

                if (theXe.TrangThai != 1)
                {
                    return Json(new QuetTheResponse
                    {
                        Success = false,
                        Message = "Thẻ xe đã bị khóa hoặc không hoạt động!"
                    });
                }

                // Kiểm tra xem thẻ đang gửi hay không
                var luotGuiHienTai = await _vehicleEntryService.GetLuotGuiDangGuiByMaTheAsync(request.MaThe);

                if (luotGuiHienTai != null)
                {
                    // Xe đang gửi -> Xử lý xe ra
                    if (string.IsNullOrEmpty(request.BienSo))
                    {
                        return Json(new QuetTheResponse
                        {
                            Success = false,
                            Message = "Vui lòng nhập biển số xe ra!"
                        });
                    }

                    // Kiểm tra biển số xe ra phải giống biển số xe vào
                    if (!string.IsNullOrEmpty(luotGuiHienTai.BienSoVao))
                    {
                        var bienSoVao = luotGuiHienTai.BienSoVao.Trim().ToUpper();
                        var bienSoRa = request.BienSo.Trim().ToUpper();
                        
                        if (bienSoVao != bienSoRa)
                        {
                            return Json(new QuetTheResponse
                            {
                                Success = false,
                                Message = $"Biển số xe ra ({request.BienSo}) không khớp với biển số xe vào ({luotGuiHienTai.BienSoVao})! Vui lòng kiểm tra lại."
                            });
                        }
                    }

                    // Lưu hình ảnh nếu có
                    string? savedImagePath = null;
                    if (!string.IsNullOrEmpty(request.HinhAnh))
                    {
                        savedImagePath = await SaveImage(request.HinhAnh, "ra");
                    }

                    var luotGuiRa = await _vehicleEntryService.XuLyXeRaAsync(
                        request.MaThe,
                        request.BienSo,
                        savedImagePath);

                    return Json(new QuetTheResponse
                    {
                        Success = true,
                        Message = $"Xe ra thành công! Tổng tiền: {luotGuiRa?.TongTien:N0} VNĐ",
                        Action = "RA",
                        LuotGui = MapToLuotGuiDto(luotGuiRa),
                        TheXe = MapToTheXeDto(theXe, false)
                    });
                }
                else
                {
                    // Xe chưa gửi -> Xử lý xe vào
                    if (string.IsNullOrEmpty(request.BienSo))
                    {
                        return Json(new QuetTheResponse
                        {
                            Success = false,
                            Message = "Vui lòng nhập biển số xe vào!"
                        });
                    }

                    // Lưu hình ảnh nếu có
                    string? savedImagePath = null;
                    if (!string.IsNullOrEmpty(request.HinhAnh))
                    {
                        savedImagePath = await SaveImage(request.HinhAnh, "vao");
                    }

                    var luotGuiVao = await _vehicleEntryService.XuLyXeVaoAsync(
                        request.MaThe,
                        request.BienSo,
                        savedImagePath,
                        request.MaViTri);

                    return Json(new QuetTheResponse
                    {
                        Success = true,
                        Message = "Xe vào thành công!",
                        Action = "VAO",
                        LuotGui = MapToLuotGuiDto(luotGuiVao),
                        TheXe = MapToTheXeDto(theXe, true)
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new QuetTheResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> KiemTraThe(string maThe)
        {
            if (string.IsNullOrEmpty(maThe))
            {
                return Json(new { success = false, message = "Mã thẻ không hợp lệ!" });
            }

            var theXe = await _vehicleEntryService.GetTheXeByMaTheAsync(maThe);
            if (theXe == null)
            {
                return Json(new { success = false, message = "Mã thẻ không tồn tại!" });
            }

            var luotGuiHienTai = await _vehicleEntryService.GetLuotGuiDangGuiByMaTheAsync(maThe);

            return Json(new
            {
                success = true,
                theXe = MapToTheXeDto(theXe, luotGuiHienTai != null),
                luotGui = luotGuiHienTai != null ? MapToLuotGuiDto(luotGuiHienTai) : null,
                action = luotGuiHienTai != null ? "RA" : "VAO"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetXeDangTrongBai()
        {
            var xeDangTrongBai = await _vehicleEntryService.GetXeDangTrongBaiAsync();
            var result = xeDangTrongBai.Select(MapToLuotGuiDto).ToList();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetThongKe()
        {
            var thongKe = new
            {
                tongXeDangGui = await _context.LuotGuis.CountAsync(l => l.TrangThai == 0),
                tongViTriTrong = await _context.ViTriDos.CountAsync(v => v.TrangThai == 0),
                tongViTri = await _context.ViTriDos.CountAsync(),
                tongThuHomNay = await _context.LuotGuis
                    .Where(l => l.ThoiGianRa != null && l.ThoiGianRa.Value.Date == DateTime.Today)
                    .SumAsync(l => l.TongTien ?? 0)
            };

            return Json(thongKe);
        }

        private async Task<string?> SaveImage(string base64Image, string prefix)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image))
                    return null;

                // Xóa header của base64 nếu có
                var base64Data = base64Image;
                if (base64Image.Contains(","))
                {
                    base64Data = base64Image.Split(',')[1];
                }

                var imageBytes = Convert.FromBase64String(base64Data);
                var fileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.jpg";
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "vehicles");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                return $"/uploads/vehicles/{fileName}";
            }
            catch
            {
                return null;
            }
        }

        private static LuotGuiDto? MapToLuotGuiDto(LuotGui? luotGui)
        {
            if (luotGui == null) return null;

            return new LuotGuiDto
            {
                MaLuotGui = luotGui.MaLuotGui,
                MaThe = luotGui.MaThe,
                BienSoVao = luotGui.BienSoVao,
                BienSoRa = luotGui.BienSoRa,
                HinhAnhVao = luotGui.HinhAnhVao,
                HinhAnhRa = luotGui.HinhAnhRa,
                ThoiGianVao = luotGui.ThoiGianVao,
                ThoiGianRa = luotGui.ThoiGianRa,
                TenViTri = luotGui.MaViTriNavigation?.TenViTri,
                TenKhuVuc = luotGui.MaViTriNavigation?.MaKhuVucNavigation?.TenKhuVuc,
                TenLoaiXe = luotGui.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe,
                TongTien = luotGui.TongTien,
                TrangThai = luotGui.TrangThai
            };
        }

        private static TheXeDto MapToTheXeDto(TheXe theXe, bool dangGui)
        {
            return new TheXeDto
            {
                MaThe = theXe.MaThe,
                TenLoaiXe = theXe.MaLoaiXeNavigation?.TenLoaiXe,
                LoaiThe = theXe.LoaiThe,
                TrangThai = theXe.TrangThai,
                DangGui = dangGui
            };
        }
    }
}
