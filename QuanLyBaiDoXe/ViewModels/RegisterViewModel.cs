using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Lo?i tài kho?n là b?t bu?c")]
        [Display(Name = "Lo?i tài kho?n")]
        public string AccountType { get; set; } = "Customer"; // Customer ho?c Employee

        [Required(ErrorMessage = "Tên ??ng nh?p là b?t bu?c")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên ??ng nh?p ph?i có t? 3-50 ký t?")]
        [Display(Name = "Tên ??ng nh?p")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "M?t kh?u ph?i có ít nh?t 6 ký t?")]
        [DataType(DataType.Password)]
        [Display(Name = "M?t kh?u")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Xác nh?n m?t kh?u là b?t bu?c")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
        [Display(Name = "Xác nh?n m?t kh?u")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "H? tên là b?t bu?c")]
        [StringLength(100, ErrorMessage = "H? tên không ???c quá 100 ký t?")]
        [Display(Name = "H? và tên")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "S? ?i?n tho?i là b?t bu?c")]
        [Phone(ErrorMessage = "S? ?i?n tho?i không h?p l?")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "S? ?i?n tho?i không h?p l?")]
        [Display(Name = "S? ?i?n tho?i")]
        public string PhoneNumber { get; set; } = null!;

        [StringLength(20, ErrorMessage = "CCCD không ???c quá 20 ký t?")]
        [Display(Name = "CCCD/CMND")]
        public string? CCCD { get; set; }

        [StringLength(200, ErrorMessage = "??a ch? không ???c quá 200 ký t?")]
        [Display(Name = "??a ch?")]
        public string? Address { get; set; }

        // Ch? cho Khách hàng
        [StringLength(20, ErrorMessage = "Bi?n s? xe không ???c quá 20 ký t?")]
        [Display(Name = "Bi?n s? xe (không b?t bu?c)")]
        public string? LicensePlate { get; set; }

        // Ch? cho Nhân viên
        [Display(Name = "Gi?i tính")]
        public string? Gender { get; set; } // Nam, N?, Khác

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [Display(Name = "Ch?c v?")]
        public int? Position { get; set; } // 0 = Admin, 1 = Nhân viên

        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateOnly? StartDate { get; set; }
    }
}
