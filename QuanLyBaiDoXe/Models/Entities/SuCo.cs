using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class SuCo
{
    public int MaSuCo { get; set; }

    public DateTime? ThoiGianGhiNhan { get; set; }

    public int? MaNhanVien { get; set; }

    public string? LoaiSuCo { get; set; }

    public string? MaThe { get; set; }

    public int? MaViTri { get; set; }

    public string? MoTaChiTiet { get; set; }

    public int? TrangThaiXuLy { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
