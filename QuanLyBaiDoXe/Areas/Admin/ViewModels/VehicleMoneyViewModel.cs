namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class VehicleMoneyViewModel
    {
        // Thống kê tổng quan
        public decimal TongDoanhThu { get; set; }
        public decimal DoanhThuLuotGui { get; set; }
        public decimal DoanhThuTheThang { get; set; }
        public int TongLuotGui { get; set; }
        public int TongTheThang { get; set; }

        // Thống kê theo thời gian
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuTuanNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public decimal DoanhThuNamNay { get; set; }

        // Chi tiết doanh thu lượt gửi theo ngày
        public List<VehicleMoneyDailyDto> DoanhThuLuotGuiTheoNgay { get; set; } = new();

        // Chi tiết doanh thu thẻ tháng theo ngày
        public List<VehicleMoneyDailyDto> DoanhThuTheThangTheoNgay { get; set; } = new();

        // Thống kê theo loại xe
        public List<VehicleMoneyByTypeDto> DoanhThuTheoLoaiXe { get; set; } = new();

        // Thống kê theo tháng (12 tháng gần nhất)
        public List<VehicleMoneyMonthlyDto> DoanhThuTheoThang { get; set; } = new();

        // Top khách hàng đóng tiền nhiều nhất
        public List<VehicleMoneyTopCustomerDto> TopKhachHang { get; set; } = new();
    }

    public class VehicleMoneyDailyDto
    {
        public DateTime Ngay { get; set; }
        public decimal TongTien { get; set; }
        public int SoLuong { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
    }

    public class VehicleMoneyByTypeDto
    {
        public string? TenLoaiXe { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int SoLuotGui { get; set; }
        public int SoTheThang { get; set; }
        public decimal TyLe { get; set; }
    }

    public class VehicleMoneyMonthlyDto
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal DoanhThuLuotGui { get; set; }
        public decimal DoanhThuTheThang { get; set; }
        public decimal TongDoanhThu { get; set; }
        public string ThangDisplay => $"{Thang:00}/{Nam}";
    }

    public class VehicleMoneyTopCustomerDto
    {
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public decimal TongTienDong { get; set; }
        public int SoTheThang { get; set; }
    }

    // Filter để lọc dữ liệu
    public class VehicleMoneyFilterDto
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? LoaiDoanhThu { get; set; } // "all", "luotgui", "thethang"
        public int? MaLoaiXe { get; set; }
    }
}

