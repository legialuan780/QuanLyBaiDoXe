using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyBaiDoXe.Services
{
    public class AuthService : IAuthService
    {
        private readonly QuanLyBaiDoXeContext _context;

        public AuthService(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage, TaiKhoan? Account, string? Role)> AuthenticateAsync(string username, string password)
        {
            try
            {
                // Tìm tài kho?n theo tên ??ng nh?p
                var account = await _context.TaiKhoans
                    .Include(t => t.NhanVien)
                    .Include(t => t.KhachHang)
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (account == null)
                {
                    return (false, "Tên ??ng nh?p ho?c m?t kh?u không ?úng!", null, null);
                }

                // Ki?m tra tr?ng thái tài kho?n
                if (account.TrangThai == false)
                {
                    return (false, "Tài kho?n ?ã b? khóa!", null, null);
                }

                // Ki?m tra m?t kh?u - so sánh tr?c ti?p (plain text)
                if (account.MatKhau != password)
                {
                    return (false, "Tên ??ng nh?p ho?c m?t kh?u không ?úng!", null, null);
                }

                // Xác ??nh role
                string role = "Customer";
                if (account.NhanVien != null)
                {
                    if (account.NhanVien.TrangThaiLamViec == false)
                    {
                        return (false, "Nhân viên ?ã ngh? vi?c!", null, null);
                    }

                    // Xác ??nh role d?a trên ch?c v?
                    // 0: Admin, 1: Nhân viên
                    role = account.NhanVien.ChucVu == 0 ? "Admin" : "Employee";
                }
                else if (account.KhachHang != null)
                {
                    role = "Customer";
                }

                return (true, null, account, role);
            }
            catch (Exception ex)
            {
                return (false, $"L?i h? th?ng: {ex.Message}", null, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, int? CustomerId)> RegisterCustomerAsync(RegisterViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Ki?m tra tên ??ng nh?p ?ã t?n t?i
                if (await UsernameExistsAsync(model.Username))
                {
                    return (false, "Tên ??ng nh?p ?ã t?n t?i!", null);
                }

                // Ki?m tra s? ?i?n tho?i ?ã t?n t?i
                if (await PhoneNumberExistsAsync(model.PhoneNumber))
                {
                    return (false, "S? ?i?n tho?i ?ã ???c ??ng ký!", null);
                }

                // Ki?m tra CCCD n?u có
                if (!string.IsNullOrEmpty(model.CCCD))
                {
                    if (await CCCDExistsAsync(model.CCCD))
                    {
                        return (false, "CCCD/CMND ?ã ???c ??ng ký!", null);
                    }
                }

                // T?o tài kho?n - L?u m?t kh?u plain text
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.Username.Trim(),
                    MatKhau = model.Password, // Plain text password
                    TrangThai = true
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                // T?o khách hàng
                var khachHang = new KhachHang
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    SoDienThoai = model.PhoneNumber.Trim(),
                    HoTen = model.FullName.Trim(),
                    Cccd = model.CCCD?.Trim(),
                    DiaChi = model.Address?.Trim(),
                    BienSoXeMacDinh = model.LicensePlate?.Trim().ToUpper()
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, null, khachHang.MaKhachHang);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"L?i h? th?ng: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, int? EmployeeId)> RegisterEmployeeAsync(RegisterViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Ki?m tra tên ??ng nh?p ?ã t?n t?i
                if (await UsernameExistsAsync(model.Username))
                {
                    return (false, "Tên ??ng nh?p ?ã t?n t?i!", null);
                }

                // Ki?m tra CCCD ?ã t?n t?i (b?t bu?c cho nhân viên)
                if (string.IsNullOrEmpty(model.CCCD))
                {
                    return (false, "CCCD/CMND là b?t bu?c ??i v?i nhân viên!", null);
                }

                if (await CCCDExistsAsync(model.CCCD))
                {
                    return (false, "CCCD/CMND ?ã ???c ??ng ký!", null);
                }

                // T?o tài kho?n - L?u m?t kh?u plain text
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.Username.Trim(),
                    MatKhau = model.Password, // Plain text password
                    TrangThai = true
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                // T?o nhân viên
                var nhanVien = new NhanVien
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = model.FullName.Trim(),
                    GioiTinh = model.Gender?.Trim(),
                    NgaySinh = model.DateOfBirth,
                    Cccd = model.CCCD.Trim(),
                    SoDienThoai = model.PhoneNumber.Trim(),
                    DiaChi = model.Address?.Trim(),
                    ChucVu = model.Position ?? 1, // M?c ??nh là nhân viên (1)
                    NgayVaoLam = model.StartDate ?? DateOnly.FromDateTime(DateTime.Now),
                    TrangThaiLamViec = true
                };

                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, null, nhanVien.MaNhanVien);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"L?i h? th?ng: {ex.Message}", null);
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.TaiKhoans
                .AnyAsync(t => t.TenDangNhap == username.Trim());
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            var existsInCustomer = await _context.KhachHangs
                .AnyAsync(k => k.SoDienThoai == phoneNumber.Trim());
            
            var existsInEmployee = await _context.NhanViens
                .AnyAsync(n => n.SoDienThoai == phoneNumber.Trim());

            return existsInCustomer || existsInEmployee;
        }

        public async Task<bool> CCCDExistsAsync(string cccd)
        {
            var existsInCustomer = await _context.KhachHangs
                .AnyAsync(k => k.Cccd == cccd.Trim());
            
            var existsInEmployee = await _context.NhanViens
                .AnyAsync(n => n.Cccd == cccd.Trim());

            return existsInCustomer || existsInEmployee;
        }

        public async Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword)
        {
            var account = await _context.TaiKhoans.FindAsync(accountId);
            if (account == null)
            {
                return false;
            }

            // Verify old password - so sánh tr?c ti?p plain text
            if (account.MatKhau != oldPassword)
            {
                return false;
            }

            // Update password - plain text
            account.MatKhau = newPassword;
            await _context.SaveChangesAsync();

            return true;
        }

        public string HashPassword(string password)
        {
            // Không mã hóa, tr? v? plain text
            return password;
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            // So sánh tr?c ti?p plain text
            return hashedPassword == password;
        }
    }
}
