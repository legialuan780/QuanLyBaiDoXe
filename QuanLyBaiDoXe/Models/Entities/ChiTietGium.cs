using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class ChiTietGium
{
    public int MaChiTiet { get; set; }

    public int? MaCauHinh { get; set; }

    public int? ThuTuBlock { get; set; }

    public int? SoPhutCuaBlock { get; set; }

    public decimal? GiaTien { get; set; }

    public bool? IsLuyTien { get; set; }

    public virtual CauHinhGium? MaCauHinhNavigation { get; set; }
}
