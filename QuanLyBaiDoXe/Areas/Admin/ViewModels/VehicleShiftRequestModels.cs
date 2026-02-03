using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    // ========== COUNTER ASSIGNMENT REQUEST MODELS ==========

    /// <summary>
    /// Request model cho phân công nhân viên vào một quầy cụ thể
    /// </summary>
    public class SingleCounterAssignmentRequest
    {
        [Required(ErrorMessage = "Số quầy không được để trống")]
        [Range(1, 3, ErrorMessage = "Số quầy phải từ 1 đến 3")]
        public int Counter { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }
    }

    /// <summary>
    /// Request model cho đóng quầy (kết thúc ca)
    /// </summary>
    public class CloseCounterRequest
    {
        [Required(ErrorMessage = "Số quầy không được để trống")]
        [Range(1, 3, ErrorMessage = "Số quầy phải từ 1 đến 3")]
        public int Counter { get; set; }

        [Required(ErrorMessage = "Tiền bàn giao không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền bàn giao không được âm")]
        public decimal TienMatBanGiao { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChu { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model cho phân công nhiều quầy cùng lúc
    /// </summary>
    public class CounterAssignmentRequest
    {
        [Required(ErrorMessage = "Danh sách phân công không được trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 phân công")]
        public List<CounterAssignment> Assignments { get; set; } = new List<CounterAssignment>();
    }

    /// <summary>
    /// Model cho một phân công quầy
    /// </summary>
    public class CounterAssignment
    {
        [Required(ErrorMessage = "Số quầy không được để trống")]
        [Range(1, 3, ErrorMessage = "Số quầy phải từ 1 đến 3")]
        public int Counter { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }
    }

    // ========== SHIFT MANAGEMENT REQUEST MODELS ==========

    /// <summary>
    /// Request model cho tạo ca làm việc mới
    /// </summary>
    public class CreateShiftRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Tiền đầu ca không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền đầu ca không được âm")]
        public decimal TienDauCa { get; set; }
    }

    /// <summary>
    /// Request model cho chốt ca làm việc
    /// </summary>
    public class EndShiftRequest
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int MaCa { get; set; }

        [Required(ErrorMessage = "Tiền bàn giao không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền bàn giao không được âm")]
        public decimal TienMatBanGiao { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChuBanGiao { get; set; }
    }

    /// <summary>
    /// Request model cho tạo nhiều ca làm việc cùng lúc
    /// </summary>
    public class CreateMultipleShiftsRequest
    {
        [Required(ErrorMessage = "Danh sách ca không được trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 ca")]
        public List<ShiftCreationData> Shifts { get; set; } = new List<ShiftCreationData>();
    }

    /// <summary>
    /// Model cho dữ liệu tạo ca
    /// </summary>
    public class ShiftCreationData
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Thời gian nhận ca không được để trống")]
        public string ThoiGianNhanCa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiền đầu ca không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền đầu ca không được âm")]
        public decimal TienDauCa { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChuBanGiao { get; set; }
    }

    /// <summary>
    /// Request model cho cập nhật nhiều ca trong ngày
    /// </summary>
    public class UpdateDayShiftsRequest
    {
        [Required(ErrorMessage = "Danh sách cập nhật không được trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 cập nhật")]
        public List<ShiftUpdateData> Updates { get; set; } = new List<ShiftUpdateData>();
    }

    /// <summary>
    /// Model cho dữ liệu cập nhật ca
    /// </summary>
    public class ShiftUpdateData
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int MaCa { get; set; }

        public int? MaNhanVien { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tiền đầu ca không được âm")]
        public decimal TienDauCa { get; set; }

        public string? GhiChuBanGiao { get; set; }
    }

    /// <summary>
    /// Request model cho điều chỉnh giờ làm việc
    /// </summary>
    public class AdjustShiftRequest
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int ShiftId { get; set; }

        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ vào ca không đúng định dạng (HH:mm)")]
        public string? CheckIn { get; set; }

        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ ra ca không đúng định dạng (HH:mm)")]
        public string? CheckOut { get; set; }

        [Required(ErrorMessage = "Lý do điều chỉnh không được để trống")]
        [StringLength(500, ErrorMessage = "Lý do không được quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model cho thêm giờ bù/tăng ca
    /// </summary>
    public class OvertimeRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Ngày làm thêm không được để trống")]
        public string Date { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ bắt đầu không được để trống")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ bắt đầu không đúng định dạng (HH:mm)")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ kết thúc không được để trống")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Giờ kết thúc không đúng định dạng (HH:mm)")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại làm thêm không được để trống")]
        [Range(1, 3, ErrorMessage = "Loại làm thêm phải từ 1 đến 3 (1: Ngày thường, 2: Ngày nghỉ, 3: Lễ)")]
        public int Type { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Request model cho ngắt ca (nghỉ đột xuất)
    /// </summary>
    public class BreakShiftRequest
    {
        [Required(ErrorMessage = "Mã ca không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã ca không hợp lệ")]
        public int ShiftId { get; set; }

        [Required(ErrorMessage = "Loại nghỉ không được để trống")]
        [Range(1, 3, ErrorMessage = "Loại nghỉ phải từ 1 đến 3 (1: Nghỉ phép, 2: Nghỉ ốm, 3: Khác)")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Lý do nghỉ không được để trống")]
        [StringLength(500, ErrorMessage = "Lý do không được quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;

        public bool NeedReplacement { get; set; }

        public int? ReplacementEmployeeId { get; set; }
    }

    // ========== SCHEDULE REQUEST MODELS ==========

    /// <summary>
    /// Request model cho thêm/lưu lịch làm việc
    /// </summary>
    public class AddScheduleRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        public DateOnly NgayLamViec { get; set; }

        [Required(ErrorMessage = "Ca làm việc không được để trống")]
        [Range(1, 3, ErrorMessage = "Ca làm việc phải từ 1 đến 3 (1: Sáng, 2: Chiều, 3: Đêm)")]
        public int CaLamViec { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChu { get; set; }
    }

    /// <summary>
    /// Request model cho lưu lịch ca làm việc
    /// </summary>
    public class SaveScheduleRequest
    {
        public int? MaLich { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        public string NgayLamViec { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ca làm việc không được để trống")]
        [Range(1, 3, ErrorMessage = "Ca làm việc phải từ 1 đến 3 (1: Sáng, 2: Chiều, 3: Đêm)")]
        public int CaLamViec { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChu { get; set; }
    }

    /// <summary>
    /// Request model cho thêm lịch làm việc mới (với giờ cụ thể)
    /// </summary>
    public class AddWorkScheduleRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        public string NgayLamViec { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ bắt đầu không được để trống")]
        public string GioBatDau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ kết thúc không được để trống")]
        public string GioKetThuc { get; set; } = string.Empty;

        [Range(0, 2, ErrorMessage = "Loại ca phải từ 0 đến 2 (0: Thường, 1: Tăng ca, 2: Đêm)")]
        public int LoaiCa { get; set; } = 0;

        [Range(0, 1, ErrorMessage = "Trạng thái phải là 0 (Nghỉ) hoặc 1 (Làm)")]
        public int TrangThai { get; set; } = 1;

        [StringLength(255, ErrorMessage = "Ghi chú không được quá 255 ký tự")]
        public string? GhiChu { get; set; }
    }

    /// <summary>
    /// Request model cho cập nhật lịch làm việc
    /// </summary>
    public class UpdateWorkScheduleRequest
    {
        [Required(ErrorMessage = "Mã lịch không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã lịch không hợp lệ")]
        public int MaLich { get; set; }

        public string? NgayLamViec { get; set; }

        public string? GioBatDau { get; set; }

        public string? GioKetThuc { get; set; }

        public int? LoaiCa { get; set; }

        public int? TrangThai { get; set; }

        public string? GhiChu { get; set; }
    }

    // ========== EMPLOYEE MANAGEMENT REQUEST MODELS ==========

    /// <summary>
    /// Request model cho cập nhật thông tin nhân viên
    /// </summary>
    public class UpdateEmployeeRequest
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã nhân viên không hợp lệ")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Giới tính không được quá 10 ký tự")]
        public string? GioiTinh { get; set; }

        public string? NgaySinh { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        public string? SoDienThoai { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [Range(0, 4, ErrorMessage = "Chức vụ phải từ 0 đến 4 (0: Admin, 1: Quản lý, 2: Bảo vệ, 3: Kỹ thuật, 4: Nhân viên)")]
        public int ChucVu { get; set; }

        public string? NgayVaoLam { get; set; }

        public bool TrangThaiLamViec { get; set; }
    }

    /// <summary>
    /// Request model cho tạo nhân viên mới
    /// </summary>
    public class CreateEmployeeRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Giới tính không được quá 10 ký tự")]
        public string? GioiTinh { get; set; }

        public string? NgaySinh { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        public string? SoDienThoai { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [Range(0, 4, ErrorMessage = "Chức vụ phải từ 0 đến 4 (0: Admin, 1: Quản lý, 2: Bảo vệ, 3: Kỹ thuật, 4: Nhân viên)")]
        public int ChucVu { get; set; }

        public string? NgayVaoLam { get; set; }

        public bool TrangThaiLamViec { get; set; } = true;
    }

    // ========== SCHEDULE REQUEST APPROVAL MODELS ==========

    /// <summary>
    /// Request model cho duyệt/từ chối yêu cầu
    /// </summary>
    public class ApproveRejectRequestRequest
    {
        [Required(ErrorMessage = "Mã yêu cầu không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã yêu cầu không hợp lệ")]
        public int RequestId { get; set; }

        [StringLength(255, ErrorMessage = "Ghi chú không được quá 255 ký tự")]
        public string? Note { get; set; }
    }
}
