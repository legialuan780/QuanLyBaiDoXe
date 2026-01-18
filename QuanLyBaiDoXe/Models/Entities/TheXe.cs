using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class TheXe
{
    public string MaThe { get; set; } = null!;

    public int? MaLoaiXe { get; set; }

    public int? LoaiThe { get; set; }

    public int? TrangThai { get; set; }

    public virtual ICollection<LuotGui> LuotGuis { get; set; } = new List<LuotGui>();

    public virtual LoaiXe? MaLoaiXeNavigation { get; set; }

    public virtual ICollection<VeThang> VeThangs { get; set; } = new List<VeThang>();
}
