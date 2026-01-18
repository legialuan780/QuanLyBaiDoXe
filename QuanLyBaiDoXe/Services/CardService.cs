using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Services
{
    public class CardService : ICardService
    {
        private readonly QuanLyBaiDoXeContext _context;

        public CardService(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<List<TheXe>> GetAllCardsAsync()
        {
            return await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .OrderByDescending(t => t.MaThe)
                .ToListAsync();
        }

        public async Task<TheXe?> GetCardByIdAsync(string maThe)
        {
            return await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .FirstOrDefaultAsync(t => t.MaThe == maThe);
        }

        public async Task<List<TheXe>> GetCardsByStatusAsync(int trangThai)
        {
            return await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .Where(t => t.TrangThai == trangThai)
                .OrderByDescending(t => t.MaThe)
                .ToListAsync();
        }

        public async Task<List<TheXe>> GetCardsByTypeAsync(int loaiThe)
        {
            return await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .Where(t => t.LoaiThe == loaiThe)
                .OrderByDescending(t => t.MaThe)
                .ToListAsync();
        }

        public async Task<List<TheXe>> GetCardsByVehicleTypeAsync(int maLoaiXe)
        {
            return await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .Where(t => t.MaLoaiXe == maLoaiXe)
                .OrderByDescending(t => t.MaThe)
                .ToListAsync();
        }

        public async Task<List<TheXe>> SearchCardsAsync(string? keyword, int? loaiThe, int? maLoaiXe, int? trangThai)
        {
            var query = _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => t.MaThe.Contains(keyword));
            }

            if (loaiThe.HasValue)
            {
                query = query.Where(t => t.LoaiThe == loaiThe);
            }

            if (maLoaiXe.HasValue)
            {
                query = query.Where(t => t.MaLoaiXe == maLoaiXe);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(t => t.TrangThai == trangThai);
            }

            return await query.OrderByDescending(t => t.MaThe).ToListAsync();
        }

        public async Task<TheXe> CreateCardAsync(TheXe theXe)
        {
            // Kiểm tra mã thẻ đã tồn tại chưa
            var existingCard = await _context.TheXes.FindAsync(theXe.MaThe);
            if (existingCard != null)
            {
                throw new Exception($"Mã thẻ '{theXe.MaThe}' đã tồn tại trong hệ thống!");
            }

            _context.TheXes.Add(theXe);
            await _context.SaveChangesAsync();
            return theXe;
        }

        public async Task<List<TheXe>> CreateMultipleCardsAsync(List<TheXe> theXes)
        {
            var createdCards = new List<TheXe>();
            
            foreach (var theXe in theXes)
            {
                var existingCard = await _context.TheXes.FindAsync(theXe.MaThe);
                if (existingCard == null)
                {
                    _context.TheXes.Add(theXe);
                    createdCards.Add(theXe);
                }
            }

            await _context.SaveChangesAsync();
            return createdCards;
        }

        public async Task<TheXe?> UpdateCardAsync(TheXe theXe)
        {
            var existingCard = await _context.TheXes.FindAsync(theXe.MaThe);
            if (existingCard == null)
            {
                return null;
            }

            existingCard.MaLoaiXe = theXe.MaLoaiXe;
            existingCard.LoaiThe = theXe.LoaiThe;
            existingCard.TrangThai = theXe.TrangThai;

            await _context.SaveChangesAsync();
            return existingCard;
        }

        public async Task<bool> LockCardAsync(string maThe)
        {
            var card = await _context.TheXes.FindAsync(maThe);
            if (card == null)
            {
                return false;
            }

            // Kiểm tra thẻ có đang gửi không
            if (await IsCardInUseAsync(maThe))
            {
                throw new Exception("Không thể khóa thẻ đang được sử dụng!");
            }

            card.TrangThai = 0; // 0 = Khóa
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockCardAsync(string maThe)
        {
            var card = await _context.TheXes.FindAsync(maThe);
            if (card == null)
            {
                return false;
            }

            card.TrangThai = 1; // 1 = Hoạt động
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCardAsync(string maThe)
        {
            var card = await _context.TheXes.FindAsync(maThe);
            if (card == null)
            {
                return false;
            }

            // Kiểm tra thẻ có đang gửi không
            if (await IsCardInUseAsync(maThe))
            {
                throw new Exception("Không thể xóa thẻ đang được sử dụng!");
            }

            // Kiểm tra có lượt gửi nào liên quan không
            var hasHistory = await _context.LuotGuis.AnyAsync(l => l.MaThe == maThe);
            if (hasHistory)
            {
                throw new Exception("Không thể xóa thẻ đã có lịch sử gửi xe!");
            }

            _context.TheXes.Remove(card);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCardInUseAsync(string maThe)
        {
            return await _context.LuotGuis
                .AnyAsync(l => l.MaThe == maThe && l.TrangThai == 0);
        }

        public async Task<List<LoaiXe>> GetVehicleTypesAsync()
        {
            return await _context.LoaiXes.ToListAsync();
        }

        public async Task<CardStatistics> GetCardStatisticsAsync()
        {
            var allCards = await _context.TheXes.ToListAsync();
            var cardsInUse = await _context.LuotGuis
                .Where(l => l.TrangThai == 0)
                .Select(l => l.MaThe)
                .Distinct()
                .CountAsync();

            return new CardStatistics
            {
                TotalCards = allCards.Count,
                ActiveCards = allCards.Count(c => c.TrangThai == 1),
                LockedCards = allCards.Count(c => c.TrangThai == 0),
                MonthlyCards = allCards.Count(c => c.LoaiThe == 1),
                SingleCards = allCards.Count(c => c.LoaiThe == 0),
                CardsInUse = cardsInUse
            };
        }
    }
}
