using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Areas.Admin.ViewModels
{
    public class VehicleEntryViewModel
    {
        // Thông tin quẹt thẻ
        public string? MaThe { get; set; }
        public string? BienSo { get; set; }
        public string? HinhAnh { get; set; }
        public int? MaViTri { get; set; }

        // Thông tin thẻ xe (khi đã quẹt thẻ)
        public TheXe? TheXe { get; set; }

        // Thông tin lượt gửi hiện tại (nếu đang gửi)
        public LuotGui? LuotGuiHienTai { get; set; }

        // Danh sách xe đang trong bãi
        public List<LuotGui> XeDangTrongBai { get; set; } = new();

        // Danh sách vị trí đỗ trống
        public List<ViTriDo> ViTriTrong { get; set; } = new();

        // Danh sách loại xe
        public List<LoaiXe> LoaiXeList { get; set; } = new();

        // Thống kê
        public int TongXeDangGui { get; set; }
        public int TongViTriTrong { get; set; }
        public int TongViTri { get; set; }
        public decimal TongThuHomNay { get; set; }
    }

    public class QuetTheRequest
    {
        public string MaThe { get; set; } = string.Empty;
        public string? BienSo { get; set; }
        public string? HinhAnh { get; set; }
        public int? MaViTri { get; set; }
    }

    public class QuetTheResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "VAO" hoặc "RA"
        public LuotGuiDto? LuotGui { get; set; }
        public TheXeDto? TheXe { get; set; }
    }

    public class LuotGuiDto
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
        public string? ThoiGianGuiFormatted => ThoiGianVao.HasValue 
            ? (DateTime.Now - ThoiGianVao.Value).ToString(@"hh\:mm\:ss") 
            : null;
    }

    public class TheXeDto
    {
        public string MaThe { get; set; } = string.Empty;
        public string? TenLoaiXe { get; set; }
        public int? LoaiThe { get; set; }
        public string TenLoaiThe => LoaiThe == 0 ? "Vé lượt" : "Vé tháng";
        public int? TrangThai { get; set; }
        public bool DangGui { get; set; }
    }
}
