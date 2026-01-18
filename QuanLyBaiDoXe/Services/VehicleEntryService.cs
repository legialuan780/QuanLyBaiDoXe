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

        public async Task<LuotGui> XuLyXeVaoAsync(string maThe, string bienSoVao, string? hinhAnhVao, int? maViTri)
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

            // Cập nhật vị trí đỗ nếu có
            if (maViTri.HasValue)
            {
                var viTri = await _context.ViTriDos.FindAsync(maViTri.Value);
                if (viTri != null)
                {
                    viTri.TrangThai = 1; // Đánh dấu đã sử dụng
                }
            }

            // Tạo lượt gửi mới
            var luotGui = new LuotGui
            {
                MaThe = maThe,
                ThoiGianVao = DateTime.Now,
                BienSoVao = bienSoVao,
                HinhAnhVao = hinhAnhVao,
                MaViTri = maViTri,
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

        public async Task<decimal> TinhTienGuiXeAsync(LuotGui luotGui)
        {
            if (luotGui.ThoiGianVao == null || luotGui.ThoiGianRa == null)
            {
                return 0;
            }

            var thoiGianGui = luotGui.ThoiGianRa.Value - luotGui.ThoiGianVao.Value;
            var soPhut = (int)Math.Ceiling(thoiGianGui.TotalMinutes);

            // Lấy loại xe từ thẻ
            var theXe = await GetTheXeByMaTheAsync(luotGui.MaThe!);
            if (theXe?.MaLoaiXe == null)
            {
                return 0;
            }

            // Lấy cấu hình giá
            var cauHinh = await _context.CauHinhGia
                .Include(c => c.ChiTietGia)
                .FirstOrDefaultAsync(c => c.MaLoaiXe == theXe.MaLoaiXe);

            if (cauHinh == null || !cauHinh.ChiTietGia.Any())
            {
                // Giá mặc định nếu không có cấu hình: 5000đ/giờ
                var soGio = Math.Ceiling(thoiGianGui.TotalHours);
                return (decimal)soGio * 5000;
            }

            // Tính tiền theo cấu hình giá (theo block phút)
            decimal tongTien = 0;
            var chiTietGiaList = cauHinh.ChiTietGia.OrderBy(c => c.ThuTuBlock).ToList();
            int phutDaTinh = 0;

            foreach (var chiTiet in chiTietGiaList)
            {
                if (phutDaTinh >= soPhut) break;

                var soPhutBlock = chiTiet.SoPhutCuaBlock ?? 60;
                var soPhutApDung = Math.Min(soPhutBlock, soPhut - phutDaTinh);

                if (chiTiet.IsLuyTien == true)
                {
                    // Tính lũy tiến theo từng block
                    tongTien += (chiTiet.GiaTien ?? 0);
                }
                else
                {
                    // Tính theo tỷ lệ phút
                    tongTien += (chiTiet.GiaTien ?? 0) * soPhutApDung / soPhutBlock;
                }

                phutDaTinh += soPhutApDung;
            }

            // Nếu còn thời gian chưa tính, dùng giá block cuối cùng
            if (phutDaTinh < soPhut && chiTietGiaList.Any())
            {
                var lastBlock = chiTietGiaList.Last();
                var soPhutCon = soPhut - phutDaTinh;
                var soBlockCon = (int)Math.Ceiling((double)soPhutCon / (lastBlock.SoPhutCuaBlock ?? 60));
                tongTien += soBlockCon * (lastBlock.GiaTien ?? 0);
            }

            return tongTien > 0 ? tongTien : (decimal)Math.Ceiling(thoiGianGui.TotalHours) * 5000;
        }

        public async Task<List<LoaiXe>> GetLoaiXeListAsync()
        {
            return await _context.LoaiXes.ToListAsync();
        }
    }
}
