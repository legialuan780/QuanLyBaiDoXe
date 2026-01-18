using Microsoft.AspNetCore.Mvc;
using QuanLyBaiDoXe.Areas.Admin.ViewModels;
using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.Services;

namespace QuanLyBaiDoXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CardController : Controller
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }

        public async Task<IActionResult> Index(string? keyword, int? loaiThe, int? loaiXe, int? trangThai)
        {
            var cards = await _cardService.SearchCardsAsync(keyword, loaiThe, loaiXe, trangThai);
            var statistics = await _cardService.GetCardStatisticsAsync();
            var vehicleTypes = await _cardService.GetVehicleTypesAsync();

            // Kiểm tra thẻ đang sử dụng
            var cardDtos = new List<CardDto>();
            foreach (var card in cards)
            {
                cardDtos.Add(new CardDto
                {
                    MaThe = card.MaThe,
                    MaLoaiXe = card.MaLoaiXe,
                    TenLoaiXe = card.MaLoaiXeNavigation?.TenLoaiXe,
                    LoaiThe = card.LoaiThe,
                    TrangThai = card.TrangThai,
                    DangSuDung = await _cardService.IsCardInUseAsync(card.MaThe)
                });
            }

            var viewModel = new CardViewModel
            {
                Cards = cards,
                VehicleTypes = vehicleTypes,
                Statistics = new CardStatisticsViewModel
                {
                    TotalCards = statistics.TotalCards,
                    ActiveCards = statistics.ActiveCards,
                    LockedCards = statistics.LockedCards,
                    MonthlyCards = statistics.MonthlyCards,
                    SingleCards = statistics.SingleCards,
                    CardsInUse = statistics.CardsInUse
                },
                SearchKeyword = keyword,
                FilterLoaiThe = loaiThe,
                FilterLoaiXe = loaiXe,
                FilterTrangThai = trangThai
            };

            ViewBag.CardDtos = cardDtos;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetCard(string maThe)
        {
            var card = await _cardService.GetCardByIdAsync(maThe);
            if (card == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thẻ!" });
            }

            var isInUse = await _cardService.IsCardInUseAsync(maThe);

            return Json(new
            {
                success = true,
                card = new CardDto
                {
                    MaThe = card.MaThe,
                    MaLoaiXe = card.MaLoaiXe,
                    TenLoaiXe = card.MaLoaiXeNavigation?.TenLoaiXe,
                    LoaiThe = card.LoaiThe,
                    TrangThai = card.TrangThai,
                    DangSuDung = isInUse
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCardRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var theXe = new TheXe
                {
                    MaThe = request.MaThe.Trim().ToUpper(),
                    MaLoaiXe = request.MaLoaiXe,
                    LoaiThe = request.LoaiThe,
                    TrangThai = request.TrangThai
                };

                await _cardService.CreateCardAsync(theXe);

                return Json(new { success = true, message = "Thêm thẻ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMultiple([FromBody] CreateMultipleCardsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var cards = new List<TheXe>();
                for (int i = 0; i < request.Quantity; i++)
                {
                    var cardNumber = request.StartNumber + i;
                    var maThe = $"{request.Prefix.Trim().ToUpper()}{cardNumber:D4}";

                    cards.Add(new TheXe
                    {
                        MaThe = maThe,
                        MaLoaiXe = request.MaLoaiXe,
                        LoaiThe = request.LoaiThe,
                        TrangThai = 1
                    });
                }

                var createdCards = await _cardService.CreateMultipleCardsAsync(cards);

                return Json(new
                {
                    success = true,
                    message = $"Đã thêm {createdCards.Count}/{request.Quantity} thẻ thành công!",
                    createdCount = createdCards.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateCardRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var theXe = new TheXe
                {
                    MaThe = request.MaThe,
                    MaLoaiXe = request.MaLoaiXe,
                    LoaiThe = request.LoaiThe,
                    TrangThai = request.TrangThai
                };

                var result = await _cardService.UpdateCardAsync(theXe);
                if (result == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thẻ!" });
                }

                return Json(new { success = true, message = "Cập nhật thẻ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Lock(string maThe)
        {
            try
            {
                var result = await _cardService.LockCardAsync(maThe);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy thẻ!" });
                }

                return Json(new { success = true, message = "Đã khóa thẻ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(string maThe)
        {
            try
            {
                var result = await _cardService.UnlockCardAsync(maThe);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy thẻ!" });
                }

                return Json(new { success = true, message = "Đã mở khóa thẻ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string maThe)
        {
            try
            {
                var result = await _cardService.DeleteCardAsync(maThe);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy thẻ!" });
                }

                return Json(new { success = true, message = "Đã xóa thẻ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var statistics = await _cardService.GetCardStatisticsAsync();
            return Json(new
            {
                totalCards = statistics.TotalCards,
                activeCards = statistics.ActiveCards,
                lockedCards = statistics.LockedCards,
                monthlyCards = statistics.MonthlyCards,
                singleCards = statistics.SingleCards,
                cardsInUse = statistics.CardsInUse
            });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? keyword, int? loaiThe, int? loaiXe, int? trangThai)
        {
            var cards = await _cardService.SearchCardsAsync(keyword, loaiThe, loaiXe, trangThai);
            
            var cardDtos = new List<CardDto>();
            foreach (var card in cards)
            {
                cardDtos.Add(new CardDto
                {
                    MaThe = card.MaThe,
                    MaLoaiXe = card.MaLoaiXe,
                    TenLoaiXe = card.MaLoaiXeNavigation?.TenLoaiXe,
                    LoaiThe = card.LoaiThe,
                    TrangThai = card.TrangThai,
                    DangSuDung = await _cardService.IsCardInUseAsync(card.MaThe)
                });
            }

            return Json(cardDtos);
        }
    }
}
