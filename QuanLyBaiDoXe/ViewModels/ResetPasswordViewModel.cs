using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_\-+=~`{}\[\]:;""'<>,.\/\\|])[A-Za-z\d@$!%*?&#^()_\-+=~`{}\[\]:;""'<>,.\/\\|]{8,}$", 
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ in hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
