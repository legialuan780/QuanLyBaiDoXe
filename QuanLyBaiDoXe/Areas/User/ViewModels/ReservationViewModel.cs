using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.Areas.User.ViewModels
{
    /// <summary>
    /// ViewModel cho đặt chỗ
    /// </summary>
    public class ReservationViewModel
    {
        public int MaDatCho { get; set; }
        public int? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public int? MaViTri { get; set; }
        public string? TenViTri { get; set; }
        public string? TenKhuVuc { get; set; }
        public DateTime? ThoiGianDat { get; set; }
        public DateTime? ThoiGianDenDuKien { get; set; }
        public DateTime? ThoiGianHetHan { get; set; }
        public int? TrangThaiDatCho { get; set; }
        public string? TrangThaiText { get; set; }
        public string? BienSoXe { get; set; }
        public int? MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
    }

    /// <summary>
    /// Request đặt chỗ
    /// </summary>
    public class CreateReservationRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn vị trí đỗ xe")]
        public int MaViTri { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập biển số xe")]
        public string BienSoXe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại xe")]
        public int MaLoaiXe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian đến dự kiến")]
        public DateTime ThoiGianDenDuKien { get; set; }

        public string? GhiChu { get; set; }
    }

    /// <summary>
    /// Request thanh toán đặt cọc cho đặt trong ngày
    /// </summary>
    public class ReservationPaymentRequest
    {
        [Required]
        public int MaDatCho { get; set; }

        [Required]
        public string PhuongThucThanhToan { get; set; } = "TienMat"; // TienMat hoặc MoMo

        public decimal TienCoc { get; set; }
    }

    /// <summary>
    /// Response sau khi tạo đặt chỗ
    /// </summary>
    public class ReservationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? MaDatCho { get; set; }
        public ReservationViewModel? Reservation { get; set; }
        public bool RequirePayment { get; set; }
        public decimal? TienCoc { get; set; }
    }

    /// <summary>
    /// ViewModel cho danh sách đặt chỗ
    /// </summary>
    public class ReservationListViewModel
    {
        public List<ReservationViewModel> DanhSachDatCho { get; set; } = new();
        public List<KhuVucDto> KhuVucs { get; set; } = new();
        public List<LoaiXeDto> LoaiXes { get; set; } = new();
        public int TongDangCho { get; set; }
        public int TongDaDuyet { get; set; }
        public int TongDaTuChoi { get; set; }
        public int TongHetHan { get; set; }
    }

    public class KhuVucDto
    {
        public int MaKhuVuc { get; set; }
        public string? TenKhuVuc { get; set; }
        public int? MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
        public int SoChoTrong { get; set; }
        public int TongSoCho { get; set; }
    }

    public class LoaiXeDto
    {
        public int MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
        public string? MoTa { get; set; }
        public decimal? GiaThang { get; set; }
    }

    public class ViTriDtoReservation
    {
        public int MaViTri { get; set; }
        public string? TenViTri { get; set; }
        public int TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public int? MaKhuVuc { get; set; }
        public string? TenKhuVuc { get; set; }
    }
}
