using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.ViewModels;

namespace QuanLyBaiDoXe.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? ErrorMessage, TaiKhoan? Account, string? Role)> AuthenticateAsync(string username, string password);
        Task<(bool Success, string? ErrorMessage, int? CustomerId)> RegisterCustomerAsync(RegisterViewModel model);
        Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneNumberExistsAsync(string phoneNumber);
        Task<bool> CCCDExistsAsync(string cccd);
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);

        // Forgot Password methods with OTP
        Task<(bool Success, string? ErrorMessage, TaiKhoan? Account)> GetAccountByEmailAsync(string email);
        Task<string> GenerateOtpAsync(int accountId);
        Task<(bool Success, string? ErrorMessage)> VerifyOtpAsync(string email, string otpCode);
        Task<(bool Success, string? ErrorMessage)> ResetPasswordWithOtpAsync(string email, string otpCode, string newPassword);
    }
}
