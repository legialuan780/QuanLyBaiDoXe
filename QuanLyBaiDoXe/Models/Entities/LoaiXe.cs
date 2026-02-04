using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class LoaiXe
{
    public int MaLoaiXe { get; set; }

    public string? TenLoaiXe { get; set; }

    public string? MoTa { get; set; }

    public decimal? GiaThang { get; set; }

    public virtual ICollection<CauHinhGium> CauHinhGia { get; set; } = new List<CauHinhGium>();

    public virtual ICollection<KhuVuc> KhuVucs { get; set; } = new List<KhuVuc>();

    public virtual ICollection<TheXe> TheXes { get; set; } = new List<TheXe>();
}
