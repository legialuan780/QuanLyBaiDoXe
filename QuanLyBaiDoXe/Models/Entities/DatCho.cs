using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class DatCho
{
    public int MaDatCho { get; set; }

    public int? MaKhachHang { get; set; }

    public int? MaViTri { get; set; }

    public DateTime? ThoiGianDat { get; set; }

    public DateTime? ThoiGianDenDuKien { get; set; }

    public DateTime? ThoiGianHetHan { get; set; }

    public int? TrangThaiDatCho { get; set; }

    public virtual ICollection<LuotGui> LuotGuis { get; set; } = new List<LuotGui>();

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual ViTriDo? MaViTriNavigation { get; set; }
}
