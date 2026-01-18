using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class LichLamViec
{
    public int MaLich { get; set; }

    public int? MaNhanVien { get; set; }

    public DateOnly? NgayLamViec { get; set; }

    public int? CaLamViec { get; set; }

    public string? GhiChu { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
