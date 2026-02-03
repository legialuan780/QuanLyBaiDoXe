using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class DangKyLich
{
    public int MaDangKy { get; set; }

    public int MaNhanVien { get; set; }

    public int? MaLich { get; set; }

    public int LoaiYeuCau { get; set; }

    public DateTime? NgayYeuCau { get; set; }

    public DateOnly? NgayLamMoi { get; set; }

    public TimeOnly? GioBatDauMoi { get; set; }

    public TimeOnly? GioKetThucMoi { get; set; }

    public string? LyDo { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public int? MaNhanVienDuyet { get; set; }

    public DateTime? ThoiGianDuyet { get; set; }

    public string? GhiChuDuyet { get; set; }

    public virtual LichLamViec? MaLichNavigation { get; set; }

    public virtual NhanVien? MaNhanVienDuyetNavigation { get; set; }

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
