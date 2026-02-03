using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class LichLamViec
{
    public int MaLich { get; set; }

    public int MaNhanVien { get; set; }

    public int? MaCa { get; set; }

    public DateOnly NgayLamViec { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public int? LoaiCa { get; set; }

    public int? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<DangKyLich> DangKyLiches { get; set; } = new List<DangKyLich>();

    public virtual CaLamViec? MaCaNavigation { get; set; }

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
