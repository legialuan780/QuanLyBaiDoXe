using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class LoaiXe
{
    public int MaLoaiXe { get; set; }

    public string? TenLoaiXe { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<CauHinhGium> CauHinhGia { get; set; } = new List<CauHinhGium>();

    public virtual ICollection<TheXe> TheXes { get; set; } = new List<TheXe>();
}
