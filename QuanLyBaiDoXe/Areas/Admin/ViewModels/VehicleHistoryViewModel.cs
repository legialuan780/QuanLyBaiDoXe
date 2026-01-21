namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class VehicleHistoryViewModel
    {
        public List<VehicleHistoryDto> Vehicles { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchKeyword { get; set; }
        public int? FilterLoaiXe { get; set; }
        public int? FilterTrangThai { get; set; }
        public List<LoaiXeDto> VehicleTypes { get; set; } = new();

        // Statistics
        public int TotalVehicles { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
    }

    public class VehicleHistoryDto
    {
        public long MaLuotGui { get; set; }
        public string? MaThe { get; set; }
        public string? BienSoVao { get; set; }
        public string? BienSoRa { get; set; }
        public string? HinhAnhVao { get; set; }
        public string? HinhAnhRa { get; set; }
        public DateTime? ThoiGianVao { get; set; }
        public DateTime? ThoiGianRa { get; set; }
        public string? TenViTri { get; set; }
        public string? TenKhuVuc { get; set; }
        public string? TenLoaiXe { get; set; }
        public decimal? TongTien { get; set; }
        public int? TrangThai { get; set; }
        public string TrangThaiText => TrangThai == 0 ? "?ang g?i" : "?ã l?y xe";
        public TimeSpan? ThoiGianGui
        {
            get
            {
                if (ThoiGianVao == null) return null;
                var endTime = ThoiGianRa ?? DateTime.Now;
                return endTime - ThoiGianVao.Value;
            }
        }
        public string ThoiGianGuiFormatted
        {
            get
            {
                if (ThoiGianGui == null) return "--:--";
                var ts = ThoiGianGui.Value;
                if (ts.TotalDays >= 1)
                    return $"{(int)ts.TotalDays} ngày {ts.Hours:D2}:{ts.Minutes:D2}";
                return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }
    }

    public class LoaiXeDto
    {
        public int MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
    }

    public class VehicleHistorySearchRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Keyword { get; set; }
        public int? LoaiXe { get; set; }
        public int? TrangThai { get; set; }
    }
}
