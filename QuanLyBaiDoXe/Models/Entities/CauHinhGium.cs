using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class CauHinhGium
{
    public int MaCauHinh { get; set; }

    public string? TenCauHinh { get; set; }

    public int? MaLoaiXe { get; set; }

    public TimeOnly? GioBatDau { get; set; }

    public TimeOnly? GioKetThuc { get; set; }

    public bool? IsUuTien { get; set; }

    public virtual ICollection<ChiTietGium> ChiTietGia { get; set; } = new List<ChiTietGium>();

    public virtual LoaiXe? MaLoaiXeNavigation { get; set; }
}
