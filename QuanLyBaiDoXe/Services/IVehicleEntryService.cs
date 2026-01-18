using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Services
{
    public interface IVehicleEntryService
    {
        // Lấy thông tin thẻ xe theo mã thẻ
        Task<TheXe?> GetTheXeByMaTheAsync(string maThe);
        
        // Lấy danh sách thẻ xe đang hoạt động
        Task<List<TheXe>> GetActiveTheXeListAsync();
        
        // Lấy vị trí đỗ trống
        Task<List<ViTriDo>> GetAvailableViTriDoAsync();
        
        // Xử lý xe vào
        Task<LuotGui> XuLyXeVaoAsync(string maThe, string bienSoVao, string? hinhAnhVao, int? maViTri);
        
        // Xử lý xe ra
        Task<LuotGui?> XuLyXeRaAsync(string maThe, string bienSoRa, string? hinhAnhRa);
        
        // Lấy lượt gửi đang trong bãi theo mã thẻ
        Task<LuotGui?> GetLuotGuiDangGuiByMaTheAsync(string maThe);
        
        // Lấy danh sách xe đang trong bãi
        Task<List<LuotGui>> GetXeDangTrongBaiAsync();
        
        // Lấy lịch sử lượt gửi
        Task<List<LuotGui>> GetLichSuLuotGuiAsync(DateTime? tuNgay, DateTime? denNgay, int pageSize = 50);
        
        // Tính tiền gửi xe
        Task<decimal> TinhTienGuiXeAsync(LuotGui luotGui);
        
        // Lấy danh sách loại xe
        Task<List<LoaiXe>> GetLoaiXeListAsync();
    }
}
