using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class CustomerController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public CustomerController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 10)
        {
            // Ensure UTF-8 response encoding
            Response.ContentType = "text/html; charset=utf-8";
            
            var query = _context.KhachHangs
                .Include(k => k.MaTaiKhoanNavigation)
                .Include(k => k.TheThangs)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(k =>
                    k.HoTen.Contains(keyword) ||
                    k.SoDienThoai.Contains(keyword) ||
                    (k.Cccd != null && k.Cccd.Contains(keyword)) ||
                    (k.BienSoXeMacDinh != null && k.BienSoXeMacDinh.Contains(keyword))
                );
            }

            // Get statistics
            var totalCustomers = await _context.KhachHangs.CountAsync();
            var customersWithAccount = await _context.KhachHangs.CountAsync(k => k.MaTaiKhoan != null);
            var customersWithMonthlyTicket = await _context.KhachHangs
                .CountAsync(k => k.TheThangs.Any(v => v.TrangThai == true && v.NgayHetHan >= DateOnly.FromDateTime(DateTime.Now)));

            // Pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var customers = await query
                .OrderByDescending(k => k.MaKhachHang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new CustomerViewModel
            {
                Customers = customers,
                SearchKeyword = keyword,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                TotalCustomers = totalCustomers,
                CustomersWithAccount = customersWithAccount,
                CustomersWithMonthlyTicket = customersWithMonthlyTicket
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.KhachHangs
                .Include(k => k.MaTaiKhoanNavigation)
                .Include(k => k.TheThangs)
                    .ThenInclude(v => v.MaTheNavigation)
                        .ThenInclude(t => t.MaLoaiXeNavigation)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm th?y khách hàng!" });
            }

            return Json(new
            {
                success = true,
                customer = new
                {
                    maKhachHang = customer.MaKhachHang,
                    hoTen = customer.HoTen,
                    soDienThoai = customer.SoDienThoai,
                    cccd = customer.Cccd,
                    diaChi = customer.DiaChi,
                    bienSoXeMacDinh = customer.BienSoXeMacDinh,
                    maTaiKhoan = customer.MaTaiKhoan,
                    tenDangNhap = customer.MaTaiKhoanNavigation?.TenDangNhap,
                    veThangs = customer.TheThangs.Select(v => new
                    {
                        maVeThang = v.MaTheThang,
                        maThe = v.MaThe,
                        tenLoaiXe = v.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe,
                        ngayBatDau = v.NgayBatDau,
                        ngayHetHan = v.NgayHetHan,
                        soTienDong = v.SoTienDong,
                        trangThai = v.TrangThai
                    })
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Check if phone number already exists
                var existingPhone = await _context.KhachHangs
                    .AnyAsync(k => k.SoDienThoai == request.SoDienThoai);

                if (existingPhone)
                {
                    return Json(new { success = false, message = "S? ?i?n tho?i ?ã t?n t?i!" });
                }

                // Check if CCCD already exists
                if (!string.IsNullOrWhiteSpace(request.Cccd))
                {
                    var existingCccd = await _context.KhachHangs
                        .AnyAsync(k => k.Cccd == request.Cccd);

                    if (existingCccd)
                    {
                        return Json(new { success = false, message = "CCCD ?ã t?n t?i!" });
                    }
                }

                var customer = new KhachHang
                {
                    HoTen = request.HoTen.Trim(),
                    SoDienThoai = request.SoDienThoai.Trim(),
                    Cccd = string.IsNullOrWhiteSpace(request.Cccd) ? null : request.Cccd.Trim(),
                    DiaChi = string.IsNullOrWhiteSpace(request.DiaChi) ? null : request.DiaChi.Trim(),
                    BienSoXeMacDinh = string.IsNullOrWhiteSpace(request.BienSoXeMacDinh) ? null : request.BienSoXeMacDinh.Trim().ToUpper()
                };

                _context.KhachHangs.Add(customer);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm khách hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateCustomerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var customer = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaKhachHang == request.MaKhachHang);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm th?y khách hàng!" });
                }

                // Check if phone number already exists (excluding current customer)
                var existingPhone = await _context.KhachHangs
                    .AnyAsync(k => k.SoDienThoai == request.SoDienThoai && k.MaKhachHang != request.MaKhachHang);

                if (existingPhone)
                {
                    return Json(new { success = false, message = "S? ?i?n tho?i ?ã t?n t?i!" });
                }

                // Check if CCCD already exists (excluding current customer)
                if (!string.IsNullOrWhiteSpace(request.Cccd))
                {
                    var existingCccd = await _context.KhachHangs
                        .AnyAsync(k => k.Cccd == request.Cccd && k.MaKhachHang != request.MaKhachHang);

                    if (existingCccd)
                    {
                        return Json(new { success = false, message = "CCCD ?ã t?n t?i!" });
                    }
                }

                customer.HoTen = request.HoTen.Trim();
                customer.SoDienThoai = request.SoDienThoai.Trim();
                customer.Cccd = string.IsNullOrWhiteSpace(request.Cccd) ? null : request.Cccd.Trim();
                customer.DiaChi = string.IsNullOrWhiteSpace(request.DiaChi) ? null : request.DiaChi.Trim();
                customer.BienSoXeMacDinh = string.IsNullOrWhiteSpace(request.BienSoXeMacDinh) ? null : request.BienSoXeMacDinh.Trim().ToUpper();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "C?p nh?t khách hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _context.KhachHangs
                    .Include(k => k.TheThangs)
                    .Include(k => k.DatChos)
                    .FirstOrDefaultAsync(k => k.MaKhachHang == id);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm th?y khách hàng!" });
                }

                // Check if customer has active monthly tickets
                var hasActiveTickets = customer.TheThangs.Any(v => v.TrangThai == true);
                if (hasActiveTickets)
                {
                    return Json(new { success = false, message = "Không th? xóa khách hàng có vé tháng ?ang ho?t ??ng!" });
                }

                // Check if customer has active bookings
                var hasActiveBookings = customer.DatChos.Any(d => d.TrangThaiDatCho == 0);
                if (hasActiveBookings)
                {
                    return Json(new { success = false, message = "Không th? xóa khách hàng có ??t ch? ?ang ho?t ??ng!" });
                }

                _context.KhachHangs.Remove(customer);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa khách hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var totalCustomers = await _context.KhachHangs.CountAsync();
            var customersWithAccount = await _context.KhachHangs.CountAsync(k => k.MaTaiKhoan != null);
            var customersWithMonthlyTicket = await _context.KhachHangs
                .CountAsync(k => k.TheThangs.Any(v => v.TrangThai == true && v.NgayHetHan >= DateOnly.FromDateTime(DateTime.Now)));
            var newCustomersThisMonth = await _context.KhachHangs
                .CountAsync(k => k.MaKhachHang >= 1); // Simplified - you may want to add a CreatedDate column

            return Json(new
            {
                totalCustomers,
                customersWithAccount,
                customersWithMonthlyTicket,
                newCustomersThisMonth
            });
        }
    }

    // Request models
    public class CreateCustomerRequest
    {
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Cccd { get; set; }
        public string? DiaChi { get; set; }
        public string? BienSoXeMacDinh { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Cccd { get; set; }
        public string? DiaChi { get; set; }
        public string? BienSoXeMacDinh { get; set; }
    }
}
