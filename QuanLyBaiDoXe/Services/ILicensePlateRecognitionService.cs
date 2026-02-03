namespace QuanLyBaiDoXe.Services
{
    public interface ILicensePlateRecognitionService
    {
        /// <summary>
        /// Nhận dạng biển số từ ảnh base64
        /// </summary>
        /// <param name="base64Image">Ảnh dạng base64 (có hoặc không có prefix data:image/...)</param>
        /// <returns>Kết quả nhận dạng biển số</returns>
        Task<LicensePlateResult> RecognizePlateAsync(string base64Image);

        /// <summary>
        /// Nhận dạng biển số từ file ảnh
        /// </summary>
        /// <param name="imagePath">Đường dẫn file ảnh</param>
        /// <returns>Kết quả nhận dạng biển số</returns>
        Task<LicensePlateResult> RecognizePlateFromFileAsync(string imagePath);
    }

    public class LicensePlateResult
    {
        /// <summary>
        /// Nhận dạng thành công hay không
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Biển số nhận dạng được (đã format)
        /// </summary>
        public string? PlateNumber { get; set; }

        /// <summary>
        /// Biển số gốc chưa format
        /// </summary>
        public string? RawPlateNumber { get; set; }

        /// <summary>
        /// Độ tin cậy (0-100%)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Thông báo lỗi nếu có
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Loại xe (car, motorcycle, truck...)
        /// </summary>
        public string? VehicleType { get; set; }

        /// <summary>
        /// Vùng/khu vực của biển số
        /// </summary>
        public string? Region { get; set; }
    }
}
