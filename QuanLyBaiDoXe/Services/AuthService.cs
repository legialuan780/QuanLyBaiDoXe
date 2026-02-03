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
                // Tìm tài khoản theo tên đăng nhập
                var account = await _context.TaiKhoans
                    .Include(t => t.NhanVien)
                    .Include(t => t.KhachHang)
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (account == null)
                {
                    return (false, "Tên đăng nhập hoặc mật khẩu không đúng!", null, null);
                }

                // Kiểm tra trạng thái tài khoản
                if (account.TrangThai == false)
                {
                    return (false, "Tài khoản đã bị khóa!", null, null);
                }

                // Kiểm tra mật khẩu - so sánh trực tiếp (plain text)
                if (account.MatKhau != password)
                {
                    return (false, "Tên đăng nhập hoặc mật khẩu không đúng!", null, null);
                }

                // Xác định role dựa trên QuyenHan trong database
                string role = account.QuyenHan; // "Admin", "Nhân viên", hoặc "Khách hàng"

                // Kiểm tra trạng thái nhân viên nếu là Admin hoặc Nhân viên
                if ((role == "Admin" || role == "Nhân viên") && account.NhanVien != null)
                {
                    if (account.NhanVien.TrangThaiLamViec == false)
                    {
                        return (false, "Nhân viên đã nghỉ việc!", null, null);
                    }
                }

                // Map role to English for consistency in the application
                string mappedRole = role switch
                {
                    "Admin" => "Admin",
                    "Nhân viên" => "Employee",
                    "Khách hàng" => "Customer",
                    _ => "Customer"
                };

                return (true, null, account, mappedRole);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}", null, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, int? CustomerId)> RegisterCustomerAsync(RegisterViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra tên đăng nhập đã tồn tại
                if (await UsernameExistsAsync(model.Username))
                {
                    return (false, "Tên đăng nhập đã tồn tại!", null);
                }

                // Kiểm tra email đã tồn tại
                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "Email đã được đăng ký!", null);
                }

                // Kiểm tra số điện thoại đã tồn tại
                if (await PhoneNumberExistsAsync(model.PhoneNumber))
                {
                    return (false, "Số điện thoại đã được đăng ký!", null);
                }

                // Kiểm tra CCCD nếu có
                if (!string.IsNullOrEmpty(model.CCCD))
                {
                    if (await CCCDExistsAsync(model.CCCD))
                    {
                        return (false, "CCCD/CMND đã được đăng ký!", null);
                    }
                }

                // Tạo tài khoản với quyền "Khách hàng"
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.Username.Trim(),
                    MatKhau = model.Password, // Plain text password
                    QuyenHan = "Khách hàng", // Chỉ cho phép đăng ký quyền Khách hàng
                    Email = model.Email.Trim(),
                    TrangThai = true
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                // Tạo khách hàng
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
                return (false, $"Lỗi hệ thống: {ex.Message}", null);
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.TaiKhoans
                .AnyAsync(t => t.TenDangNhap == username.Trim());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.TaiKhoans
                .AnyAsync(t => t.Email == email.Trim());
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

            // Verify old password - so sánh trực tiếp plain text
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
            // Không mã hóa, trả về plain text
            return password;
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            // So sánh trực tiếp plain text
            return hashedPassword == password;
        }

        public async Task<(bool Success, string? ErrorMessage, TaiKhoan? Account)> GetAccountByEmailAsync(string email)
        {
            try
            {
                var account = await _context.TaiKhoans
                    .Include(t => t.NhanVien)
                    .Include(t => t.KhachHang)
                    .FirstOrDefaultAsync(t => t.Email == email);

                if (account == null)
                {
                    return (false, "Email không tồn tại trong hệ thống!", null);
                }

                if (account.TrangThai == false)
                {
                    return (false, "Tài khoản đã bị khóa!", null);
                }

                return (true, null, account);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}", null);
            }
        }

        public async Task<string> GenerateOtpAsync(int accountId)
        {
            // Invalidate any existing OTPs for this account
            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.MaTaiKhoan == accountId && !t.IsUsed)
                .ToListAsync();

            foreach (var existingToken in existingTokens)
            {
                existingToken.IsUsed = true;
            }

            // Generate 6-digit OTP
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            var resetToken = new PasswordResetToken
            {
                MaTaiKhoan = accountId,
                Token = otpCode,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(5), // OTP expires in 5 minutes
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            return otpCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> VerifyOtpAsync(string email, string otpCode)
        {
            try
            {
                var account = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.Email == email);

                if (account == null)
                {
                    return (false, "Email không tồn tại trong hệ thống!");
                }

                var resetToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == account.MaTaiKhoan 
                        && t.Token == otpCode 
                        && !t.IsUsed);

                if (resetToken == null)
                {
                    return (false, "Mã OTP không đúng!");
                }

                if (resetToken.ExpiresAt < DateTime.Now)
                {
                    return (false, "Mã OTP đã hết hạn! Vui lòng yêu cầu mã mới.");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordWithOtpAsync(string email, string otpCode, string newPassword)
        {
            try
            {
                var account = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.Email == email);

                if (account == null)
                {
                    return (false, "Email không tồn tại trong hệ thống!");
                }

                var resetToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == account.MaTaiKhoan 
                        && t.Token == otpCode 
                        && !t.IsUsed);

                if (resetToken == null)
                {
                    return (false, "Mã OTP không hợp lệ!");
                }

                if (resetToken.ExpiresAt < DateTime.Now)
                {
                    return (false, "Mã OTP đã hết hạn!");
                }

                // Update password
                account.MatKhau = newPassword;
                resetToken.IsUsed = true;

                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
