using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class KhachHang
{
    public int MaKhachHang { get; set; }

    public int? MaTaiKhoan { get; set; }

    public string SoDienThoai { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? Cccd { get; set; }

    public string? DiaChi { get; set; }

    public string? BienSoXeMacDinh { get; set; }

    public virtual ICollection<DatCho> DatChos { get; set; } = new List<DatCho>();

    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }

    public virtual ICollection<VeThang> VeThangs { get; set; } = new List<VeThang>();
}
