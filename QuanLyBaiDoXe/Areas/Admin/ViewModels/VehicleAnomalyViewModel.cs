using System;

namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class VehicleAnomalyViewModel
    {
        public int MaSuCo { get; set; }
        public DateTime? ThoiGianGhiNhan { get; set; }
        public int? MaNhanVien { get; set; }
        public string? TenNhanVien { get; set; }
        public string? LoaiSuCo { get; set; }
        public string? MaThe { get; set; }
        public int? MaViTri { get; set; }
        public string? MoTaChiTiet { get; set; }
        public int? TrangThaiXuLy { get; set; }
        public string? TrangThaiXuLyText { get; set; }
        public string? MucDoUuTien { get; set; }
        public string? LinkVideo { get; set; }
        public string? LinkHinhAnh { get; set; }
        
        // Thông tin x? lý
        public int? NhanVienXuLyId { get; set; }
        public string? NhanVienXuLyTen { get; set; }
        public DateTime? ThoiGianXuLy { get; set; }
        public string? GhiChuXuLy { get; set; }
    }

    public class AnomalyFilterViewModel
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? LoaiSuCo { get; set; }
        public int? TrangThaiXuLy { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class AnomalyStatisticsViewModel
    {
        public int TongSuCo { get; set; }
        public int ChuaXuLy { get; set; }
        public int DangXuLy { get; set; }
        public int DaXuLy { get; set; }
        public int KhanCap { get; set; }
        public double ThoiGianXuLyTrungBinh { get; set; }
    }
}
