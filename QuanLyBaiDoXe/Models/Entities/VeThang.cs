using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class VeThang
{
    public int MaVeThang { get; set; }

    public int? MaKhachHang { get; set; }

    public string? MaThe { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayHetHan { get; set; }

    public decimal? SoTienDong { get; set; }

    public bool? TrangThai { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual TheXe? MaTheNavigation { get; set; }
}
