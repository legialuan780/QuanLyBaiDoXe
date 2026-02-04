using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải có từ 3-50 kí tự")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 kí tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_\-+=~`{}\[\]:;""'<>,.\/\\|])[A-Za-z\d@$!%*?&#^()_\-+=~`{}\[\]:;""'<>,.\/\\|]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ in hoa, một chữ thường, một kí tự đặc biệt")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 kí tự ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 kí tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = null!;

        [StringLength(20, ErrorMessage = "CCCD không được quá 20 kí tự")]
        [Display(Name = "CCCD/CMND")]
        public string? CCCD { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 kí tự")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "Biển số xe không được quá 20 kí tự")]
        [Display(Name = "Biển số xe (không bắt buộc)")]
        public string? LicensePlate { get; set; }
    }
}
