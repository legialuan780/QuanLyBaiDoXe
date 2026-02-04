using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.User.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Services;
using System.Security.Claims;

namespace QuanLyBaiDoXe.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Customer")]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IMoMoService _momoService;
        private readonly QuanLyBaiDoXeContext _context;

        public ReservationController(
            IReservationService reservationService,
            IMoMoService momoService,
            QuanLyBaiDoXeContext context)
        {
            _reservationService = reservationService;
            _momoService = momoService;
            _context = context;
        }

        // GET: User/Reservation/Index
        public async Task<IActionResult> Index()
        {
            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var danhSachDatCho = await _reservationService.GetDatChoByKhachHangAsync(maKhachHang.Value);

            var viewModel = new ReservationListViewModel
            {
                DanhSachDatCho = danhSachDatCho,
                TongDangCho = danhSachDatCho.Count(dc => dc.TrangThaiDatCho == 0),
                TongDaDuyet = danhSachDatCho.Count(dc => dc.TrangThaiDatCho == 1),
                TongDaTuChoi = danhSachDatCho.Count(dc => dc.TrangThaiDatCho == 3),
                TongHetHan = danhSachDatCho.Count(dc => dc.TrangThaiDatCho == 5)
            };

            return View(viewModel);
        }

        // GET: User/Reservation/Create
        public async Task<IActionResult> Create()
        {
            var loaiXes = await _context.LoaiXes
                .Select(lx => new LoaiXeDto
                {
                    MaLoaiXe = lx.MaLoaiXe,
                    TenLoaiXe = lx.TenLoaiXe,
                    MoTa = lx.MoTa,
                    GiaThang = lx.GiaThang
                })
                .ToListAsync();

            ViewBag.LoaiXes = loaiXes;
            return View();
        }

        // POST: Tạo đặt chỗ
        [HttpPost]
        public async Task<IActionResult> TaoDatCho([FromBody] CreateReservationRequest request)
        {
            try
            {
                var maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
                }

                // Validate thời gian
                if (request.ThoiGianDenDuKien <= DateTime.Now)
                {
                    return Json(new { success = false, message = "Thời gian đến phải lớn hơn thời gian hiện tại!" });
                }

                var (success, message, datCho) = await _reservationService.TaoDatChoAsync(
                    maKhachHang.Value,
                    request.MaViTri,
                    request.ThoiGianDenDuKien,
                    request.BienSoXe,
                    request.MaLoaiXe);

                if (!success || datCho == null)
                {
                    return Json(new { success = false, message });
                }

                // Tính tiền cọc - 50% giá tháng chia 30 ngày (tạm tính 1 ngày)
                var loaiXe = await _context.LoaiXes.FindAsync(request.MaLoaiXe);
                var tienCoc = loaiXe?.GiaThang != null ? (loaiXe.GiaThang / 30) * 0.5m : 50000;

                return Json(new
                {
                    success = true,
                    message,
                    maDatCho = datCho.MaDatCho,
                    requirePayment = true,
                    tienCoc = tienCoc
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: Lấy danh sách vị trí trống theo loại xe
        [HttpGet]
        public async Task<IActionResult> GetViTriTrong(int maLoaiXe, DateTime? thoiGianDenDuKien)
        {
            try
            {
                var viTriTrong = await _reservationService.GetViTriTrongTheLoaiXeAsync(maLoaiXe, thoiGianDenDuKien);

                // Load khu vực trước
                var khuVucs = await _context.KhuVucs
                    .Include(kv => kv.MaLoaiXeNavigation)
                    .Include(kv => kv.ViTriDos)
                    .Where(kv => kv.MaLoaiXe == null || kv.MaLoaiXe == maLoaiXe)
                    .ToListAsync();

                // Tính toán trong memory để tránh lỗi LINQ translation
                var khuVucDtos = khuVucs.Select(kv => new KhuVucDto
                {
                    MaKhuVuc = kv.MaKhuVuc,
                    TenKhuVuc = kv.TenKhuVuc,
                    MaLoaiXe = kv.MaLoaiXe,
                    TenLoaiXe = kv.MaLoaiXeNavigation?.TenLoaiXe,
                    SoChoTrong = viTriTrong.Count(vt => vt.MaKhuVuc == kv.MaKhuVuc),
                    TongSoCho = kv.ViTriDos.Count()
                }).ToList();

                return Json(new
                {
                    success = true,
                    viTriTrong,
                    khuVucs = khuVucDtos
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Thanh toán tiền mặt
        [HttpPost]
        public async Task<IActionResult> ThanhToanTienMat([FromBody] ReservationPaymentRequest request)
        {
            try
            {
                var maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
                }

                // Kiểm tra đặt chỗ tồn tại và thuộc về khách hàng
                var datCho = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == request.MaDatCho && dc.MaKhachHang == maKhachHang);

                if (datCho == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt chỗ!" });
                }

                // Cập nhật trạng thái đặt chỗ thành "Đã thanh toán" (1)
                datCho.TrangThaiDatCho = 1;

                // Tạo bản ghi thanh toán trong bảng ThanhToan hoặc ghi chú
                // TODO: Implement payment record if needed

                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = "Đặt chỗ thành công! Vui lòng đến quầy thanh toán tiền cọc trước khi vào bãi." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: Thanh toán MoMo
        [HttpPost]
        public async Task<IActionResult> ThanhToanMoMo([FromBody] ReservationPaymentRequest request)
        {
            try
            {
                var maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
                }

                // Kiểm tra đặt chỗ tồn tại và thuộc về khách hàng
                var datCho = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == request.MaDatCho && dc.MaKhachHang == maKhachHang);

                if (datCho == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt chỗ!" });
                }

                // Tạo orderId theo format: RESERVATION_{MaDatCho}_{timestamp}
                var orderId = $"RESERVATION_{request.MaDatCho}_{DateTime.Now:yyyyMMddHHmmss}";
                var orderInfo = $"Dat coc dat cho #{request.MaDatCho}";

                // Tạo URL callback cho Reservation
                var returnUrl = Url.Action("MoMoReturn", "Reservation", new { area = "User" }, Request.Scheme);
                var notifyUrl = Url.Action("MoMoNotify", "Reservation", new { area = "User" }, Request.Scheme);

                // Gọi MoMo service với custom URLs
                var momoResponse = await _momoService.CreatePaymentAsync(
                    orderId: orderId,
                    amount: (long)request.TienCoc,
                    orderInfo: orderInfo,
                    returnUrl: returnUrl!,
                    notifyUrl: notifyUrl!
                );

                if (momoResponse.Success)
                {
                    // Trả về đầy đủ thông tin như VehicleEntry
                    return Json(new
                    {
                        success = true,
                        payUrl = momoResponse.PayUrl,
                        qrCodeUrl = momoResponse.QrCodeUrl,
                        deepLink = momoResponse.DeepLink,
                        orderId = orderId,
                        amount = request.TienCoc,
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
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: MoMo Return URL - Callback khi thanh toán hoàn thành
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
            try
            {
                // Kiểm tra kết quả thanh toán
                if (resultCode == 0)
                {
                    // Thanh toán thành công
                    // Parse orderId: RESERVATION_{MaDatCho}_{timestamp}
                    var parts = orderId.Split('_');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var maDatCho))
                    {
                        var datCho = await _context.DatChos.FindAsync(maDatCho);
                        if (datCho != null)
                        {
                            datCho.TrangThaiDatCho = 1; // Đã thanh toán
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Format số tiền với dấu phân cách
                    var formattedAmount = amount.ToString("N0");
                    TempData["SuccessMessage"] = $"Thanh toán đặt cọc thành công! Số tiền: {formattedAmount} VNĐ";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = $"Thanh toán thất bại: {message}";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Create");
            }
        }

        // POST: MoMo IPN - Server-to-server notification
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

                // Kiểm tra kết quả
                if (request.ResultCode == 0)
                {
                    // Thanh toán thành công
                    // Parse orderId: RESERVATION_{MaDatCho}_{timestamp}
                    var parts = request.OrderId.Split('_');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var maDatCho))
                    {
                        var datCho = await _context.DatChos.FindAsync(maDatCho);
                        if (datCho != null && datCho.TrangThaiDatCho == 0)
                        {
                            datCho.TrangThaiDatCho = 1; // Đã thanh toán
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: Hủy đặt chỗ
        [HttpPost]
        public async Task<IActionResult> HuyDatCho(int maDatCho)
        {
            try
            {
                var maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
                }

                var (success, message) = await _reservationService.HuyDatChoAsync(maDatCho, maKhachHang.Value);

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper: Lấy mã khách hàng hiện tại
        private async Task<int?> GetCurrentMaKhachHangAsync()
        {
            var maTaiKhoanStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTaiKhoanStr) || !int.TryParse(maTaiKhoanStr, out var maTaiKhoan))
            {
                return null;
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.MaTaiKhoan == maTaiKhoan);

            return khachHang?.MaKhachHang;
        }
    }
}
