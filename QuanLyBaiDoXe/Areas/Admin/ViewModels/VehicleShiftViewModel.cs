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
        public decimal TongTienHeThong { get; set; }
        public decimal TienMatBanGiao { get; set; }
        public string? GhiChuBanGiao { get; set; }
        public int TrangThaiCa { get; set; }

        public string TrangThaiCaText
        {
            get
            {
                return TrangThaiCa switch
                {
                    0 => "Đang trực",
                    1 => "Đã chốt",
                    _ => "Không xác định"
                };
            }
        }

        public decimal TienCuoiCa => TienDauCa + TongTienHeThong;

        public decimal ChenhLech => TienMatBanGiao - TienCuoiCa;

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
                    0 => "Admin",
                    1 => "Quản lý",
                    2 => "Bảo vệ",
                    3 => "Kỹ thuật",
                    4 => "Nhân viên",
                    _ => "Chưa xác định"
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
                    0 => "Admin",
                    1 => "Quản lý",
                    2 => "Bảo vệ",
                    3 => "Kỹ thuật",
                    4 => "Nhân viên",
                    _ => "Chưa xác định"
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

    public class WeeklyScheduleViewModel
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<EmployeeViewModel> Employees { get; set; } = new List<EmployeeViewModel>();
        public List<ScheduleViewModel> ExistingSchedules { get; set; } = new List<ScheduleViewModel>();
    }

    public class DailyScheduleViewModel
    {
        public DateTime SelectedDate { get; set; }
        public List<EmployeeViewModel> AllEmployees { get; set; } = new List<EmployeeViewModel>();
        public List<ScheduleViewModel> ScheduledShifts { get; set; } = new List<ScheduleViewModel>();
        public List<ShiftViewModel> ActiveShifts { get; set; } = new List<ShiftViewModel>();
        public int ActiveEmployees => ActiveShifts.Select(s => s.MaNhanVien).Distinct().Count();
    }

    public class ShiftCalendarViewModel
    {
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public Dictionary<DateTime, List<ShiftViewModel>> ShiftsByDate { get; set; } = new Dictionary<DateTime, List<ShiftViewModel>>();
        public MonthStatsViewModel MonthStats { get; set; } = new MonthStatsViewModel();
    }

    public class MonthStatsViewModel
    {
        public int TotalActiveShifts { get; set; }
        public int TotalCompletedShifts { get; set; }
        public decimal TotalWorkHours { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ViewModel chi tiết nhân viên
    public class EmployeeDetailViewModel
    {
        public EmployeeViewModel Employee { get; set; } = new EmployeeViewModel();
        public List<ShiftViewModel> RecentShifts { get; set; } = new List<ShiftViewModel>();
        public EmployeeStatsViewModel Stats { get; set; } = new EmployeeStatsViewModel();
    }

    public class EmployeeStatsViewModel
    {
        public int TotalShifts { get; set; }
        public int ActiveShifts { get; set; }
        public int CompletedShifts { get; set; }
        public decimal TotalWorkHours { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageShiftHours { get; set; }
        public int CurrentMonthShifts { get; set; }
        public decimal CurrentMonthHours { get; set; }
    }
}
