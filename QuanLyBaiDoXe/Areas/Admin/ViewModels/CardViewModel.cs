using QuanLyBaiDoXe.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class CardViewModel
    {
        public List<TheXe> Cards { get; set; } = new();
        public List<LoaiXe> VehicleTypes { get; set; } = new();
        public CardStatisticsViewModel Statistics { get; set; } = new();
        
        // Filters
        public string? SearchKeyword { get; set; }
        public int? FilterLoaiThe { get; set; }
        public int? FilterLoaiXe { get; set; }
        public int? FilterTrangThai { get; set; }
    }

    public class CardStatisticsViewModel
    {
        public int TotalCards { get; set; }
        public int ActiveCards { get; set; }
        public int LockedCards { get; set; }
        public int MonthlyCards { get; set; }
        public int SingleCards { get; set; }
        public int CardsInUse { get; set; }
    }

    public class CreateCardRequest
    {
        [Required(ErrorMessage = "Mã thẻ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã thẻ không quá 50 ký tự")]
        public string MaThe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại xe là bắt buộc")]
        public int MaLoaiXe { get; set; }

        public int LoaiThe { get; set; } = 0; // 0: Vé lượt, 1: Vé tháng

        public int TrangThai { get; set; } = 1; // 1: Hoạt động
    }

    public class CreateMultipleCardsRequest
    {
        [Required(ErrorMessage = "Tiền tố là bắt buộc")]
        public string Prefix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số bắt đầu là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số bắt đầu phải lớn hơn 0")]
        public int StartNumber { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số lượng từ 1 đến 1000")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Loại xe là bắt buộc")]
        public int MaLoaiXe { get; set; }

        public int LoaiThe { get; set; } = 0;
    }

    public class UpdateCardRequest
    {
        [Required]
        public string MaThe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại xe là bắt buộc")]
        public int MaLoaiXe { get; set; }

        public int LoaiThe { get; set; }

        public int TrangThai { get; set; }
    }

    public class CardDto
    {
        public string MaThe { get; set; } = string.Empty;
        public int? MaLoaiXe { get; set; }
        public string? TenLoaiXe { get; set; }
        public int? LoaiThe { get; set; }
        public string TenLoaiThe => LoaiThe == 0 ? "Vé lượt" : "Vé tháng";
        public int? TrangThai { get; set; }
        public string TenTrangThai => TrangThai == 1 ? "Hoạt động" : "Đã khóa";
        public bool DangSuDung { get; set; }
    }
}
