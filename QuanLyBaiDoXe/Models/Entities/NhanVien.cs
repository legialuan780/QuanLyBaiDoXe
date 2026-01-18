using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class NhanVien
{
    public int MaNhanVien { get; set; }

    public int? MaTaiKhoan { get; set; }

    public string HoTen { get; set; } = null!;

    public string? GioiTinh { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? Cccd { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public int? ChucVu { get; set; }

    public DateOnly? NgayVaoLam { get; set; }

    public bool? TrangThaiLamViec { get; set; }

    public virtual ICollection<CaLamViec> CaLamViecs { get; set; } = new List<CaLamViec>();

    public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();

    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }

    public virtual ICollection<SuCo> SuCos { get; set; } = new List<SuCo>();
}
