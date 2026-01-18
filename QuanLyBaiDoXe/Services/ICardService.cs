using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Services
{
    public interface ICardService
    {
        // Lấy danh sách thẻ xe
        Task<List<TheXe>> GetAllCardsAsync();
        
        // Lấy thẻ xe theo mã
        Task<TheXe?> GetCardByIdAsync(string maThe);
        
        // Lấy danh sách thẻ theo trạng thái
        Task<List<TheXe>> GetCardsByStatusAsync(int trangThai);
        
        // Lấy danh sách thẻ theo loại thẻ
        Task<List<TheXe>> GetCardsByTypeAsync(int loaiThe);
        
        // Lấy danh sách thẻ theo loại xe
        Task<List<TheXe>> GetCardsByVehicleTypeAsync(int maLoaiXe);
        
        // Tìm kiếm thẻ
        Task<List<TheXe>> SearchCardsAsync(string? keyword, int? loaiThe, int? maLoaiXe, int? trangThai);
        
        // Thêm thẻ mới
        Task<TheXe> CreateCardAsync(TheXe theXe);
        
        // Thêm nhiều thẻ
        Task<List<TheXe>> CreateMultipleCardsAsync(List<TheXe> theXes);
        
        // Cập nhật thẻ
        Task<TheXe?> UpdateCardAsync(TheXe theXe);
        
        // Khóa thẻ
        Task<bool> LockCardAsync(string maThe);
        
        // Mở khóa thẻ
        Task<bool> UnlockCardAsync(string maThe);
        
        // Xóa thẻ
        Task<bool> DeleteCardAsync(string maThe);
        
        // Kiểm tra thẻ có đang được sử dụng không
        Task<bool> IsCardInUseAsync(string maThe);
        
        // Lấy danh sách loại xe
        Task<List<LoaiXe>> GetVehicleTypesAsync();
        
        // Thống kê thẻ
        Task<CardStatistics> GetCardStatisticsAsync();
    }
    
    public class CardStatistics
    {
        public int TotalCards { get; set; }
        public int ActiveCards { get; set; }
        public int LockedCards { get; set; }
        public int MonthlyCards { get; set; }
        public int SingleCards { get; set; }
        public int CardsInUse { get; set; }
    }
}
