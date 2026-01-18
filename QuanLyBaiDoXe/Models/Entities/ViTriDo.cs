using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class ViTriDo
{
    public int MaViTri { get; set; }

    public int? MaKhuVuc { get; set; }

    public string? TenViTri { get; set; }

    public int? TrangThai { get; set; }

    public virtual ICollection<DatCho> DatChos { get; set; } = new List<DatCho>();

    public virtual ICollection<LuotGui> LuotGuis { get; set; } = new List<LuotGui>();

    public virtual KhuVuc? MaKhuVucNavigation { get; set; }
}
