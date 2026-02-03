using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "TenDangNhap la bat buoc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "TenDangNhap phai co tu 3-50 ky tu")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "MatKhau la bat buoc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "MatKhau phai co it nhat 6 ky tu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "XacNhanMatKhau la bat buoc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "MatKhauXacNhan khong khop")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Email la bat buoc")]
        [EmailAddress(ErrorMessage = "Email khong hop le")]
        [StringLength(100, ErrorMessage = "Email khong duoc qua 100 ky tu")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "HoTen la bat buoc")]
        [StringLength(100, ErrorMessage = "HoTen khong duoc qua 100 ky tu")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "SoDienThoai la bat buoc")]
        [Phone(ErrorMessage = "SoDienThoai khong hop le")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "SoDienThoai khong hop le")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = null!;

        [StringLength(20, ErrorMessage = "CCCD khong duoc qua 20 ky tu")]
        [Display(Name = "CCCD/CMND")]
        public string? CCCD { get; set; }

        [StringLength(200, ErrorMessage = "DiaChi khong duoc qua 200 ky tu")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "BienSoXe khong duoc qua 20 ky tu")]
        [Display(Name = "Biển số xe (không bắt buộc)")]
        public string? LicensePlate { get; set; }
    }
}
