using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.EF;
using System.Globalization;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VehicleVisionController : Controller
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleVisionController(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await LoadParkingDataAsync();
            
            // Load trạng thái 3 quầy từ CaLamViec
            ViewBag.CountersStatus = await LoadCountersStatusAsync();

            return View(viewModel);
        }

        /// <summary>
        /// Load dữ liệu bãi đỗ xe từ database
        /// </summary>
        private async Task<VehicleVisionViewModel> LoadParkingDataAsync()
        {
            var viewModel = new VehicleVisionViewModel();

            // Lấy tất cả khu vực với loại xe
            var khuVucs = await _context.KhuVucs
                .Include(k => k.MaLoaiXeNavigation)
                .Include(k => k.ViTriDos)
                .OrderBy(k => k.MaKhuVuc)
                .ToListAsync();

            // Lấy các lượt gửi đang đỗ (TrangThai = 0: đang gửi)
            var luotGuiDangDo = await _context.LuotGuis
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Where(l => l.TrangThai == 0 && l.MaViTri != null)
                .ToListAsync();

            // Lấy các đặt chỗ đang chờ (TrangThaiDatCho = 0 hoặc 1)
            var datChoDangCho = await _context.DatChos
                .Include(d => d.MaKhachHangNavigation)
                .Where(d => d.TrangThaiDatCho == 0 || d.TrangThaiDatCho == 1)
                .Where(d => d.ThoiGianHetHan == null || d.ThoiGianHetHan > DateTime.Now)
                .ToListAsync();

            // Tạo dictionary để tra cứu nhanh
            var luotGuiByViTri = luotGuiDangDo
                .Where(l => l.MaViTri.HasValue)
                .ToDictionary(l => l.MaViTri!.Value, l => l);
            var datChoByViTri = datChoDangCho
                .Where(d => d.MaViTri.HasValue)
                .GroupBy(d => d.MaViTri!.Value)
                .ToDictionary(g => g.Key, g => g.First());


            // Map zone code dựa vào thứ tự
            var zoneCodes = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
            var zoneIndex = 0;

            foreach (var khuVuc in khuVucs)
            {
                var loaiXe = khuVuc.MaLoaiXeNavigation;
                
                // Bỏ qua khu vực nhân viên
                var tenLoaiXeLower = loaiXe?.TenLoaiXe?.ToLower() ?? "";
                if (tenLoaiXeLower.Contains("nhân viên") || tenLoaiXeLower.Contains("employee") || tenLoaiXeLower.Contains("vip"))
                {
                    continue;
                }
                
                var zoneCode = zoneIndex < zoneCodes.Length ? zoneCodes[zoneIndex] : $"Z{zoneIndex}";
                
                // Xác định icon và class dựa vào loại xe
                var (iconClass, vehicleTypeClass, gridClass) = GetVehicleTypeStyles(loaiXe?.TenLoaiXe ?? "Xe");

                var zone = new ParkingZoneViewModel
                {
                    MaKhuVuc = khuVuc.MaKhuVuc,
                    TenKhuVuc = khuVuc.TenKhuVuc ?? $"Khu {zoneCode}",
                    MoTaLoaiXe = loaiXe?.TenLoaiXe ?? "Xe",
                    IconClass = iconClass,
                    ZoneCode = zoneCode,
                    GridClass = gridClass,
                    VehicleTypeClass = vehicleTypeClass
                };

                // Xử lý từng vị trí đỗ trong khu vực
                var slotIndex = 1;
                foreach (var viTri in khuVuc.ViTriDos.OrderBy(v => v.MaViTri))
                {
                    var slotCode = $"{zoneCode}{slotIndex:D2}";
                    
                    // Xác định trạng thái thực tế
                    var trangThai = viTri.TrangThai ?? 0;
                    VehicleInfoViewModel? vehicleInfo = null;

                    // Kiểm tra nếu có xe đang đỗ
                    if (luotGuiByViTri.TryGetValue(viTri.MaViTri, out var luotGui))
                    {
                        trangThai = 1; // Đang đỗ
                        vehicleInfo = new VehicleInfoViewModel
                        {
                            MaLuotGui = luotGui.MaLuotGui,
                            BienSo = luotGui.BienSoVao ?? "Không rõ",
                            LoaiXe = luotGui.MaTheNavigation?.MaLoaiXeNavigation?.TenLoaiXe ?? loaiXe?.TenLoaiXe ?? "Xe",
                            ThoiGianVao = luotGui.ThoiGianVao,
                            PhiTamTinh = CalculateParkingFee(luotGui.ThoiGianVao, loaiXe)
                        };
                    }
                    // Kiểm tra nếu đã đặt chỗ
                    else if (datChoByViTri.TryGetValue(viTri.MaViTri, out var datCho))
                    {
                        trangThai = 2; // Đã đặt
                        vehicleInfo = new VehicleInfoViewModel
                        {
                            BienSo = "Đã đặt",
                            LoaiXe = loaiXe?.TenLoaiXe ?? "Xe",
                            ThoiGianVao = datCho.ThoiGianDat ?? DateTime.Now,
                            TenKhachHang = datCho.MaKhachHangNavigation?.HoTen,
                            ThoiGianDenDuKien = datCho.ThoiGianDenDuKien
                        };
                    }
                    // Nếu trạng thái là bảo trì
                    else if (viTri.TrangThai == 3)
                    {
                        trangThai = 3; // Bảo trì
                    }

                    var slot = new ParkingSlotViewModel
                    {
                        MaViTri = viTri.MaViTri,
                        TenViTri = viTri.TenViTri ?? slotCode,
                        SlotCode = slotCode,
                        MaKhuVuc = khuVuc.MaKhuVuc,
                        ZoneCode = zoneCode,
                        TrangThai = trangThai,
                        VehicleTypeClass = vehicleTypeClass,
                        IconClass = iconClass,
                        VehicleInfo = vehicleInfo
                    };

                    zone.Slots.Add(slot);
                    viewModel.AllSlots.Add(slot);
                    slotIndex++;
                }

                // Cập nhật thống kê khu vực
                zone.TotalSlots = zone.Slots.Count;
                zone.AvailableSlots = zone.Slots.Count(s => s.TrangThai == 0);
                zone.OccupiedSlots = zone.Slots.Count(s => s.TrangThai == 1);
                zone.ReservedSlots = zone.Slots.Count(s => s.TrangThai == 2);
                zone.MaintenanceSlots = zone.Slots.Count(s => s.TrangThai == 3);

                viewModel.Zones.Add(zone);
                zoneIndex++;
            }

            // Tính thống kê tổng
            viewModel.TotalSlots = viewModel.AllSlots.Count;
            viewModel.AvailableSlots = viewModel.AllSlots.Count(s => s.TrangThai == 0);
            viewModel.OccupiedSlots = viewModel.AllSlots.Count(s => s.TrangThai == 1);
            viewModel.ReservedSlots = viewModel.AllSlots.Count(s => s.TrangThai == 2);
            viewModel.MaintenanceSlots = viewModel.AllSlots.Count(s => s.TrangThai == 3);

            // Load hoạt động gần đây
            viewModel.RecentActivities = await LoadRecentActivitiesAsync();

            return viewModel;
        }

        /// <summary>
        /// Lấy icon và class dựa vào loại xe
        /// </summary>
        private (string iconClass, string vehicleTypeClass, string gridClass) GetVehicleTypeStyles(string tenLoaiXe)
        {
            var lower = tenLoaiXe.ToLower();
            
            if (lower.Contains("máy") || lower.Contains("moto") || lower.Contains("motorcycle"))
                return ("fa-motorcycle", "motorcycle", "motorcycle-grid");
            
            if (lower.Contains("tải") || lower.Contains("truck"))
                return ("fa-truck", "truck", "truck-grid");
            
            if (lower.Contains("7 chỗ") || lower.Contains("suv") || lower.Contains("van"))
                return ("fa-shuttle-van", "suv", "suv-grid");
            
            if (lower.Contains("4 chỗ") || lower.Contains("sedan"))
                return ("fa-car", "car", "car-grid");
            
            if (lower.Contains("nhân viên") || lower.Contains("vip") || lower.Contains("employee"))
                return ("fa-id-badge", "employee", "employee-grid");
            
            // Default
            return ("fa-car", "car", "car-grid");
        }

        /// <summary>
        /// Tính phí tạm tính dựa vào thời gian đỗ
        /// </summary>
        private decimal CalculateParkingFee(DateTime thoiGianVao, Models.Entities.LoaiXe? loaiXe)
        {
            var hours = (decimal)(DateTime.Now - thoiGianVao).TotalHours;
            if (hours < 0) hours = 0;

            // Giá mặc định theo loại xe
            var giaGio = loaiXe?.TenLoaiXe?.ToLower() switch
            {
                var t when t != null && t.Contains("máy") => 5000m,
                var t when t != null && t.Contains("tải") => 20000m,
                var t when t != null && (t.Contains("7 chỗ") || t.Contains("suv")) => 15000m,
                _ => 10000m
            };

            return Math.Ceiling(hours) * giaGio;
        }

        /// <summary>
        /// Load hoạt động gần đây
        /// </summary>
        private async Task<List<RecentActivityViewModel>> LoadRecentActivitiesAsync()
        {
            var activities = new List<RecentActivityViewModel>();

            // Lấy 10 lượt gửi gần nhất
            var recentLuotGuis = await _context.LuotGuis
                .OrderByDescending(l => l.ThoiGianVao)
                .Take(10)
                .ToListAsync();

            foreach (var luotGui in recentLuotGuis)
            {
                // Nếu đã ra
                if (luotGui.ThoiGianRa.HasValue)
                {
                    activities.Add(new RecentActivityViewModel
                    {
                        ActivityType = "exit",
                        BienSo = luotGui.BienSoRa ?? luotGui.BienSoVao ?? "Không rõ",
                        ThoiGian = luotGui.ThoiGianRa.Value
                    });

                    if (luotGui.TongTien.HasValue && luotGui.TongTien > 0)
                    {
                        activities.Add(new RecentActivityViewModel
                        {
                            ActivityType = "payment",
                            BienSo = luotGui.BienSoVao ?? "Không rõ",
                            SoTien = luotGui.TongTien,
                            ThoiGian = luotGui.ThoiGianRa.Value
                        });
                    }
                }
                
                // Xe vào
                activities.Add(new RecentActivityViewModel
                {
                    ActivityType = "entry",
                    BienSo = luotGui.BienSoVao ?? "Không rõ",
                    ThoiGian = luotGui.ThoiGianVao
                });
            }

            // Sắp xếp theo thời gian và lấy 10 hoạt động gần nhất
            return activities
                .OrderByDescending(a => a.ThoiGian)
                .Take(10)
                .ToList();
        }

        /// <summary>
        /// Load trạng thái các quầy thu phí
        /// </summary>
        private async Task<List<object>> LoadCountersStatusAsync()
        {
            var countersStatus = new List<object>();

            for (int i = 1; i <= 3; i++)
            {
                // Tìm ca làm việc đang hoạt động cho quầy này
                var activeShift = await _context.CaLamViecs
                    .Include(c => c.MaNhanVienNavigation)
                    .Where(c => c.TrangThaiCa == 0 && 
                                c.GhiChuBanGiao != null && 
                                c.GhiChuBanGiao.Contains($"Phân công quầy {i}"))
                    .OrderByDescending(c => c.ThoiGianNhanCa)
                    .FirstOrDefaultAsync();

                if (activeShift != null && activeShift.MaNhanVienNavigation != null)
                {
                    // Quầy đang hoạt động
                    var soGioLam = activeShift.ThoiGianNhanCa.HasValue 
                        ? (DateTime.Now - activeShift.ThoiGianNhanCa.Value).TotalHours 
                        : 0;

                    // Tính doanh thu từ LuotGuis trong ca
                    var revenue = await _context.LuotGuis
                        .Where(l => (l.MaCaVao == activeShift.MaCa || l.MaCaRa == activeShift.MaCa) 
                                 && l.TongTien.HasValue)
                        .SumAsync(l => l.TongTien ?? 0);

                    countersStatus.Add(new
                    {
                        Counter = i,
                        IsActive = true,
                        EmployeeName = activeShift.MaNhanVienNavigation.HoTen,
                        EmployeeCode = $"NV{activeShift.MaNhanVien:D4}",
                        ShiftHours = Math.Round(soGioLam, 1),
                        Revenue = revenue,
                        RevenueFormatted = revenue.ToString("N0", new CultureInfo("vi-VN")) + " VNĐ"
                    });
                }
                else
                {
                    // Quầy không hoạt động
                    countersStatus.Add(new
                    {
                        Counter = i,
                        IsActive = false,
                        EmployeeName = (string?)null,
                        EmployeeCode = (string?)null,
                        ShiftHours = (double?)null,
                        Revenue = 0m,
                        RevenueFormatted = "0 VNĐ"
                    });
                }
            }

            return countersStatus;
        }

        /// <summary>
        /// API: Lấy dữ liệu bãi đỗ xe (dùng cho refresh AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetParkingData()
        {
            try
            {
                var data = await LoadParkingDataAsync();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Lấy chi tiết vị trí đỗ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSlotDetails(int maViTri)
        {
            try
            {
                var viTri = await _context.ViTriDos
                    .Include(v => v.MaKhuVucNavigation)
                        .ThenInclude(k => k!.MaLoaiXeNavigation)
                    .FirstOrDefaultAsync(v => v.MaViTri == maViTri);

                if (viTri == null)
                    return Json(new { success = false, message = "Không tìm thấy vị trí" });

                // Kiểm tra có xe đang đỗ không
                var luotGui = await _context.LuotGuis
                    .Include(l => l.MaTheNavigation)
                    .Where(l => l.MaViTri == maViTri && l.TrangThai == 0)
                    .FirstOrDefaultAsync();

                // Kiểm tra có đặt chỗ không
                var datCho = await _context.DatChos
                    .Include(d => d.MaKhachHangNavigation)
                    .Where(d => d.MaViTri == maViTri && 
                               (d.TrangThaiDatCho == 0 || d.TrangThaiDatCho == 1) &&
                               (d.ThoiGianHetHan == null || d.ThoiGianHetHan > DateTime.Now))
                    .FirstOrDefaultAsync();

                var loaiXe = viTri.MaKhuVucNavigation?.MaLoaiXeNavigation;

                var result = new
                {
                    success = true,
                    data = new
                    {
                        MaViTri = viTri.MaViTri,
                        TenViTri = viTri.TenViTri,
                        TrangThai = luotGui != null ? 1 : datCho != null ? 2 : viTri.TrangThai ?? 0,
                        KhuVuc = viTri.MaKhuVucNavigation?.TenKhuVuc,
                        LoaiXe = loaiXe?.TenLoaiXe,
                        
                        // Thông tin xe đang đỗ
                        BienSo = luotGui?.BienSoVao,
                        ThoiGianVao = luotGui?.ThoiGianVao,
                        ThoiGianVaoFormatted = luotGui?.ThoiGianVao.ToString("dd/MM/yyyy HH:mm"),
                        ThoiGianDo = luotGui != null ? FormatTimeSpan(DateTime.Now - luotGui.ThoiGianVao) : null,
                        PhiTamTinh = luotGui != null ? CalculateParkingFee(luotGui.ThoiGianVao, loaiXe) : 0,
                        PhiTamTinhFormatted = luotGui != null 
                            ? CalculateParkingFee(luotGui.ThoiGianVao, loaiXe).ToString("N0") + " VNĐ" 
                            : null,
                        
                        // Thông tin đặt chỗ
                        TenKhachHang = datCho?.MaKhachHangNavigation?.HoTen,
                        ThoiGianDenDuKien = datCho?.ThoiGianDenDuKien?.ToString("dd/MM/yyyy HH:mm")
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours} giờ {ts.Minutes} phút";
            return $"{ts.Minutes} phút";
        }

        /// <summary>
        /// API: Cập nhật trạng thái vị trí (bảo trì)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateSlotStatus([FromBody] UpdateSlotStatusRequest request)
        {
            try
            {
                var viTri = await _context.ViTriDos.FindAsync(request.MaViTri);
                if (viTri == null)
                    return Json(new { success = false, message = "Không tìm thấy vị trí" });

                viTri.TrangThai = request.TrangThai;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Lấy thống kê bãi đỗ xe (dùng sau khi cập nhật trạng thái)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetParkingStatistics()
        {
            try
            {
                var viTriList = await _context.ViTriDos.ToListAsync();
                var luotGuiDangDo = await _context.LuotGuis
                    .Where(l => l.TrangThai == 0)
                    .Select(l => l.MaViTri)
                    .ToListAsync();

                var datChoHieuLuc = await _context.DatChos
                    .Where(d => (d.TrangThaiDatCho == 0 || d.TrangThaiDatCho == 1) &&
                               (d.ThoiGianHetHan == null || d.ThoiGianHetHan > DateTime.Now))
                    .Select(d => d.MaViTri)
                    .ToListAsync();

                int totalSlots = viTriList.Count;
                int occupiedSlots = 0;
                int reservedSlots = 0;
                int maintenanceSlots = 0;

                foreach (var viTri in viTriList)
                {
                    if (luotGuiDangDo.Contains(viTri.MaViTri))
                    {
                        occupiedSlots++;
                    }
                    else if (datChoHieuLuc.Contains(viTri.MaViTri))
                    {
                        reservedSlots++;
                    }
                    else if (viTri.TrangThai == 3)
                    {
                        maintenanceSlots++;
                    }
                }

                int availableSlots = totalSlots - occupiedSlots - reservedSlots - maintenanceSlots;

                return Json(new
                {
                    success = true,
                    totalSlots,
                    availableSlots,
                    occupiedSlots,
                    reservedSlots,
                    maintenanceSlots
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class UpdateSlotStatusRequest
    {
        public int MaViTri { get; set; }
        public int TrangThai { get; set; }
    }
}
