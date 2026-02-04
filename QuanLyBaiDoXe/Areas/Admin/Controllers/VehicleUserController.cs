using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VehicleUserController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleUserController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        // GET: Admin/VehicleUser
        public async Task<IActionResult> Index()
        {
            var users = await _context.TaiKhoans
                .OrderByDescending(t => t.MaTaiKhoan)
                .Select(t => new VehicleUserViewModel
                {
                    MaTaiKhoan = t.MaTaiKhoan,
                    TenDangNhap = t.TenDangNhap,
                    QuyenHan = t.QuyenHan,
                    Email = t.Email,
                    TrangThai = t.TrangThai ?? true
                })
                .ToListAsync();

            // Thống kê
            var allUsers = await _context.TaiKhoans.ToListAsync();
            var viewModel = new VehicleUserListViewModel
            {
                Users = users,
                TotalUsers = allUsers.Count,
                AdminCount = allUsers.Count(u => u.QuyenHan == "Admin"),
                CustomerCount = allUsers.Count(u => u.QuyenHan == "Khách hàng"),
                EmployeeCount = allUsers.Count(u => u.QuyenHan == "Nhân viên"),
                ActiveUsers = allUsers.Count(u => u.TrangThai == true),
                InactiveUsers = allUsers.Count(u => u.TrangThai == false)
            };

            return View(viewModel);
        }

        // GET: Admin/VehicleUser/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.KhachHangs)
                .Include(t => t.NhanViens)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
            {
                return NotFound();
            }

            var viewModel = new VehicleUserViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                QuyenHan = taiKhoan.QuyenHan,
                Email = taiKhoan.Email,
                TrangThai = taiKhoan.TrangThai ?? true
            };

            return View(viewModel);
        }

        // GET: Admin/VehicleUser/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/VehicleUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại
                if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại
                if (!string.IsNullOrWhiteSpace(model.Email) && 
                    await _context.TaiKhoans.AnyAsync(t => t.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau), // Hash mật khẩu
                    QuyenHan = model.QuyenHan,
                    Email = model.Email,
                    TrangThai = model.TrangThai
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/VehicleUser/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            var viewModel = new VehicleUserViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                QuyenHan = taiKhoan.QuyenHan,
                Email = taiKhoan.Email,
                TrangThai = taiKhoan.TrangThai ?? true
            };

            return View(viewModel);
        }

        // POST: Admin/VehicleUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleUserViewModel model)
        {
            if (id != model.MaTaiKhoan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var taiKhoan = await _context.TaiKhoans.FindAsync(id);
                    if (taiKhoan == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra tên đăng nhập đã tồn tại (ngoại trừ chính nó)
                    if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap && t.MaTaiKhoan != id))
                    {
                        ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                        return View(model);
                    }

                    // Kiểm tra email đã tồn tại (ngoại trừ chính nó)
                    if (!string.IsNullOrWhiteSpace(model.Email) && 
                        await _context.TaiKhoans.AnyAsync(t => t.Email == model.Email && t.MaTaiKhoan != id))
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng");
                        return View(model);
                    }

                    taiKhoan.TenDangNhap = model.TenDangNhap;
                    taiKhoan.QuyenHan = model.QuyenHan;
                    taiKhoan.Email = model.Email;
                    taiKhoan.TrangThai = model.TrangThai;

                    // Chỉ cập nhật mật khẩu nếu người dùng nhập mật khẩu mới
                    if (!string.IsNullOrWhiteSpace(model.MatKhau))
                    {
                        taiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
                    }

                    _context.Update(taiKhoan);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaiKhoanExists(model.MaTaiKhoan))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        // POST: Admin/VehicleUser/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            // Không cho xóa admin cuối cùng
            var adminCount = await _context.TaiKhoans.CountAsync(t => t.QuyenHan == "Admin");
            if (taiKhoan.QuyenHan == "Admin" && adminCount <= 1)
            {
                return Json(new { success = false, message = "Không thể xóa admin cuối cùng trong hệ thống" });
            }

            try
            {
                _context.TaiKhoans.Remove(taiKhoan);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa người dùng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể xóa người dùng: " + ex.Message });
            }
        }

        // POST: Admin/VehicleUser/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            try
            {
                taiKhoan.TrangThai = !(taiKhoan.TrangThai ?? true);
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Thay đổi trạng thái thành công",
                    newStatus = taiKhoan.TrangThai ?? true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/VehicleUser/GetStatistics
        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var allUsers = await _context.TaiKhoans.ToListAsync();
            
            var stats = new
            {
                totalUsers = allUsers.Count,
                adminCount = allUsers.Count(u => u.QuyenHan == "Admin"),
                customerCount = allUsers.Count(u => u.QuyenHan == "Khách hàng"),
                employeeCount = allUsers.Count(u => u.QuyenHan == "Nhân viên"),
                activeUsers = allUsers.Count(u => u.TrangThai == true),
                inactiveUsers = allUsers.Count(u => u.TrangThai == false)
            };

            return Json(stats);
        }

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoans.Any(e => e.MaTaiKhoan == id);
        }
    }
}
