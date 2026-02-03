using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
using System.Security.Claims;

namespace QuanLyBaiDoXe.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Customer")]
    public class IncidentController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public IncidentController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReportIncident([FromBody] ReportIncidentRequest request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
                var customer = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaKhachHang == customerId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng!" });
                }

                // Tạo sự cố với prefix khách hàng trong MoTaChiTiet
                var suCo = new SuCo
                {
                    ThoiGianGhiNhan = DateTime.Now,
                    LoaiSuCo = request.LoaiSuCo,
                    MaThe = request.MaThe,
                    MaViTri = request.MaViTri,
                    MoTaChiTiet = $"[KH#{customerId} - {customer.HoTen} - {customer.SoDienThoai}] {request.MoTaChiTiet}",
                    TrangThaiXuLy = 0, // Chưa xử lý
                    MaNhanVien = null
                };

                _context.SuCos.Add(suCo);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Báo cáo sự cố thành công! Chúng tôi sẽ xử lý trong thời gian sớm nhất." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyIncidents()
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerId")?.Value ?? "0");
                
                // Tìm tất cả sự cố có chứa mã khách hàng trong MoTaChiTiet
                var incidents = await _context.SuCos
                    .Include(sc => sc.MaNhanVienNavigation)
                    .Where(sc => sc.MoTaChiTiet!.Contains($"[KH#{customerId}"))
                    .OrderByDescending(sc => sc.ThoiGianGhiNhan)
                    .Select(sc => new
                    {
                        maSuCo = sc.MaSuCo,
                        thoiGianGhiNhan = sc.ThoiGianGhiNhan,
                        loaiSuCo = sc.LoaiSuCo,
                        maThe = sc.MaThe,
                        maViTri = sc.MaViTri,
                        // Remove customer prefix from description
                        moTaChiTiet = sc.MoTaChiTiet != null && sc.MoTaChiTiet.Contains("]") 
                            ? sc.MoTaChiTiet.Substring(sc.MoTaChiTiet.IndexOf("]") + 2)
                            : sc.MoTaChiTiet,
                        trangThaiXuLy = sc.TrangThaiXuLy,
                        nguoiXuLy = sc.MaNhanVienNavigation != null ? sc.MaNhanVienNavigation.HoTen : "Chưa phân công"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = incidents });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    public class ReportIncidentRequest
    {
        public string LoaiSuCo { get; set; } = null!;
        public string? MaThe { get; set; }
        public int? MaViTri { get; set; }
        public string MoTaChiTiet { get; set; } = null!;
    }
}
