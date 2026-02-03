using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải là 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; } = null!;
    }
}
