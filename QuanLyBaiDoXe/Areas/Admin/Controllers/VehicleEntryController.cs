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
        private readonly ILicensePlateRecognitionService _plateRecognitionService;
        private readonly IMoMoService _momoService;

        public VehicleEntryController(
            IVehicleEntryService vehicleEntryService,
            QuanLyBaiDoXeContext context,
            IWebHostEnvironment webHostEnvironment,
            ILicensePlateRecognitionService plateRecognitionService,
            IMoMoService momoService)
        {
            _vehicleEntryService = vehicleEntryService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _plateRecognitionService = plateRecognitionService;
            _momoService = momoService;
            }

                public async Task<IActionResult> Index()
            {
                // Lấy danh sách vị trí trống
                var viTriTrong = await _vehicleEntryService.GetAvailableViTriDoAsync();

                // Nhóm theo khu vực và đếm số chỗ trống (bao gồm thông tin loại xe) - hiển thị TẤT CẢ khu vực
                var khuVucList = await _context.KhuVucs
                    .Include(kv => kv.MaLoaiXeNavigation)
                    .Select(kv => new KhuVucChoTrongDto
                    {
                        MaKhuVuc = kv.MaKhuVuc,
                        TenKhuVuc = kv.TenKhuVuc,
                        MaLoaiXe = kv.MaLoaiXe,
                        TenLoaiXe = kv.MaLoaiXeNavigation != null ? kv.MaLoaiXeNavigation.TenLoaiXe : null,
                        SoChoTrong = kv.ViTriDos.Count(v => v.TrangThai == 0),
                        TongSoCho = kv.ViTriDos.Count(),
                        ViTriTrong = kv.ViTriDos
                            .Where(v => v.TrangThai == 0)
                            .Select(v => new ViTriDoDto
                            {
                                MaViTri = v.MaViTri,
                                TenViTri = v.TenViTri ?? ""
                            })
                            .ToList()
                    })
                    // Không lọc - hiển thị tất cả khu vực, kể cả đầy
                    .OrderBy(kv => kv.TenKhuVuc)
                    .ToListAsync();

                var viewModel = new VehicleEntryViewModel
                {
                    XeDangTrongBai = await _vehicleEntryService.GetXeDangTrongBaiAsync(),
                    ViTriTrong = viTriTrong,
                    KhuVucList = khuVucList,
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
                                request.MaKhuVuc); // Truyền mã khu vực thay vì mã vị trí

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
                                // Log chi tiết lỗi để debug
                                var errorMessage = ex.Message;
                                if (ex.InnerException != null)
                                {
                                    errorMessage = ex.InnerException.Message;
                        }
                
                        return Json(new QuetTheResponse
                        {
                            Success = false,
                            Message = errorMessage
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

            // Kiểm tra vé tháng chi tiết
            var veThangInfo = await _vehicleEntryService.KiemTraVeThangChiTietAsync(maThe);

            return Json(new
            {
                success = true,
                theXe = MapToTheXeDto(theXe, luotGuiHienTai != null),
                luotGui = luotGuiHienTai != null ? MapToLuotGuiDto(luotGuiHienTai) : null,
                action = luotGuiHienTai != null ? "RA" : "VAO",
                veThangInfo = veThangInfo.CoVeThang ? new
                {
                    coVeThang = veThangInfo.CoVeThang,
                    hopLe = veThangInfo.HopLe,
                    daHetHan = veThangInfo.DaHetHan,
                    sapHetHan = veThangInfo.SapHetHan,
                    soNgayConLai = veThangInfo.SoNgayConLai,
                    khongCoKhachHang = veThangInfo.KhongCoKhachHang,
                    bienSoMacDinh = veThangInfo.BienSoMacDinh,
                    tenKhachHang = veThangInfo.TenKhachHang,
                    ngayHetHan = veThangInfo.NgayHetHan?.ToString("dd/MM/yyyy"),
                    thongBao = veThangInfo.ThongBao
                } : null
            });
        }

        /// <summary>
        /// Lấy danh sách khu vực theo loại xe (bao gồm cả khu vực đầy)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetKhuVucTheoLoaiXe(int? maLoaiXe)
        {
            var khuVucList = await _context.KhuVucs
                .Include(kv => kv.MaLoaiXeNavigation)
                .Include(kv => kv.ViTriDos)
                // Không lọc theo số chỗ trống - hiển thị tất cả khu vực
                .Where(kv => maLoaiXe == null || kv.MaLoaiXe == null || kv.MaLoaiXe == maLoaiXe)
                .Select(kv => new
                {
                    maKhuVuc = kv.MaKhuVuc,
                    tenKhuVuc = kv.TenKhuVuc,
                    maLoaiXe = kv.MaLoaiXe,
                    tenLoaiXe = kv.MaLoaiXeNavigation != null ? kv.MaLoaiXeNavigation.TenLoaiXe : null,
                    soChoTrong = kv.ViTriDos.Count(v => v.TrangThai == 0),
                    tongSoCho = kv.ViTriDos.Count()
                })
                .OrderBy(kv => kv.tenKhuVuc)
                .ToListAsync();

            return Json(khuVucList);
        }

        /// <summary>
        /// Kiểm tra biển số xe đang trong bãi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> KiemTraBienSoDangTrongBai(string bienSo)
        {
            if (string.IsNullOrEmpty(bienSo))
            {
                return Json(new { dangTrongBai = false });
            }

            var dangTrongBai = await _vehicleEntryService.KiemTraBienSoDangTrongBaiAsync(bienSo);
            
            return Json(new { dangTrongBai = dangTrongBai });
        }

        [HttpGet]
        public async Task<IActionResult> GetXeDangTrongBai()
        {
            var xeDangTrongBai = await _vehicleEntryService.GetXeDangTrongBaiAsync();
            var result = xeDangTrongBai.Select(MapToLuotGuiDto).ToList();
            return Json(result);
        }

        /// <summary>
        /// Preview tính tiền xe ra - KHÔNG lưu database
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PreviewXeRa([FromBody] QuetTheRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MaThe))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã thẻ!" });
                }


                var luotGui = await _vehicleEntryService.GetLuotGuiDangGuiByMaTheAsync(request.MaThe);
                if (luotGui == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lượt gửi của thẻ này!" });
                }

                // Kiểm tra biển số
                if (!string.IsNullOrEmpty(request.BienSo) && !string.IsNullOrEmpty(luotGui.BienSoVao))
                {
                    var bienSoVao = luotGui.BienSoVao.Trim().ToUpper();
                    var bienSoRa = request.BienSo.Trim().ToUpper();
                    
                    if (bienSoVao != bienSoRa)
                    {
                        return Json(new { 
                            success = false, 
                            message = $"Biển số xe ra ({request.BienSo}) không khớp với biển số xe vào ({luotGui.BienSoVao})!" 
                        });
                    }
                }

                // Kiểm tra vé tháng chi tiết với biển số
                var veThangInfo = await _vehicleEntryService.KiemTraVeThangChiTietAsync(request.MaThe, request.BienSo ?? luotGui.BienSoVao);

                // Tính tiền preview (không lưu)
                var thoiGianRa = DateTime.Now;
                var tongTien = await _vehicleEntryService.TinhTienGuiXePreviewAsync(luotGui, thoiGianRa);

                // Tính thời gian gửi
                var thoiGianGui = thoiGianRa - luotGui.ThoiGianVao;
                var soGio = (int)thoiGianGui.TotalHours;
                var soPhut = thoiGianGui.Minutes;

                // Thông báo thêm về vé tháng
                string? thongBaoVeThang = null;
                if (veThangInfo.CoVeThang)
                {
                    if (veThangInfo.HopLe)
                    {
                        thongBaoVeThang = "VÉ THÁNG HỢP LỆ - Miễn phí gửi xe";
                    }
                    else if (veThangInfo.DaHetHan)
                    {
                        thongBaoVeThang = $"Vé tháng đã hết hạn từ {veThangInfo.NgayHetHan:dd/MM/yyyy}";
                    }
                    else if (veThangInfo.BienSoKhongKhop)
                    {
                        thongBaoVeThang = $"Biển số không khớp với vé tháng ({veThangInfo.BienSoMacDinh})";
                    }
                    else if (veThangInfo.KhongCoKhachHang)
                    {
                        thongBaoVeThang = "Vé tháng chưa liên kết khách hàng";
                    }
                }

                return Json(new
                {
                    success = true,
                    maThe = luotGui.MaThe,
                    bienSo = request.BienSo ?? luotGui.BienSoVao,
                    tenLoaiXe = luotGui.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe ?? "Chưa phân loại",
                    thoiGianVao = luotGui.ThoiGianVao,
                    thoiGianRa = thoiGianRa,
                    thoiGianGui = $"{soGio} giờ {soPhut} phút",
                    tongTien = tongTien,
                    veThangHopLe = veThangInfo.HopLe,
                    thongBaoVeThang = thongBaoVeThang
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xác nhận xe ra sau khi thanh toán
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConfirmXeRa([FromBody] ConfirmXeRaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MaThe))
                {
                    return Json(new { success = false, message = "Mã thẻ không hợp lệ!" });
                }

                var luotGui = await _vehicleEntryService.GetLuotGuiDangGuiByMaTheAsync(request.MaThe);
                if (luotGui == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lượt gửi của thẻ này!" });
                }

                // Lưu hình ảnh nếu có
                string? savedImagePath = null;
                if (!string.IsNullOrEmpty(request.HinhAnh))
                {
                    savedImagePath = await SaveImage(request.HinhAnh, "ra");
                }

                // Xử lý xe ra thực sự
                var luotGuiRa = await _vehicleEntryService.XuLyXeRaAsync(
                    request.MaThe,
                    request.BienSo ?? luotGui.BienSoVao ?? "",
                    savedImagePath);

                return Json(new
                {
                    success = true,
                    message = $"Xe ra thành công! Tổng tiền: {luotGuiRa?.TongTien:N0} VNĐ",
                    phuongThucThanhToan = request.PhuongThucThanhToan,
                    tongTien = luotGuiRa?.TongTien
                });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
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

        /// <summary>
        /// API nhận dạng biển số xe từ ảnh
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RecognizePlate([FromBody] RecognizePlateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageBase64))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ảnh để nhận dạng biển số"
                    });
                }

                var result = await _plateRecognitionService.RecognizePlateAsync(request.ImageBase64);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        plateNumber = result.PlateNumber,
                        rawPlate = result.RawPlateNumber,
                        confidence = Math.Round(result.Confidence, 1),
                        vehicleType = result.VehicleType,
                        message = $"Nhận dạng thành công: {result.PlateNumber} (độ tin cậy: {result.Confidence:F1}%)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.ErrorMessage ?? "Không thể nhận dạng biển số từ ảnh"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi nhận dạng: {ex.Message}"
                });
            }
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
                        MaLoaiXe = theXe.MaLoaiXe,
                        TenLoaiXe = theXe.MaLoaiXeNavigation?.TenLoaiXe,
                        LoaiThe = theXe.LoaiThe,
                        TrangThai = theXe.TrangThai,
                        DangGui = dangGui
                    };
                }

                /// <summary>
                /// Tạo thanh toán MoMo
                /// </summary>
                [HttpPost]
                public async Task<IActionResult> CreateMoMoPayment([FromBody] MoMoPaymentRequest request)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(request.MaThe))
                        {
                            return Json(new { success = false, message = "Mã thẻ không hợp lệ!" });
                        }

                        var luotGui = await _vehicleEntryService.GetLuotGuiDangGuiByMaTheAsync(request.MaThe);
                        if (luotGui == null)
                        {
                            return Json(new { success = false, message = "Không tìm thấy lượt gửi của thẻ này!" });
                        }

                        // Tính tiền
                        var thoiGianRa = DateTime.Now;
                        var tongTien = await _vehicleEntryService.TinhTienGuiXePreviewAsync(luotGui, thoiGianRa);

                        if (tongTien <= 0)
                        {
                            return Json(new { success = false, message = "Số tiền thanh toán phải lớn hơn 0!" });
                        }

                        // Tạo mã đơn hàng unique
                        var orderId = $"PARKING_{luotGui.MaLuotGui}_{DateTime.Now:yyyyMMddHHmmss}";
                        var orderInfo = $"Thanh toán phí gửi xe - Mã thẻ: {request.MaThe} - Biển số: {luotGui.BienSoVao}";

                        // Gọi MoMo API
                        var momoResponse = await _momoService.CreatePaymentAsync(orderId, (long)tongTien, orderInfo);

                        if (momoResponse.Success)
                        {
                            // Lưu thông tin thanh toán pending (có thể lưu vào database nếu cần)
                            return Json(new
                            {
                                success = true,
                                payUrl = momoResponse.PayUrl,
                                qrCodeUrl = momoResponse.QrCodeUrl,
                                deepLink = momoResponse.DeepLink,
                                orderId = orderId,
                                amount = tongTien,
                                message = "Tạo thanh toán MoMo thành công!"
                            });
                        }

                        return Json(new
                        {
                            success = false,
                            message = momoResponse.Message ?? "Không thể tạo thanh toán MoMo"
                        });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = ex.Message });
                    }
                }

                /// <summary>
                /// Callback URL khi thanh toán MoMo hoàn thành (redirect từ MoMo)
                /// </summary>
                [HttpGet]
                public async Task<IActionResult> MoMoReturn(
                    string partnerCode,
                    string orderId,
                    string requestId,
                    long amount,
                    string orderInfo,
                    string orderType,
                    long transId,
                    int resultCode,
                    string message,
                    string payType,
                    long responseTime,
                    string extraData,
                    string signature)
                {
                    // Kiểm tra kết quả thanh toán
                    if (resultCode == 0)
                    {
                        // Thanh toán thành công
                        // Parse orderId để lấy MaLuotGui: PARKING_{MaLuotGui}_{timestamp}
                        var parts = orderId.Split('_');
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var maLuotGui))
                        {
                            var luotGui = await _context.LuotGuis
                                .Include(l => l.MaTheNavigation)
                                .FirstOrDefaultAsync(l => l.MaLuotGui == maLuotGui && l.TrangThai == 0);

                            if (luotGui != null)
                            {
                                // Xử lý xe ra
                                await _vehicleEntryService.XuLyXeRaAsync(
                                    luotGui.MaThe!,
                                    luotGui.BienSoVao ?? "",
                                    null);
                            }
                        }

                        // Format số tiền với dấu phân cách
                        var formattedAmount = amount.ToString("N0");
                        return RedirectToAction("Index", new { momoResult = "success", amount = formattedAmount, transactionId = transId });
                    }
                    else
                    {
                        return RedirectToAction("Index", new { momoResult = "failed", errorMessage = message });
                    }
                }

                /// <summary>
                /// IPN URL - MoMo gọi để thông báo kết quả thanh toán (server-to-server)
                /// </summary>
                [HttpPost]
                public async Task<IActionResult> MoMoNotify([FromBody] MoMoCallbackRequest request)
                {
                    try
                    {
                        // Xác minh chữ ký
                        if (!_momoService.VerifySignature(request))
                        {
                            return BadRequest(new { message = "Invalid signature" });
                        }

                        if (request.ResultCode == 0)
                        {
                            // Thanh toán thành công
                            var parts = request.OrderId.Split('_');
                            if (parts.Length >= 2 && long.TryParse(parts[1], out var maLuotGui))
                            {
                                var luotGui = await _context.LuotGuis
                                    .Include(l => l.MaTheNavigation)
                                    .FirstOrDefaultAsync(l => l.MaLuotGui == maLuotGui && l.TrangThai == 0);

                                if (luotGui != null)
                                {
                                    // Xử lý xe ra
                                    await _vehicleEntryService.XuLyXeRaAsync(
                                        luotGui.MaThe!,
                                        luotGui.BienSoVao ?? "",
                                        null);
                                }
                            }
                        }

                        return Ok(new { message = "Received" });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { message = ex.Message });
                    }
                }

                /// <summary>
                /// Kiểm tra trạng thái thanh toán MoMo
                /// </summary>
                [HttpGet]
                public async Task<IActionResult> CheckMoMoPaymentStatus(string orderId)
                {
                    try
                    {
                        // Parse orderId để lấy MaLuotGui
                        var parts = orderId.Split('_');
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var maLuotGui))
                        {
                            var luotGui = await _context.LuotGuis.FindAsync(maLuotGui);
                            if (luotGui != null)
                            {
                                // TrangThai = 1 nghĩa là đã thanh toán và xe đã ra
                                return Json(new
                                {
                                    success = true,
                                    paid = luotGui.TrangThai == 1,
                                    message = luotGui.TrangThai == 1 ? "Đã thanh toán" : "Chưa thanh toán"
                                });
                            }
                        }

                        return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = ex.Message });
                    }
                }
            }
        }
