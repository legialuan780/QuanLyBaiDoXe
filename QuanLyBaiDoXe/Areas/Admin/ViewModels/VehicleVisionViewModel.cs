using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    /// <summary>
    /// ViewModel chính cho trang giám sát bãi đỗ xe
    /// </summary>
    public class VehicleVisionViewModel
    {
        // Thống kê tổng quan
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public int ReservedSlots { get; set; }
        public int MaintenanceSlots { get; set; }
        
        // Tỷ lệ phần trăm
        public int AvailablePercent => TotalSlots > 0 ? (int)Math.Round(AvailableSlots * 100.0 / TotalSlots) : 0;
        public int OccupiedPercent => TotalSlots > 0 ? (int)Math.Round(OccupiedSlots * 100.0 / TotalSlots) : 0;
        
        // Danh sách khu vực
        public List<ParkingZoneViewModel> Zones { get; set; } = new();
        
        // Danh sách vị trí đỗ (flat list cho JavaScript)
        public List<ParkingSlotViewModel> AllSlots { get; set; } = new();
        
        // Hoạt động gần đây
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    /// <summary>
    /// ViewModel cho khu vực đỗ xe
    /// </summary>
    public class ParkingZoneViewModel
    {
        public int MaKhuVuc { get; set; }
        public string TenKhuVuc { get; set; } = string.Empty;
        public string MoTaLoaiXe { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fa-car";
        public string ZoneCode { get; set; } = string.Empty; // A, B, C, D, E
        
        // Thống kê khu vực
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public int ReservedSlots { get; set; }
        public int MaintenanceSlots { get; set; }
        
        // Tỷ lệ phần trăm sử dụng
        public int OccupiedPercent => TotalSlots > 0 ? (int)Math.Round(OccupiedSlots * 100.0 / TotalSlots) : 0;
        public int ReservedPercent => TotalSlots > 0 ? (int)Math.Round(ReservedSlots * 100.0 / TotalSlots) : 0;
        public int MaintenancePercent => TotalSlots > 0 ? (int)Math.Round(MaintenanceSlots * 100.0 / TotalSlots) : 0;
        
        // CSS class cho grid
        public string GridClass { get; set; } = "car-grid";
        
        // CSS class cho loại xe (motorcycle, car, suv, truck, employee)
        public string VehicleTypeClass { get; set; } = "car";
        
        // Danh sách vị trí đỗ trong khu vực
        public List<ParkingSlotViewModel> Slots { get; set; } = new();
    }

    /// <summary>
    /// ViewModel cho từng vị trí đỗ xe
    /// </summary>
    public class ParkingSlotViewModel
    {
        public int MaViTri { get; set; }
        public string TenViTri { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty; // A01, B02, ...
        public int MaKhuVuc { get; set; }
        public string ZoneCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Trạng thái: 0 = Trống, 1 = Đang đỗ, 2 = Đã đặt, 3 = Bảo trì
        /// </summary>
        public int TrangThai { get; set; }
        public string TrangThaiText => TrangThai switch
        {
            0 => "available",
            1 => "occupied",
            2 => "reserved",
            3 => "maintenance",
            _ => "available"
        };
        
        // Thông tin xe đang đỗ (nếu có)
        public VehicleInfoViewModel? VehicleInfo { get; set; }
        
        // CSS class cho loại xe
        public string VehicleTypeClass { get; set; } = "car";
        public string IconClass { get; set; } = "fa-car";
    }

    /// <summary>
    /// Thông tin xe đang đỗ
    /// </summary>
    public class VehicleInfoViewModel
    {
        public long MaLuotGui { get; set; }
        public string BienSo { get; set; } = string.Empty;
        public string LoaiXe { get; set; } = string.Empty;
        public DateTime ThoiGianVao { get; set; }
        public string ThoiGianVaoFormatted => ThoiGianVao.ToString("dd/MM/yyyy HH:mm");
        
        // Tính thời gian đỗ
        public TimeSpan ThoiGianDo => DateTime.Now - ThoiGianVao;
        public string ThoiGianDoFormatted
        {
            get
            {
                var ts = ThoiGianDo;
                if (ts.TotalHours >= 1)
                    return $"{(int)ts.TotalHours} giờ {ts.Minutes} phút";
                return $"{ts.Minutes} phút";
            }
        }
        
        // Phí tạm tính
        public decimal PhiTamTinh { get; set; }
        public string PhiTamTinhFormatted => PhiTamTinh.ToString("N0") + " VNĐ";
        
        // Thông tin đặt chỗ (nếu có)
        public string? TenKhachHang { get; set; }
        public DateTime? ThoiGianDenDuKien { get; set; }
    }

    /// <summary>
    /// Hoạt động gần đây
    /// </summary>
    public class RecentActivityViewModel
    {
        public string ActivityType { get; set; } = "entry"; // entry, exit, payment
        public string BienSo { get; set; } = string.Empty;
        public decimal? SoTien { get; set; }
        public DateTime ThoiGian { get; set; }
        
        public string TimeAgo
        {
            get
            {
                var diff = DateTime.Now - ThoiGian;
                if (diff.TotalMinutes < 1) return "Vừa xong";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
                return $"{(int)diff.TotalDays} ngày trước";
            }
        }
        
        public string IconClass => ActivityType switch
        {
            "entry" => "fa-arrow-right",
            "exit" => "fa-arrow-left",
            "payment" => "fa-dollar-sign",
            _ => "fa-info-circle"
        };
        
        public string ActivityClass => ActivityType;
    }
}
