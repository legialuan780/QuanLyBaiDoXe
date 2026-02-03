using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class CaLamViec
{
    public int MaCa { get; set; }

    public int? MaNhanVien { get; set; }

    public DateTime? ThoiGianNhanCa { get; set; }

    public DateTime? ThoiGianGiaoCa { get; set; }

    public decimal? TienDauCa { get; set; }

    public decimal? TongTienHeThong { get; set; }

    public decimal? TienMatBanGiao { get; set; }

    public string? GhiChuBanGiao { get; set; }

    public int? TrangThaiCa { get; set; }

    public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();

    public virtual ICollection<LuotGui> LuotGuiMaCaRaNavigations { get; set; } = new List<LuotGui>();

    public virtual ICollection<LuotGui> LuotGuiMaCaVaoNavigations { get; set; } = new List<LuotGui>();

    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
