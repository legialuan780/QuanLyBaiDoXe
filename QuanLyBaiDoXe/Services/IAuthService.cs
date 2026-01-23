using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.ViewModels;

namespace QuanLyBaiDoXe.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? ErrorMessage, TaiKhoan? Account, string? Role)> AuthenticateAsync(string username, string password);
        Task<(bool Success, string? ErrorMessage, int? CustomerId)> RegisterCustomerAsync(RegisterViewModel model);
        Task<(bool Success, string? ErrorMessage, int? EmployeeId)> RegisterEmployeeAsync(RegisterViewModel model);
        Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> PhoneNumberExistsAsync(string phoneNumber);
        Task<bool> CCCDExistsAsync(string cccd);
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }
}
