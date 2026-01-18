using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class KhuVuc
{
    public int MaKhuVuc { get; set; }

    public string? TenKhuVuc { get; set; }

    public virtual ICollection<ViTriDo> ViTriDos { get; set; } = new List<ViTriDo>();
}
