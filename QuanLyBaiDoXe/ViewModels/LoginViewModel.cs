using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email ho?c Tên ??ng nh?p là b?t bu?c")]
        [Display(Name = "Email/Tên ??ng nh?p")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [DataType(DataType.Password)]
        [Display(Name = "M?t kh?u")]
        public string Password { get; set; } = null!;

        [Display(Name = "Ghi nh? ??ng nh?p")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
