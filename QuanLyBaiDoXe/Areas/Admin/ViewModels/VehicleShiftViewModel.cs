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

        // Trạng thái hiển thị động dựa trên thời gian thực
        public string GetDisplayStatus()
        {
            // Nếu đã chốt ca
            if (TrangThaiCa == 1)
            {
                return "completed"; // Đã chốt
            }

            // Nếu chưa chốt ca (TrangThaiCa = 0)
            if (!ThoiGianNhanCa.HasValue)
            {
                return "unknown";
            }

            var now = DateTime.Now;
            var shiftStartTime = ThoiGianNhanCa.Value;
            
            // Xác định loại ca dựa trên giờ nhận ca
            var hour = shiftStartTime.Hour;
            DateTime shiftStartDate = shiftStartTime.Date;
            DateTime shiftEndTime;
            
            if (hour >= 6 && hour < 14)
            {
                // Ca sáng: 6h - 14h
                shiftEndTime = shiftStartDate.AddHours(14);
            }
            else if (hour >= 14 && hour < 22)
            {
                // Ca chiều: 14h - 22h
                shiftEndTime = shiftStartDate.AddHours(22);
            }
            else
            {
                // Ca đêm: 22h - 6h (sang ngày hôm sau)
                if (hour >= 22)
                {
                    // Bắt đầu từ 22h hôm nay, kết thúc 6h ngày mai
                    shiftEndTime = shiftStartDate.AddDays(1).AddHours(6);
                }
                else
                {
                    // Bắt đầu từ 0h - 6h (tiếp tục ca đêm hôm trước)
                    shiftEndTime = shiftStartDate.AddHours(6);
                }
            }

            // Kiểm tra thời gian hiện tại có nằm trong ca không
            if (now >= shiftStartTime && now < shiftEndTime)
            {
                return "active"; // Đang trực (xanh lá)
            }
            
            // Nếu thời gian hiện tại đã qua thời gian kết thúc ca
            if (now >= shiftEndTime)
            {
                return "finished"; // Đã xong - Chờ chốt ca (vàng cam)
            }

            // Ca chưa bắt đầu
            return "upcoming"; // Sắp tới
        }

        public string TrangThaiCaText
        {
            get
            {
                var status = GetDisplayStatus();
                return status switch
                {
                    "active" => "Đang trực",
                    "finished" => "Chờ chốt",
                    "completed" => "Đã chốt",
                    "upcoming" => "Sắp tới",
                    _ => "Không xác định"
                };
            }
        }

        public string TrangThaiCaColor
        {
            get
            {
                var status = GetDisplayStatus();
                return status switch
                {
                    "active" => "success", // Xanh lá
                    "finished" => "warning", // Vàng cam
                    "completed" => "secondary", // Xám
                    "upcoming" => "info", // Xanh dương
                    _ => "info"
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

        // Tính số giờ làm real-time cho ca đang trực
        public double SoGioLamHienTai
        {
            get
            {
                if (!ThoiGianNhanCa.HasValue)
                    return 0;

                // Nếu ca chưa bắt đầu (sắp tới), không tính giờ
                if (ThoiGianNhanCa.Value > DateTime.Now)
                    return 0;

                // Nếu đã có thời gian giao ca (ca đã kết thúc), dùng nó
                if (ThoiGianGiaoCa.HasValue)
                {
                    return (ThoiGianGiaoCa.Value - ThoiGianNhanCa.Value).TotalHours;
                }

                // Nếu chưa có thời gian giao ca (ca đang trực), tính từ lúc bắt đầu đến hiện tại
                return (DateTime.Now - ThoiGianNhanCa.Value).TotalHours;
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

    public class DailyShiftViewModel
    {
        public DateTime Date { get; set; }
        public List<ShiftViewModel> Shifts { get; set; } = new List<ShiftViewModel>();
        public ShiftViewModel? CurrentShift { get; set; }
        public ShiftViewModel? NextShift { get; set; }
        public bool IsToday => Date.Date == DateTime.Today;
        public string DayOfWeekText => Date.ToString("dddd", new System.Globalization.CultureInfo("vi-VN"));
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

    // ViewModel cho lịch cá nhân của nhân viên
    public class PersonalScheduleViewModel
    {
        public EmployeeViewModel Employee { get; set; } = new EmployeeViewModel();
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<ScheduleViewModel> ScheduledShifts { get; set; } = new List<ScheduleViewModel>();
        public List<ShiftViewModel> ActualShifts { get; set; } = new List<ShiftViewModel>();

        public int TotalScheduledShifts => ScheduledShifts.Count;
        public int TotalActualShifts => ActualShifts.Count;
        public decimal TotalWorkHours => ActualShifts.Sum(s => (decimal)s.SoGioLam);
        public decimal TotalRevenue => ActualShifts.Sum(s => s.TongTienHeThong);
    }
}

