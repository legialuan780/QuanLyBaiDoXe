using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Services
{
    public class VehicleEntryService : IVehicleEntryService
    {
        private readonly QuanLyBaiDoXeContext _context;

        public VehicleEntryService(QuanLyBaiDoXeContext context)
        {
            _context = context;
        }

        public async Task<TheXe?> GetTheXeByMaTheAsync(string maThe)
        {
            return await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .FirstOrDefaultAsync(t => t.MaThe == maThe);
        }

        public async Task<List<TheXe>> GetActiveTheXeListAsync()
        {
            return await _context.TheXes
                .Include(t => t.MaLoaiXeNavigation)
                .Where(t => t.TrangThai == 1) // Thẻ đang hoạt động
                .ToListAsync();
        }

        public async Task<List<ViTriDo>> GetAvailableViTriDoAsync()
        {
            return await _context.ViTriDos
                .Include(v => v.MaKhuVucNavigation)
                .Where(v => v.TrangThai == 0) // Vị trí trống
                .ToListAsync();
        }

        public async Task<LuotGui> XuLyXeVaoAsync(string maThe, string bienSoVao, string? hinhAnhVao, int? maKhuVuc)
        {
            // Kiểm tra thẻ có tồn tại và đang hoạt động
            var theXe = await GetTheXeByMaTheAsync(maThe);
            if (theXe == null || theXe.TrangThai != 1)
            {
                throw new Exception("Thẻ xe không hợp lệ hoặc đã bị khóa!");
            }

            // Kiểm tra thẻ đã có lượt gửi chưa ra
            var luotGuiCu = await GetLuotGuiDangGuiByMaTheAsync(maThe);
            if (luotGuiCu != null)
            {
                throw new Exception("Thẻ xe này đang có lượt gửi chưa lấy xe ra!");
            }

            // Kiểm tra biển số xe đã trong bãi (tránh duplicate)
            var bienSoChuanHoa = bienSoVao?.Trim().ToUpper();
            if (!string.IsNullOrEmpty(bienSoChuanHoa))
            {
                var xeTrungBienSo = await _context.LuotGuis
                    .Where(l => l.TrangThai == 0 && l.BienSoVao != null)
                    .FirstOrDefaultAsync(l => l.BienSoVao!.Trim().ToUpper() == bienSoChuanHoa);
                
                if (xeTrungBienSo != null)
                {
                    throw new Exception($"Biển số {bienSoVao} đã có trong bãi với thẻ {xeTrungBienSo.MaThe}! Vui lòng kiểm tra lại.");
                }
            }

            // Kiểm tra vé tháng - nếu có vé tháng thì biển số PHẢI khớp mới cho vào
            var veThangInfo = await KiemTraVeThangChiTietAsync(maThe, bienSoVao);
            if (veThangInfo.CoVeThang && !veThangInfo.DaHetHan && !veThangInfo.KhongCoKhachHang)
            {
                // Có vé tháng còn hiệu lực và có khách hàng -> kiểm tra biển số
                if (veThangInfo.BienSoKhongKhop)
                {
                    throw new Exception($"Biển số {bienSoVao} không khớp với biển số đăng ký vé tháng ({veThangInfo.BienSoMacDinh})! Không thể vào bãi.");
                }
            }

            // Kiểm tra khu vực bắt buộc
            if (!maKhuVuc.HasValue)
            {
                throw new Exception("Vui lòng chọn khu vực đỗ xe!");
            }

            // Kiểm tra khu vực còn chỗ trống không
            var khuVuc = await _context.KhuVucs
                .Include(kv => kv.MaLoaiXeNavigation)
                .FirstOrDefaultAsync(kv => kv.MaKhuVuc == maKhuVuc.Value);

            if (khuVuc == null)
            {
                throw new Exception("Khu vực đỗ không tồn tại!");
            }

            var soChoTrong = await _context.ViTriDos
                .CountAsync(v => v.MaKhuVuc == maKhuVuc.Value && v.TrangThai == 0);

            if (soChoTrong == 0)
            {
                var tenLoaiXe = khuVuc.MaLoaiXeNavigation?.TenLoaiXe ?? "xe";
                throw new Exception($"Khu vực {khuVuc.TenKhuVuc} đã hết chỗ cho {tenLoaiXe}! Vui lòng chọn khu vực khác.");
            }

            // Tự động gán vị trí trống trong khu vực được chọn
            int? maViTriDuocGan = null;
            // Tìm vị trí trống đầu tiên trong khu vực
            var viTriTrong = await _context.ViTriDos
                .Where(v => v.MaKhuVuc == maKhuVuc.Value && v.TrangThai == 0)
                .OrderBy(v => v.TenViTri)
                .FirstOrDefaultAsync();

            if (viTriTrong != null)
            {
                viTriTrong.TrangThai = 1; // Đánh dấu đã sử dụng
                maViTriDuocGan = viTriTrong.MaViTri;
            }

            // Tạo lượt gửi mới
            var luotGui = new LuotGui
            {
                MaThe = maThe,
                ThoiGianVao = DateTime.Now,
                BienSoVao = bienSoVao,
                HinhAnhVao = hinhAnhVao,
                MaViTri = maViTriDuocGan,
                TrangThai = 0 // Đang gửi
            };

            _context.LuotGuis.Add(luotGui);
            await _context.SaveChangesAsync();

            return luotGui;
        }

        public async Task<LuotGui?> XuLyXeRaAsync(string maThe, string bienSoRa, string? hinhAnhRa)
        {
            var luotGui = await GetLuotGuiDangGuiByMaTheAsync(maThe);
            if (luotGui == null)
            {
                throw new Exception("Không tìm thấy lượt gửi của thẻ này!");
            }

            luotGui.ThoiGianRa = DateTime.Now;
            luotGui.BienSoRa = bienSoRa;
            luotGui.HinhAnhRa = hinhAnhRa;
            luotGui.TrangThai = 1; // Đã lấy xe

            // Tính tiền
            luotGui.TongTien = await TinhTienGuiXeAsync(luotGui);

            // Giải phóng vị trí đỗ
            if (luotGui.MaViTri.HasValue)
            {
                var viTri = await _context.ViTriDos.FindAsync(luotGui.MaViTri.Value);
                if (viTri != null)
                {
                    viTri.TrangThai = 0; // Đánh dấu trống
                }
            }

            await _context.SaveChangesAsync();

            return luotGui;
        }

        public async Task<LuotGui?> GetLuotGuiDangGuiByMaTheAsync(string maThe)
        {
            return await _context.LuotGuis
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Include(l => l.MaViTriNavigation)
                    .ThenInclude(v => v!.MaKhuVucNavigation)
                .FirstOrDefaultAsync(l => l.MaThe == maThe && l.TrangThai == 0);
        }

        public async Task<List<LuotGui>> GetXeDangTrongBaiAsync()
        {
            return await _context.LuotGuis
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Include(l => l.MaViTriNavigation)
                    .ThenInclude(v => v!.MaKhuVucNavigation)
                .Where(l => l.TrangThai == 0)
                .OrderByDescending(l => l.ThoiGianVao)
                .ToListAsync();
        }

        public async Task<List<LuotGui>> GetLichSuLuotGuiAsync(DateTime? tuNgay, DateTime? denNgay, int pageSize = 50)
        {
            var query = _context.LuotGuis
                .Include(l => l.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .Include(l => l.MaViTriNavigation)
                .AsQueryable();

            if (tuNgay.HasValue)
            {
                query = query.Where(l => l.ThoiGianVao >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                query = query.Where(l => l.ThoiGianVao <= denNgay.Value);
            }

            return await query
                .OrderByDescending(l => l.ThoiGianVao)
                .Take(pageSize)
                        .ToListAsync();
                }

                /// <summary>
                /// Tính tiền gửi xe preview - dùng để hiển thị trước khi xác nhận thanh toán
                /// </summary>
                public async Task<decimal> TinhTienGuiXePreviewAsync(LuotGui luotGui, DateTime thoiGianRa)
                {
                    // Tạo một bản copy để tính toán mà không ảnh hưởng entity gốc
                    var tempLuotGui = new LuotGui
                    {
                        MaThe = luotGui.MaThe,
                        BienSoVao = luotGui.BienSoVao, // Thêm biển số để kiểm tra vé tháng
                        ThoiGianVao = luotGui.ThoiGianVao,
                        ThoiGianRa = thoiGianRa,
                        MaViTri = luotGui.MaViTri
                    };

                    return await TinhTienGuiXeAsync(tempLuotGui);
                }

                public async Task<decimal> TinhTienGuiXeAsync(LuotGui luotGui)
                {
                    if (luotGui.ThoiGianRa == null)
                    {
                        return 0;
                    }

                    // Kiểm tra thẻ có vé tháng còn hiệu lực không (bao gồm kiểm tra biển số)
                    var veThangInfo = await KiemTraVeThangChiTietAsync(luotGui.MaThe!, luotGui.BienSoVao);
                    if (veThangInfo.HopLe)
                    {
                        return 0;
                    }


                    var thoiGianGui = luotGui.ThoiGianRa.Value - luotGui.ThoiGianVao;
                    var soPhut = (int)Math.Ceiling(thoiGianGui.TotalMinutes);
                    if (soPhut <= 0) soPhut = 1;

                    var theXe = await GetTheXeByMaTheAsync(luotGui.MaThe!);
                    if (theXe?.MaLoaiXe == null)
                    {
                        return 0;
                    }

                    var cauHinh = await GetCauHinhGiaPhùHopAsync(theXe.MaLoaiXe.Value, luotGui.ThoiGianVao);

                    if (cauHinh == null || !cauHinh.ChiTietGia.Any())
                    {
                        // Giá mặc định: 5000đ/giờ
                        var soGio = (int)Math.Ceiling(thoiGianGui.TotalHours);
                        return soGio * 5000m;
                    }

                    /*
                     * Giải thích cách tính:
                     * - IsLuyTien = false (0): Block cố định, ví dụ: 60 phút đầu = 5000đ (gửi 30p vẫn tính 5000đ)
                     * - IsLuyTien = true (1): Block lũy tiến, ví dụ: mỗi 60 phút tiếp = 3000đ
                     * 
                     * Ví dụ xe máy gửi 150 phút:
                     * - Block 1: 60 phút đầu = 5000đ (IsLuyTien = false) -> phutDaTinh = 60
                     * - Block 2: Mỗi 60 phút tiếp = 3000đ (IsLuyTien = true)
                     *   + Còn 90 phút, cần 2 block = 2 x 3000 = 6000đ
                     * - Tổng: 5000 + 6000 = 11000đ
                     */
                    decimal tongTien = 0;
                    var chiTietGiaList = cauHinh.ChiTietGia.OrderBy(c => c.ThuTuBlock).ToList();
                    int phutDaTinh = 0;

                    foreach (var chiTiet in chiTietGiaList)
                    {
                        if (phutDaTinh >= soPhut) break;

                        var soPhutBlock = chiTiet.SoPhutCuaBlock ?? 60;
                        var giaTienBlock = chiTiet.GiaTien ?? 0;

                        if (chiTiet.IsLuyTien == true)
                        {
                            // Lũy tiến: Tính số block còn lại với giá cố định mỗi block
                            var soPhutCon = soPhut - phutDaTinh;
                            var soBlockCanTinh = (int)Math.Ceiling((double)soPhutCon / soPhutBlock);
                            tongTien += soBlockCanTinh * giaTienBlock;
                            phutDaTinh = soPhut; // Đã tính hết
                        }
                        else
                        {
                            // Cố định: Tính đủ giá block (không theo tỷ lệ)
                            if (soPhut - phutDaTinh > 0)
                            {
                                tongTien += giaTienBlock;
                                phutDaTinh += soPhutBlock;
                            }
                        }
                    }

                    // Nếu vẫn còn thời gian chưa tính, dùng block cuối
                    if (phutDaTinh < soPhut && chiTietGiaList.Any())
                    {
                        var lastBlock = chiTietGiaList.Last();
                        var soPhutCon = soPhut - phutDaTinh;
                        var soBlockCon = (int)Math.Ceiling((double)soPhutCon / (lastBlock.SoPhutCuaBlock ?? 60));
                        tongTien += soBlockCon * (lastBlock.GiaTien ?? 0);
                    }

                    // Đảm bảo có giá tối thiểu
                    if (tongTien <= 0)
                    {
                        var soGio = (int)Math.Ceiling(thoiGianGui.TotalHours);
                        tongTien = soGio * 5000m;
                    }

                    return Math.Round(tongTien, 0);
                }

        /// <summary>
        /// Kiểm tra thẻ có vé tháng còn hiệu lực không
        /// </summary>
        public async Task<bool> KiemTraVeThangHopLeAsync(string maThe)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var veThang = await _context.TheThangs
                .Include(v => v.MaKhachHangNavigation)
                .FirstOrDefaultAsync(v => v.MaThe == maThe 
                    && v.TrangThai == true 
                    && v.NgayBatDau <= today 
                    && v.NgayHetHan >= today);

            // Vé tháng phải liên kết với khách hàng mới được miễn phí
            return veThang != null && veThang.MaKhachHang != null;
        }

        /// <summary>
        /// Kiểm tra vé tháng với biển số xe
        /// </summary>
        public async Task<VeThangInfoResult> KiemTraVeThangChiTietAsync(string maThe, string? bienSo = null)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var veThang = await _context.TheThangs
                .Include(v => v.MaKhachHangNavigation)
                .Include(v => v.MaTheNavigation)
                    .ThenInclude(t => t!.MaLoaiXeNavigation)
                .FirstOrDefaultAsync(v => v.MaThe == maThe && v.TrangThai == true);

            if (veThang == null)
            {
                return new VeThangInfoResult { CoVeThang = false };
            }

            var result = new VeThangInfoResult
            {
                CoVeThang = true,
                NgayBatDau = veThang.NgayBatDau,
                NgayHetHan = veThang.NgayHetHan,
                TenKhachHang = veThang.MaKhachHangNavigation?.HoTen,
                BienSoMacDinh = veThang.MaKhachHangNavigation?.BienSoXeMacDinh
            };

            // Kiểm tra hết hạn
            if (veThang.NgayHetHan < today)
            {
                result.DaHetHan = true;
                result.ThongBao = $"Vé tháng đã hết hạn từ {veThang.NgayHetHan:dd/MM/yyyy}! Vui lòng gia hạn.";
                return result;
            }

            // Kiểm tra sắp hết hạn (còn 7 ngày)
            var ngayConLai = veThang.NgayHetHan!.Value.DayNumber - today.DayNumber;
            if (ngayConLai <= 7)
            {
                result.SapHetHan = true;
                result.SoNgayConLai = ngayConLai;
                result.ThongBao = $"Vé tháng sẽ hết hạn sau {ngayConLai} ngày ({veThang.NgayHetHan:dd/MM/yyyy}). Vui lòng gia hạn sớm!";
            }

            // Kiểm tra khách hàng
            if (veThang.MaKhachHang == null)
            {
                result.KhongCoKhachHang = true;
                result.ThongBao = "Vé tháng chưa liên kết khách hàng! Không được miễn phí.";
                return result;
            }

            // Kiểm tra biển số (nếu có)
            if (!string.IsNullOrEmpty(bienSo) && !string.IsNullOrEmpty(result.BienSoMacDinh))
            {
                var bienSoNhap = bienSo.Trim().ToUpper().Replace(" ", "").Replace("-", "").Replace(".", "");
                var bienSoMD = result.BienSoMacDinh.Trim().ToUpper().Replace(" ", "").Replace("-", "").Replace(".", "");
                
                if (bienSoNhap != bienSoMD)
                {
                    result.BienSoKhongKhop = true;
                    result.ThongBao = $"Biển số {bienSo} không khớp với biển số đăng ký vé tháng ({result.BienSoMacDinh})! Sẽ tính phí bình thường.";
                    return result;
                }
            }

            result.HopLe = !result.DaHetHan && !result.KhongCoKhachHang && !result.BienSoKhongKhop;
            if (result.HopLe && string.IsNullOrEmpty(result.ThongBao))
            {
                result.ThongBao = "Vé tháng hợp lệ - Miễn phí gửi xe.";
            }

            return result;
        }

        /// <summary>
        /// Lấy cấu hình giá phù hợp theo loại xe và thời gian
        /// Ưu tiên: 1. Cấu hình có IsUuTien = true trong khung giờ
        ///          2. Cấu hình trong khung giờ
        ///          3. Cấu hình mặc định của loại xe
        /// </summary>
        private async Task<CauHinhGium?> GetCauHinhGiaPhùHopAsync(int maLoaiXe, DateTime thoiGianVao)
        {
            var gioVao = TimeOnly.FromDateTime(thoiGianVao);

            // Tìm cấu hình ưu tiên trong khung giờ
            var cauHinhUuTien = await _context.CauHinhGia
                .Include(c => c.ChiTietGia)
                .Where(c => c.MaLoaiXe == maLoaiXe 
                    && c.IsUuTien == true
                    && c.GioBatDau != null 
                    && c.GioKetThuc != null
                    && c.GioBatDau <= gioVao 
                    && c.GioKetThuc >= gioVao)
                .FirstOrDefaultAsync();

            if (cauHinhUuTien != null)
            {
                return cauHinhUuTien;
            }

            // Tìm cấu hình trong khung giờ (không ưu tiên)
            var cauHinhTrongGio = await _context.CauHinhGia
                .Include(c => c.ChiTietGia)
                .Where(c => c.MaLoaiXe == maLoaiXe
                    && c.GioBatDau != null
                    && c.GioKetThuc != null
                    && c.GioBatDau <= gioVao
                    && c.GioKetThuc >= gioVao)
                .FirstOrDefaultAsync();

            if (cauHinhTrongGio != null)
            {
                return cauHinhTrongGio;
            }

            // Tìm cấu hình mặc định (không có khung giờ hoặc ưu tiên nhất)
            var cauHinhMacDinh = await _context.CauHinhGia
                .Include(c => c.ChiTietGia)
                .Where(c => c.MaLoaiXe == maLoaiXe)
                .OrderByDescending(c => c.IsUuTien)
                .FirstOrDefaultAsync();

            return cauHinhMacDinh;
        }

        public async Task<List<LoaiXe>> GetLoaiXeListAsync()
        {
            return await _context.LoaiXes.ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách khu vực theo loại xe
        /// </summary>
        public async Task<List<KhuVuc>> GetKhuVucTheoLoaiXeAsync(int? maLoaiXe)
        {
            if (maLoaiXe == null)
            {
                return await _context.KhuVucs
                    .Include(k => k.ViTriDos)
                    .Where(k => k.ViTriDos.Any(v => v.TrangThai == 0))
                    .ToListAsync();
            }

            return await _context.KhuVucs
                .Include(k => k.ViTriDos)
                .Where(k => k.MaLoaiXe == maLoaiXe && k.ViTriDos.Any(v => v.TrangThai == 0))
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra biển số đang trong bãi
        /// </summary>
        public async Task<bool> KiemTraBienSoDangTrongBaiAsync(string bienSo)
        {
            var bienSoChuanHoa = bienSo?.Trim().ToUpper();
            if (string.IsNullOrEmpty(bienSoChuanHoa)) return false;

            return await _context.LuotGuis
                .Where(l => l.TrangThai == 0 && l.BienSoVao != null)
                .AnyAsync(l => l.BienSoVao!.Trim().ToUpper() == bienSoChuanHoa);
        }
    }

    /// <summary>
    /// Kết quả kiểm tra vé tháng chi tiết
    /// </summary>
    public class VeThangInfoResult
    {
        public bool CoVeThang { get; set; }
        public bool HopLe { get; set; }
        public bool DaHetHan { get; set; }
        public bool SapHetHan { get; set; }
        public int SoNgayConLai { get; set; }
        public bool KhongCoKhachHang { get; set; }
        public bool BienSoKhongKhop { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayHetHan { get; set; }
        public string? TenKhachHang { get; set; }
        public string? BienSoMacDinh { get; set; }
        public string? ThongBao { get; set; }
    }
}
