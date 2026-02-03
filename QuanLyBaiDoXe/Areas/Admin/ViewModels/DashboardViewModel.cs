namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        // Thống kê chính
        public int XeTrongBai { get; set; }
        public int XeVaoHomNay { get; set; }
        public int XeRaHomNay { get; set; }
        public decimal DoanhThuHomNay { get; set; }

        // So sánh với hôm qua (%)
        public decimal TyLeXeTrongBai { get; set; }
        public decimal TyLeXeVao { get; set; }
        public decimal TyLeXeRa { get; set; }
        public decimal TyLeDoanhThu { get; set; }

        // Tình trạng bãi đỗ theo loại xe
        public List<ParkingStatusByType> TinhTrangBaiDo { get; set; } = new();

        // Vé tháng sắp hết hạn
        public List<ExpiringMonthlyTicket> VeThangSapHetHan { get; set; } = new();

        // Hoạt động gần đây
        public List<RecentActivityDto> HoatDongGanDay { get; set; } = new();

        // Thống kê doanh thu theo ngày (7 ngày gần nhất)
        public List<DailyRevenueDto> DoanhThuTheoNgay { get; set; } = new();

        // Thống kê doanh thu theo tháng
        public List<MonthlyRevenueDto> DoanhThuTheoThang { get; set; } = new();
    }

    public class ParkingStatusByType
    {
        public int MaLoaiXe { get; set; }
        public string TenLoaiXe { get; set; } = string.Empty;
        public int SoLuongDangGui { get; set; }
        public int TongSoViTri { get; set; }
        public int PhanTramSuDung => TongSoViTri > 0 ? (int)((double)SoLuongDangGui / TongSoViTri * 100) : 0;
        public string MauSac { get; set; } = "#21A691";
    }

    public class ExpiringMonthlyTicket
    {
        public int MaVeThang { get; set; }
        public string? BienSo { get; set; }
        public string? TenLoaiXe { get; set; }
        public DateOnly? NgayHetHan { get; set; }
        public int SoNgayConLai { get; set; }
        public string TrangThaiClass => SoNgayConLai <= 3 ? "danger" : SoNgayConLai <= 7 ? "warning" : "info";
    }

    public class RecentActivityDto
    {
        public long MaLuotGui { get; set; }
        public string? BienSoVao { get; set; }
        public string? BienSoRa { get; set; }
        public string? TenLoaiXe { get; set; }
        public DateTime ThoiGianVao { get; set; }
        public DateTime? ThoiGianRa { get; set; }
        public decimal? TongTien { get; set; }
        public int? TrangThai { get; set; }
        public string TrangThaiText => TrangThai switch
        {
            0 => "Đang gửi",
            1 => "Đã thanh toán",
            2 => "Vé tháng",
            _ => "Không xác định"
        };
        public string TrangThaiClass => TrangThai switch
        {
            0 => "info",
            1 => "success",
            2 => "warning",
            _ => "secondary"
        };
    }

    public class DailyRevenueDto
    {
        public DateTime Ngay { get; set; }
        public string TenNgay => Ngay.ToString("dd/MM");
        public decimal DoanhThu { get; set; }
        public int SoLuotGui { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string TenThang => $"T{Thang}";
        public decimal DoanhThu { get; set; }
        public int SoLuotGui { get; set; }
    }
}
