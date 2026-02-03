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

        // Danh sách khu vực với số chỗ trống
        public List<KhuVucChoTrongDto> KhuVucList { get; set; } = new();

        // Danh sách loại xe
        public List<LoaiXe> LoaiXeList { get; set; } = new();

        // Thống kê
        public int TongXeDangGui { get; set; }
        public int TongViTriTrong { get; set; }
        public int TongViTri { get; set; }
        public decimal TongThuHomNay { get; set; }
    }

    /// <summary>
    /// DTO cho khu vực với số chỗ trống
    /// </summary>
    public class KhuVucChoTrongDto
    {
        public int MaKhuVuc { get; set; }
        public string TenKhuVuc { get; set; } = string.Empty;
        public int SoChoTrong { get; set; }
        public int TongSoCho { get; set; }
        public List<ViTriDoDto> ViTriTrong { get; set; } = new();
    }

    /// <summary>
    /// DTO cho vị trí đỗ
    /// </summary>
    public class ViTriDoDto
    {
        public int MaViTri { get; set; }
            public string TenViTri { get; set; } = string.Empty;
        }

        public class QuetTheRequest
        {
            public string MaThe { get; set; } = string.Empty;
            public string? BienSo { get; set; }
            public string? HinhAnh { get; set; }
            public int? MaKhuVuc { get; set; } // Chỉ chọn khu vực, hệ thống tự gán vị trí
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

            /// <summary>
            /// Request model cho API nhận dạng biển số
            /// </summary>
            public class RecognizePlateRequest
            {
                /// <summary>
                /// Ảnh dạng base64 (có hoặc không có prefix data:image/...)
                /// </summary>
                public string ImageBase64 { get; set; } = string.Empty;
            }

            /// <summary>
            /// Response model cho API nhận dạng biển số
            /// </summary>
            public class RecognizePlateResponse
            {
                public bool Success { get; set; }
                public string? PlateNumber { get; set; }
                public string? RawPlate { get; set; }
                public double Confidence { get; set; }
                public string? VehicleType { get; set; }
                public string? Message { get; set; }
            }

            /// <summary>
            /// Request model cho xác nhận xe ra
            /// </summary>
            public class ConfirmXeRaRequest
            {
                public string MaThe { get; set; } = string.Empty;
                public string? BienSo { get; set; }
                public string? HinhAnh { get; set; }
                public string PhuongThucThanhToan { get; set; } = "cash"; // "cash" hoặc "momo"
            }
        }
