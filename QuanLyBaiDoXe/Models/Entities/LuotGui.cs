using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class LuotGui
{
    public long MaLuotGui { get; set; }

    public string? MaThe { get; set; }

    public int? MaDatCho { get; set; }

    public int? MaCaVao { get; set; }

    public DateTime? ThoiGianVao { get; set; }

    public string? BienSoVao { get; set; }

    public string? HinhAnhVao { get; set; }

    public int? MaViTri { get; set; }

    public int? MaCaRa { get; set; }

    public DateTime? ThoiGianRa { get; set; }

    public string? BienSoRa { get; set; }

    public string? HinhAnhRa { get; set; }

    public decimal? TongTien { get; set; }

    public int? TrangThai { get; set; }

    public virtual CaLamViec? MaCaRaNavigation { get; set; }

    public virtual CaLamViec? MaCaVaoNavigation { get; set; }

    public virtual DatCho? MaDatChoNavigation { get; set; }

    public virtual TheXe? MaTheNavigation { get; set; }

    public virtual ViTriDo? MaViTriNavigation { get; set; }
}
