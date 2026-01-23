namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class ShiftViewModel
    {
        public int MaCa { get; set; }
        public int? MaNhanVien { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public DateTime? ThoiGianNhanCa { get; set; }
        public DateTime? ThoiGianGiaoCa { get; set; }
        public decimal TienDauCa { get; set; }
        public decimal TongTienThu { get; set; }
        public int TrangThaiCa { get; set; }

        public string TrangThaiCaText
        {
            get
            {
                return TrangThaiCa switch
                {
                    0 => "Chưa bắt đầu",
                    1 => "Đang làm việc",
                    2 => "Đã kết thúc",
                    _ => "Không xác định"
                };
            }
        }

        public decimal TienCuoiCa => TienDauCa + TongTienThu;

        public double SoGioLam
        {
            get
            {
                if (ThoiGianNhanCa.HasValue && ThoiGianGiaoCa.HasValue)
                {
                    return (ThoiGianGiaoCa.Value - ThoiGianNhanCa.Value).TotalHours;
                }
                return 0;
            }
        }
    }

    public class ScheduleViewModel
    {
        public int MaLich { get; set; }
        public int? MaNhanVien { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public DateOnly? NgayLamViec { get; set; }
        public int? CaLamViec { get; set; }
        public string? GhiChu { get; set; }

        public string TenCa
        {
            get
            {
                return CaLamViec switch
                {
                    1 => "Ca sáng (6h-14h)",
                    2 => "Ca chiều (14h-22h)",
                    3 => "Ca đêm (22h-6h)",
                    _ => "Chưa xác định"
                };
            }
        }
    }

    public class EmployeeTimeSheetViewModel
    {
        public int MaNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public int ChucVu { get; set; }
        public int SoCaLam { get; set; }
        public decimal TongGioLam { get; set; }
        public decimal TongDoanhThu { get; set; }

        public string ChucVuText
        {
            get
            {
                return ChucVu switch
                {
                    1 => "Bảo vệ",
                    2 => "Thu ngân",
                    3 => "Giám sát",
                    4 => "Quản lý",
                    _ => "Nhân viên"
                };
            }
        }
    }

    public class EmployeeViewModel
    {
        public int MaNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public int ChucVu { get; set; }
        public DateOnly? NgayVaoLam { get; set; }
        public bool TrangThaiLamViec { get; set; }

        public string ChucVuText
        {
            get
            {
                return ChucVu switch
                {
                    1 => "Bảo vệ",
                    2 => "Thu ngân",
                    3 => "Giám sát",
                    4 => "Quản lý",
                    _ => "Nhân viên"
                };
            }
        }

        public int Tuoi
        {
            get
            {
                if (NgaySinh.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var age = today.Year - NgaySinh.Value.Year;
                    if (NgaySinh.Value > today.AddYears(-age)) age--;
                    return age;
                }
                return 0;
            }
        }
    }
}
