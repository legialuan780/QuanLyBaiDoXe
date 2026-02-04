using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.Areas.User.ViewModels;

namespace QuanLyBaiDoXe.Services
{
    public interface IReservationService
    {
        /// <summary>
        /// Tạo đặt chỗ mới
        /// </summary>
        Task<(bool Success, string Message, DatCho? DatCho)> TaoDatChoAsync(
            int maKhachHang, 
            int maViTri, 
            DateTime thoiGianDenDuKien,
            string bienSoXe,
            int maLoaiXe);

        /// <summary>
        /// Lấy danh sách đặt chỗ của khách hàng
        /// </summary>
        Task<List<ReservationViewModel>> GetDatChoByKhachHangAsync(int maKhachHang);

        /// <summary>
        /// Lấy danh sách đặt chỗ chờ duyệt (cho Admin)
        /// </summary>
        Task<List<ReservationViewModel>> GetDatChoChoDuyetAsync();

        /// <summary>
        /// Lấy tất cả đặt chỗ (cho Admin)
        /// </summary>
        Task<List<ReservationViewModel>> GetAllDatChoAsync();

        /// <summary>
        /// Duyệt đặt chỗ (Admin)
        /// </summary>
        Task<(bool Success, string Message)> DuyetDatChoAsync(int maDatCho, int maNhanVien);

        /// <summary>
        /// Từ chối đặt chỗ (Admin)
        /// </summary>
        Task<(bool Success, string Message)> TuChoiDatChoAsync(int maDatCho, string lyDo);

        /// <summary>
        /// Hủy đặt chỗ (User)
        /// </summary>
        Task<(bool Success, string Message)> HuyDatChoAsync(int maDatCho, int maKhachHang);

        /// <summary>
        /// Kiểm tra vị trí có đặt chỗ không
        /// </summary>
        Task<bool> KiemTraViTriDaDatChoAsync(int maViTri, DateTime? thoiGianDenDuKien = null, DateTime? thoiGianHetHan = null);

        /// <summary>
        /// Lấy thông tin đặt chỗ theo ID
        /// </summary>
        Task<DatCho?> GetDatChoByIdAsync(int maDatCho);

        /// <summary>
        /// Xử lý đặt chỗ hết hạn tự động
        /// </summary>
        Task XuLyDatChoHetHanAsync();

        /// <summary>
        /// Lấy danh sách vị trí trống có thể đặt cho loại xe
        /// </summary>
        Task<List<ViTriDtoReservation>> GetViTriTrongTheLoaiXeAsync(int maLoaiXe, DateTime? thoiGianDenDuKien = null);

        /// <summary>
        /// Thiết lập vị trí đã đặt trước cho ngày mai (Admin setup)
        /// </summary>
        Task<(bool Success, string Message)> SetupViTriDatTruocAsync(int maDatCho);
    }
}
