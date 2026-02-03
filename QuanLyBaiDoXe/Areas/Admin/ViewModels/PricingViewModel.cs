namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class PricingViewModel
    {
        public List<PricingConfigDto> PricingConfigs { get; set; } = new();
        public List<VehicleTypeSelectDto> VehicleTypes { get; set; } = new();
        public int TotalConfigs { get; set; }
        public int TotalActiveConfigs { get; set; }
        public int TotalVehicleTypes { get; set; }
    }

    public class PricingConfigDto
    {
        public int MaCauHinh { get; set; }
        public string? TenCauHinh { get; set; }
        public int? MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
        public string? GioBatDau { get; set; }
        public string? GioKetThuc { get; set; }
        public bool IsUuTien { get; set; }
        public int SoBlock { get; set; }
        public List<PricingDetailDto> ChiTietGia { get; set; } = new();
    }

    public class PricingDetailDto
    {
        public int MaChiTiet { get; set; }
        public int? MaCauHinh { get; set; }
        public int? ThuTuBlock { get; set; }
        public int? SoPhutCuaBlock { get; set; }
        public decimal? GiaTien { get; set; }
        public bool IsLuyTien { get; set; }
    }

    public class VehicleTypeSelectDto
    {
        public int MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
    }

    public class PricingConfigRequest
    {
        public int? MaCauHinh { get; set; }
        public string TenCauHinh { get; set; } = string.Empty;
        public int? MaLoaiXe { get; set; }
        public string? GioBatDau { get; set; }
        public string? GioKetThuc { get; set; }
        public bool IsUuTien { get; set; }
        public List<PricingDetailRequest> ChiTietGia { get; set; } = new();
    }

    public class PricingDetailRequest
    {
        public int? MaChiTiet { get; set; }
        public int ThuTuBlock { get; set; }
        public int SoPhutCuaBlock { get; set; }
        public decimal GiaTien { get; set; }
        public bool IsLuyTien { get; set; }
    }
}
