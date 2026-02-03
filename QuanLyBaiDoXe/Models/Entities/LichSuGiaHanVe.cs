using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class LichSuGiaHanVe
{
    public int MaGiaHan { get; set; }

    public int? MaVeThang { get; set; }

    public DateTime? NgayGiaHan { get; set; }

    public DateOnly? ThoiHanCu { get; set; }

    public DateOnly? ThoiHanMoi { get; set; }

    public decimal? SoTien { get; set; }

    public int? MaNhanVienThucHien { get; set; }

    public virtual NhanVien? MaNhanVienThucHienNavigation { get; set; }

    public virtual VeThang? MaVeThangNavigation { get; set; }
}
