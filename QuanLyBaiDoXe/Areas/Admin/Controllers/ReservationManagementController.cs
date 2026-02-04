using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.User.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Services;
using System.Security.Claims;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class ReservationManagementController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly QuanLyBaiDoXeContext _context;

        public ReservationManagementController(
            IReservationService reservationService,
            QuanLyBaiDoXeContext context)
        {
            _reservationService = reservationService;
            _context = context;
        }

        // GET: Admin/ReservationManagement/Index
        public async Task<IActionResult> Index()
        {
            var danhSachDatCho = await _reservationService.GetAllDatChoAsync();

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

        // GET: Admin/ReservationManagement/ChoDuyet
        public async Task<IActionResult> ChoDuyet()
        {
            var danhSachChoDuyet = await _reservationService.GetDatChoChoDuyetAsync();

            var viewModel = new ReservationListViewModel
            {
                DanhSachDatCho = danhSachChoDuyet,
                TongDangCho = danhSachChoDuyet.Count
            };

            return View(viewModel);
        }

        // POST: Duyệt đặt chỗ
        [HttpPost]
        public async Task<IActionResult> DuyetDatCho(int maDatCho)
        {
            try
            {
                var maNhanVien = await GetCurrentMaNhanVienAsync();
                if (maNhanVien == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên!" });
                }

                var (success, message) = await _reservationService.DuyetDatChoAsync(maDatCho, maNhanVien.Value);

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Từ chối đặt chỗ
        [HttpPost]
        public async Task<IActionResult> TuChoiDatCho(int maDatCho, string lyDo)
        {
            try
            {
                var (success, message) = await _reservationService.TuChoiDatChoAsync(maDatCho, lyDo);

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Setup vị trí đặt trước cho ngày mai
        [HttpPost]
        public async Task<IActionResult> SetupViTriDatTruoc(int maDatCho)
        {
            try
            {
                var (success, message) = await _reservationService.SetupViTriDatTruocAsync(maDatCho);

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Lấy chi tiết đặt chỗ
        [HttpGet]
        public async Task<IActionResult> GetChiTietDatCho(int maDatCho)
        {
            try
            {
                var datCho = await _reservationService.GetDatChoByIdAsync(maDatCho);
                if (datCho == null)
                {
                    return Json(new { success = false, message = "Đặt chỗ không tồn tại!" });
                }

                var result = new
                {
                    success = true,
                    maDatCho = datCho.MaDatCho,
                    maKhachHang = datCho.MaKhachHang,
                    tenKhachHang = datCho.MaKhachHangNavigation?.HoTen,
                    soDienThoai = datCho.MaKhachHangNavigation?.SoDienThoai,
                    maViTri = datCho.MaViTri,
                    tenViTri = datCho.MaViTriNavigation?.TenViTri,
                    tenKhuVuc = datCho.MaViTriNavigation?.MaKhuVucNavigation?.TenKhuVuc,
                    thoiGianDat = datCho.ThoiGianDat,
                    thoiGianDenDuKien = datCho.ThoiGianDenDuKien,
                    thoiGianHetHan = datCho.ThoiGianHetHan,
                    trangThaiDatCho = datCho.TrangThaiDatCho,
                    trangThaiText = GetTrangThaiText(datCho.TrangThaiDatCho)
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Lấy thống kê đặt chỗ
        [HttpGet]
        public async Task<IActionResult> GetThongKeDatCho()
        {
            try
            {
                var tongDatCho = await _context.DatChos.CountAsync();
                var tongDangCho = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 0);
                var tongDaDuyet = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 1);
                var tongHoanThanh = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 2);
                var tongTuChoi = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 3);
                var tongHuy = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 4);
                var tongHetHan = await _context.DatChos.CountAsync(dc => dc.TrangThaiDatCho == 5);

                // Đặt chỗ hôm nay
                var datChoHomNay = await _context.DatChos
                    .Where(dc => dc.ThoiGianDat!.Value.Date == DateTime.Today)
                    .CountAsync();

                // Đặt chỗ sắp tới (trong 7 ngày)
                var datChoSapToi = await _context.DatChos
                    .Where(dc =>
                        dc.ThoiGianDenDuKien >= DateTime.Now &&
                        dc.ThoiGianDenDuKien <= DateTime.Now.AddDays(7) &&
                        (dc.TrangThaiDatCho == 0 || dc.TrangThaiDatCho == 1))
                    .CountAsync();

                return Json(new
                {
                    success = true,
                    tongDatCho,
                    tongDangCho,
                    tongDaDuyet,
                    tongHoanThanh,
                    tongTuChoi,
                    tongHuy,
                    tongHetHan,
                    datChoHomNay,
                    datChoSapToi
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Background job: Tự động xử lý đặt chỗ hết hạn
        [HttpPost]
        public async Task<IActionResult> XuLyDatChoHetHan()
        {
            try
            {
                await _reservationService.XuLyDatChoHetHanAsync();
                return Json(new { success = true, message = "Đã xử lý các đặt chỗ hết hạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper: Lấy mã nhân viên hiện tại
        private async Task<int?> GetCurrentMaNhanVienAsync()
        {
            var maTaiKhoanStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTaiKhoanStr) || !int.TryParse(maTaiKhoanStr, out var maTaiKhoan))
            {
                return null;
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.MaTaiKhoan == maTaiKhoan);

            return nhanVien?.MaNhanVien;
        }

        private static string GetTrangThaiText(int? trangThai)
        {
            return trangThai switch
            {
                0 => "Chờ xử lý",
                1 => "Đã duyệt",
                2 => "Hoàn thành",
                3 => "Từ chối",
                4 => "Đã hủy",
                5 => "Hết hạn",
                _ => "Không xác định"
            };
        }
    }
}
