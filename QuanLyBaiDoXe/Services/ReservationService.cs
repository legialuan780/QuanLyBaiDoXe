using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;
using QuanLyBaiDoXe.Areas.User.ViewModels;

namespace QuanLyBaiDoXe.Services
{
    public class ReservationService : IReservationService
    {
        private readonly QuanLyBaiDoXeContext _context;

        public ReservationService(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, DatCho? DatCho)> TaoDatChoAsync(
            int maKhachHang,
            int maViTri,
            DateTime thoiGianDenDuKien,
            string bienSoXe,
            int maLoaiXe)
        {
            try
            {
                // Kiểm tra khách hàng
                var khachHang = await _context.KhachHangs.FindAsync(maKhachHang);
                if (khachHang == null)
                {
                    return (false, "Khách hàng không tồn tại!", null);
                }

                // Kiểm tra vị trí
                var viTri = await _context.ViTriDos
                    .Include(v => v.MaKhuVucNavigation)
                    .ThenInclude(kv => kv!.MaLoaiXeNavigation)
                    .FirstOrDefaultAsync(v => v.MaViTri == maViTri);

                if (viTri == null)
                {
                    return (false, "Vị trí không tồn tại!", null);
                }

                // Kiểm tra loại xe phù hợp với khu vực
                if (viTri.MaKhuVucNavigation?.MaLoaiXe != null && viTri.MaKhuVucNavigation.MaLoaiXe != maLoaiXe)
                {
                    return (false, $"Vị trí này chỉ dành cho {viTri.MaKhuVucNavigation.MaLoaiXeNavigation?.TenLoaiXe}!", null);
                }

                // Xác định thời gian hết hạn - Hết hạn sau 2 giờ
                DateTime thoiGianHetHan = thoiGianDenDuKien.AddHours(2);

                // Kiểm tra vị trí đã được đặt cho khoảng thời gian này chưa
                var daDatCho = await KiemTraViTriDaDatChoAsync(maViTri, thoiGianDenDuKien, thoiGianHetHan);
                if (daDatCho)
                {
                    return (false, "Vị trí này đã được đặt cho thời gian đó!", null);
                }

                // Trạng thái chờ xử lý
                int trangThai = 0;

                // Tạo đặt chỗ mới
                var datCho = new DatCho
                {
                    MaKhachHang = maKhachHang,
                    MaViTri = maViTri,
                    ThoiGianDat = DateTime.Now,
                    ThoiGianDenDuKien = thoiGianDenDuKien,
                    ThoiGianHetHan = thoiGianHetHan,
                    TrangThaiDatCho = trangThai
                };

                _context.DatChos.Add(datCho);

                // Cập nhật trạng thái vị trí thành "Đã đặt" (2)
                viTri.TrangThai = 2;
                _context.ViTriDos.Update(viTri);

                await _context.SaveChangesAsync();

                return (true, "Đặt chỗ thành công! Vui lòng thanh toán đặt cọc trong 30 phút.", datCho);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        public async Task<List<ReservationViewModel>> GetDatChoByKhachHangAsync(int maKhachHang)
        {
            var datChos = await _context.DatChos
                .Include(dc => dc.MaKhachHangNavigation)
                .Include(dc => dc.MaViTriNavigation)
                .ThenInclude(vt => vt!.MaKhuVucNavigation)
                .ThenInclude(kv => kv!.MaLoaiXeNavigation)
                .Where(dc => dc.MaKhachHang == maKhachHang)
                .OrderByDescending(dc => dc.ThoiGianDat)
                .ToListAsync();

            return datChos.Select(dc => new ReservationViewModel
            {
                MaDatCho = dc.MaDatCho,
                MaKhachHang = dc.MaKhachHang,
                TenKhachHang = dc.MaKhachHangNavigation!.HoTen,
                SoDienThoai = dc.MaKhachHangNavigation.SoDienThoai,
                MaViTri = dc.MaViTri,
                TenViTri = dc.MaViTriNavigation!.TenViTri,
                TenKhuVuc = dc.MaViTriNavigation.MaKhuVucNavigation!.TenKhuVuc,
                ThoiGianDat = dc.ThoiGianDat,
                ThoiGianDenDuKien = dc.ThoiGianDenDuKien,
                ThoiGianHetHan = dc.ThoiGianHetHan,
                TrangThaiDatCho = dc.TrangThaiDatCho,
                TrangThaiText = GetTrangThaiText(dc.TrangThaiDatCho),
                TenLoaiXe = dc.MaViTriNavigation.MaKhuVucNavigation.MaLoaiXeNavigation!.TenLoaiXe,
                BienSoXe = dc.MaKhachHangNavigation.BienSoXeMacDinh,
                MaLoaiXe = dc.MaViTriNavigation.MaKhuVucNavigation.MaLoaiXe
            }).ToList();
        }

        public async Task<List<ReservationViewModel>> GetDatChoChoDuyetAsync()
        {
            var datChos = await _context.DatChos
                .Include(dc => dc.MaKhachHangNavigation)
                .Include(dc => dc.MaViTriNavigation)
                .ThenInclude(vt => vt!.MaKhuVucNavigation)
                .ThenInclude(kv => kv!.MaLoaiXeNavigation)
                .Where(dc => dc.TrangThaiDatCho == 0) // Chờ duyệt
                .OrderBy(dc => dc.ThoiGianDat)
                .Select(dc => new ReservationViewModel
                {
                    MaDatCho = dc.MaDatCho,
                    MaKhachHang = dc.MaKhachHang,
                    TenKhachHang = dc.MaKhachHangNavigation!.HoTen,
                    SoDienThoai = dc.MaKhachHangNavigation.SoDienThoai,
                    MaViTri = dc.MaViTri,
                    TenViTri = dc.MaViTriNavigation!.TenViTri,
                    TenKhuVuc = dc.MaViTriNavigation.MaKhuVucNavigation!.TenKhuVuc,
                    ThoiGianDat = dc.ThoiGianDat,
                    ThoiGianDenDuKien = dc.ThoiGianDenDuKien,
                    ThoiGianHetHan = dc.ThoiGianHetHan,
                    TrangThaiDatCho = dc.TrangThaiDatCho,
                    TrangThaiText = GetTrangThaiText(dc.TrangThaiDatCho),
                    TenLoaiXe = dc.MaViTriNavigation.MaKhuVucNavigation.MaLoaiXeNavigation!.TenLoaiXe
                })
                .ToListAsync();

            return datChos;
        }

        public async Task<List<ReservationViewModel>> GetAllDatChoAsync()
        {
            var datChos = await _context.DatChos
                .Include(dc => dc.MaKhachHangNavigation)
                .Include(dc => dc.MaViTriNavigation)
                .ThenInclude(vt => vt!.MaKhuVucNavigation)
                .ThenInclude(kv => kv!.MaLoaiXeNavigation)
                .OrderByDescending(dc => dc.ThoiGianDat)
                .Select(dc => new ReservationViewModel
                {
                    MaDatCho = dc.MaDatCho,
                    MaKhachHang = dc.MaKhachHang,
                    TenKhachHang = dc.MaKhachHangNavigation!.HoTen,
                    SoDienThoai = dc.MaKhachHangNavigation.SoDienThoai,
                    MaViTri = dc.MaViTri,
                    TenViTri = dc.MaViTriNavigation!.TenViTri,
                    TenKhuVuc = dc.MaViTriNavigation.MaKhuVucNavigation!.TenKhuVuc,
                    ThoiGianDat = dc.ThoiGianDat,
                    ThoiGianDenDuKien = dc.ThoiGianDenDuKien,
                    ThoiGianHetHan = dc.ThoiGianHetHan,
                    TrangThaiDatCho = dc.TrangThaiDatCho,
                    TrangThaiText = GetTrangThaiText(dc.TrangThaiDatCho),
                    TenLoaiXe = dc.MaViTriNavigation.MaKhuVucNavigation.MaLoaiXeNavigation!.TenLoaiXe
                })
                .ToListAsync();

            return datChos;
        }

        public async Task<(bool Success, string Message)> DuyetDatChoAsync(int maDatCho, int maNhanVien)
        {
            try
            {
                var datCho = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == maDatCho);

                if (datCho == null)
                {
                    return (false, "Đặt chỗ không tồn tại!");
                }

                if (datCho.TrangThaiDatCho != 0)
                {
                    return (false, "Đặt chỗ này đã được xử lý!");
                }

                // Kiểm tra có đặt chỗ khác đang active cho vị trí này không
                var coDatChoKhac = await _context.DatChos
                    .Where(dc =>
                        dc.MaViTri == datCho.MaViTri &&
                        dc.MaDatCho != maDatCho && // Không phải đặt chỗ hiện tại
                        (dc.TrangThaiDatCho == 0 || dc.TrangThaiDatCho == 1) && // Chờ duyệt hoặc đã duyệt
                        dc.ThoiGianDenDuKien <= datCho.ThoiGianHetHan && // Có trùng lịch
                        dc.ThoiGianHetHan >= datCho.ThoiGianDenDuKien)
                    .AnyAsync();

                if (coDatChoKhac)
                {
                    return (false, "Vị trí này đã có đặt chỗ khác trong cùng khoảng thời gian!");
                }

                // Duyệt đặt chỗ
                datCho.TrangThaiDatCho = 1; // Đã duyệt

                // Cập nhật trạng thái vị trí
                // Đặt hẹn lịch: Nếu chưa tới ngày, giữ trạng thái Trống (0) để các xe vãng lai vẫn đỗ được
                // Chỉ chuyển sang "Đã đặt" (2) khi gần đến thời gian hẹn (trong vòng 2 giờ)
                if (datCho.MaViTriNavigation != null)
                {
                    var thoiGianConLai = (datCho.ThoiGianDenDuKien - DateTime.Now)?.TotalHours ?? 0;
                    
                    if (thoiGianConLai <= 2) // Nếu còn <= 2 giờ nữa là đến giờ hẹn
                    {
                        datCho.MaViTriNavigation.TrangThai = 2; // Đã đặt
                    }
                    // Nếu còn nhiều thời gian, giữ trạng thái hiện tại (0 hoặc 1)
                    // để các xe vãng lai vẫn có thể đỗ, tự động giải phóng khi đến giờ hẹn
                }

                await _context.SaveChangesAsync();

                return (true, "Duyệt đặt chỗ thành công!");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> TuChoiDatChoAsync(int maDatCho, string lyDo)
        {
            try
            {
                var datCho = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == maDatCho);

                if (datCho == null)
                {
                    return (false, "Đặt chỗ không tồn tại!");
                }

                if (datCho.TrangThaiDatCho != 0)
                {
                    return (false, "Đặt chỗ này đã được xử lý!");
                }

                // Từ chối đặt chỗ
                datCho.TrangThaiDatCho = 3; // Từ chối

                // Giải phóng vị trí nếu đã đặt
                if (datCho.MaViTriNavigation != null && datCho.MaViTriNavigation.TrangThai == 2)
                {
                    datCho.MaViTriNavigation.TrangThai = 0; // Trống
                }

                await _context.SaveChangesAsync();

                return (true, "Từ chối đặt chỗ thành công!");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> HuyDatChoAsync(int maDatCho, int maKhachHang)
        {
            try
            {
                var datCho = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == maDatCho && dc.MaKhachHang == maKhachHang);

                if (datCho == null)
                {
                    return (false, "Đặt chỗ không tồn tại hoặc bạn không có quyền hủy!");
                }

                if (datCho.TrangThaiDatCho == 2)
                {
                    return (false, "Đặt chỗ đã hoàn thành, không thể hủy!");
                }

                if (datCho.TrangThaiDatCho == 4)
                {
                    return (false, "Đặt chỗ đã bị hủy!");
                }

                // Hủy đặt chỗ
                datCho.TrangThaiDatCho = 4; // Đã hủy

                // Giải phóng vị trí
                if (datCho.MaViTriNavigation != null && datCho.MaViTriNavigation.TrangThai == 2)
                {
                    datCho.MaViTriNavigation.TrangThai = 0; // Trống
                }

                await _context.SaveChangesAsync();

                return (true, "Hủy đặt chỗ thành công!");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<bool> KiemTraViTriDaDatChoAsync(int maViTri, DateTime? thoiGianDenDuKien = null, DateTime? thoiGianHetHan = null)
        {
            var query = _context.DatChos.Where(dc =>
                dc.MaViTri == maViTri &&
                dc.TrangThaiDatCho != 3 && // Không bị từ chối
                dc.TrangThaiDatCho != 4 && // Không bị hủy
                dc.TrangThaiDatCho != 5);  // Không hết hạn

            if (thoiGianDenDuKien.HasValue && thoiGianHetHan.HasValue)
            {
                // Kiểm tra trùng lịch: 2 khoảng thời gian overlap nếu:
                // (Start1 <= End2) AND (End1 >= Start2)
                query = query.Where(dc =>
                    dc.ThoiGianDenDuKien <= thoiGianHetHan &&
                    dc.ThoiGianHetHan >= thoiGianDenDuKien);
            }

            return await query.AnyAsync();
        }

        public async Task<DatCho?> GetDatChoByIdAsync(int maDatCho)
        {
            return await _context.DatChos
                .Include(dc => dc.MaKhachHangNavigation)
                .Include(dc => dc.MaViTriNavigation)
                .ThenInclude(vt => vt!.MaKhuVucNavigation)
                .FirstOrDefaultAsync(dc => dc.MaDatCho == maDatCho);
        }

        public async Task XuLyDatChoHetHanAsync()
        {
            var now = DateTime.Now;

            // Lấy các đặt chỗ đã hết hạn
            var datChosHetHan = await _context.DatChos
                .Include(dc => dc.MaViTriNavigation)
                .Where(dc =>
                    dc.ThoiGianHetHan <= now &&
                    dc.TrangThaiDatCho != 2 && // Chưa hoàn thành
                    dc.TrangThaiDatCho != 3 && // Chưa từ chối
                    dc.TrangThaiDatCho != 4 && // Chưa hủy
                    dc.TrangThaiDatCho != 5)   // Chưa hết hạn
                .ToListAsync();

            foreach (var datCho in datChosHetHan)
            {
                datCho.TrangThaiDatCho = 5; // Hết hạn

                // Giải phóng vị trí
                if (datCho.MaViTriNavigation != null && datCho.MaViTriNavigation.TrangThai == 2)
                {
                    datCho.MaViTriNavigation.TrangThai = 0; // Trống
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ViTriDtoReservation>> GetViTriTrongTheLoaiXeAsync(int maLoaiXe, DateTime? thoiGianDenDuKien = null)
        {
            var viTriTrong = await _context.ViTriDos
                .Include(vt => vt.MaKhuVucNavigation)
                .Where(vt =>
                    vt.TrangThai == 0 && // Vị trí trống
                    (vt.MaKhuVucNavigation!.MaLoaiXe == null || vt.MaKhuVucNavigation.MaLoaiXe == maLoaiXe))
                .Select(vt => new ViTriDtoReservation
                {
                    MaViTri = vt.MaViTri,
                    TenViTri = vt.TenViTri,
                    TrangThai = vt.TrangThai ?? 0,
                    TrangThaiText = vt.TrangThai == 0 ? "Trống" : vt.TrangThai == 1 ? "Đã đỗ" : "Đã đặt",
                    MaKhuVuc = vt.MaKhuVuc,
                    TenKhuVuc = vt.MaKhuVucNavigation!.TenKhuVuc
                })
                .ToListAsync();

            // Lọc ra các vị trí không có đặt chỗ trùng thời gian (nếu có)
            if (thoiGianDenDuKien.HasValue)
            {
                var viTriDaDat = await _context.DatChos
                    .Where(dc =>
                        dc.ThoiGianDenDuKien <= thoiGianDenDuKien &&
                        dc.ThoiGianHetHan >= thoiGianDenDuKien &&
                        dc.TrangThaiDatCho != 3 && // Không bị từ chối
                        dc.TrangThaiDatCho != 4 && // Không bị hủy
                        dc.TrangThaiDatCho != 5)   // Không hết hạn
                    .Select(dc => dc.MaViTri)
                    .ToListAsync();

                viTriTrong = viTriTrong.Where(vt => !viTriDaDat.Contains(vt.MaViTri)).ToList();
            }

            return viTriTrong;
        }

        public async Task<(bool Success, string Message)> SetupViTriDatTruocAsync(int maDatCho)
        {
            try
            {
                var datCho = await _context.DatChos
                    .Include(dc => dc.MaViTriNavigation)
                    .FirstOrDefaultAsync(dc => dc.MaDatCho == maDatCho);

                if (datCho == null)
                {
                    return (false, "Đặt chỗ không tồn tại!");
                }

                if (datCho.TrangThaiDatCho != 1)
                {
                    return (false, "Đặt chỗ chưa được duyệt!");
                }

                // Kiểm tra xem có xe đang đỗ ở vị trí này không
                var xeDangDo = await _context.LuotGuis
                    .Where(lg => lg.MaViTri == datCho.MaViTri && lg.TrangThai == 0)
                    .FirstOrDefaultAsync();

                if (xeDangDo != null)
                {
                    // Thông báo: Xe này phải ra trước 23h hôm nay
                    return (false, $"Vị trí này có xe đang đỗ. Xe phải ra trước 23h hôm nay để ngày mai trống cho khách đặt!");
                }

                // Cập nhật vị trí thành "Đã đặt"
                if (datCho.MaViTriNavigation != null)
                {
                    datCho.MaViTriNavigation.TrangThai = 2; // Đã đặt
                }

                await _context.SaveChangesAsync();

                return (true, "Thiết lập vị trí đặt trước thành công!");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        private static string GetTrangThaiText(int? trangThai)
        {
            return trangThai switch
            {
                0 => "Chờ xử lý",
                1 => "Đã duyệt",
                2 => "Hoàn thành",
                3 => "Từ chối",
                4 => "Đã hủy",
                5 => "Hết hạn",
                _ => "Không xác định"
            };
        }
    }
}
