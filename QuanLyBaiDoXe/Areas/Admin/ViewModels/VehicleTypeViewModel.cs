namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class VehicleTypeViewModel
    {
        public List<VehicleTypeDto> VehicleTypes { get; set; } = new();
        public int TotalVehicleTypes { get; set; }
        public int TotalCards { get; set; }
        public int ActiveCards { get; set; }
    }

    public class VehicleTypeDto
    {
        public int MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
        public string? MoTa { get; set; }
        public int SoLuongThe { get; set; }
        public int SoLuongTheHoatDong { get; set; }
        public int SoLuongCauHinhGia { get; set; }
    }

    public class VehicleTypeRequest
    {
        public int? MaLoaiXe { get; set; }
        public string TenLoaiXe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }
}
