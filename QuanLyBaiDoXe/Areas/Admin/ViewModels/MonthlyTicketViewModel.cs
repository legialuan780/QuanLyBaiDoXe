namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class MonthlyTicketViewModel
    {
        public List<MonthlyTicketDto> MonthlyTickets { get; set; } = new();
        public List<CustomerSelectDto> Customers { get; set; } = new();
        public List<CardSelectDto> AvailableCards { get; set; } = new();
        public List<VehicleTypeSelectDto> VehicleTypes { get; set; } = new();
        public MonthlyTicketStatistics Statistics { get; set; } = new();
    }

    public class MonthlyTicketDto
    {
        public int MaTheThang { get; set; }
        public int? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public string? BienSoXe { get; set; }
        public string? MaThe { get; set; }
        public string? TenLoaiXe { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayHetHan { get; set; }
        public decimal? SoTienDong { get; set; }
        public bool? TrangThai { get; set; }
        public int SoNgayConLai { get; set; }
        public string TrangThaiText => TrangThai == true ? "Hoạt động" : "Hết hạn";
        public string TrangThaiClass => TrangThai == true 
            ? (SoNgayConLai <= 7 ? "warning" : "success") 
            : "danger";
    }

    public class CustomerSelectDto
    {
        public int MaKhachHang { get; set; }
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string? BienSoXeMacDinh { get; set; }
        public string DisplayText => $"{HoTen} - {SoDienThoai}";
    }

    public class CardSelectDto
    {
        public string MaThe { get; set; } = string.Empty;
        public string? TenLoaiXe { get; set; }
        public int? MaLoaiXe { get; set; }
        public string DisplayText => $"{MaThe} ({TenLoaiXe ?? "Chưa phân loại"})";
    }

    public class MonthlyTicketStatistics
    {
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int ExpiringTickets { get; set; }
        public int ExpiredTickets { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CreateMonthlyTicketRequest
    {
        public int MaKhachHang { get; set; }
        public string MaThe { get; set; } = string.Empty;
        public string? BienSoXe { get; set; }
        public int SoThang { get; set; } = 1;
        public decimal SoTienDong { get; set; }
    }

    public class RenewMonthlyTicketRequest
    {
        public int MaTheThang { get; set; }
        public int SoThang { get; set; } = 1;
        public decimal SoTienDong { get; set; }
    }
}
