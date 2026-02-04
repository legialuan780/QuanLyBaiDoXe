using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Models.EF;

public partial class QuanLyBaiDoXeContext : DbContext
{
    public QuanLyBaiDoXeContext(DbContextOptions<QuanLyBaiDoXeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CaLamViec> CaLamViecs { get; set; }

    public virtual DbSet<CauHinhGium> CauHinhGia { get; set; }

    public virtual DbSet<ChiTietGium> ChiTietGia { get; set; }

    public virtual DbSet<DangKyLich> DangKyLiches { get; set; }

    public virtual DbSet<DatCho> DatChos { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhuVuc> KhuVucs { get; set; }

    public virtual DbSet<LichLamViec> LichLamViecs { get; set; }

    public virtual DbSet<LichSuGiaHanThe> LichSuGiaHanThes { get; set; }

    public virtual DbSet<LoaiXe> LoaiXes { get; set; }

    public virtual DbSet<LuotGui> LuotGuis { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<SuCo> SuCos { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TheThang> TheThangs { get; set; }

    public virtual DbSet<TheXe> TheXes { get; set; }

    public virtual DbSet<ViTriDo> ViTriDos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaLamViec>(entity =>
        {
            entity.HasKey(e => e.MaCa).HasName("PK__CaLamVie__27258E7BE52D2279");

            entity.ToTable("CaLamViec");

            entity.Property(e => e.GhiChuBanGiao).HasMaxLength(255);
            entity.Property(e => e.ThoiGianGiaoCa).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianNhanCa)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TienDauCa)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TienMatBanGiao)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TongTienHeThong)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThaiCa).HasDefaultValue(0);

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.CaLamViecs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__CaLamViec__MaNha__3A81B327");
        });

        modelBuilder.Entity<CauHinhGium>(entity =>
        {
            entity.HasKey(e => e.MaCauHinh).HasName("PK__CauHinhG__F0685B7DD40FB77D");

            entity.Property(e => e.IsUuTien).HasDefaultValue(false);
            entity.Property(e => e.TenCauHinh).HasMaxLength(100);

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.CauHinhGia)
                .HasForeignKey(d => d.MaLoaiXe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CauHinhGi__MaLoa__5441852A");
        });

        modelBuilder.Entity<ChiTietGium>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietG__CDF0A114BAE26507");

            entity.Property(e => e.GiaTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.IsLuyTien).HasDefaultValue(false);

            entity.HasOne(d => d.MaCauHinhNavigation).WithMany(p => p.ChiTietGia)
                .HasForeignKey(d => d.MaCauHinh)
                .HasConstraintName("FK__ChiTietGi__MaCau__5812160E");
        });

        modelBuilder.Entity<DangKyLich>(entity =>
        {
            entity.HasKey(e => e.MaDangKy).HasName("PK__DangKyLi__BA90F02D23780F6B");

            entity.ToTable("DangKyLich");

            entity.Property(e => e.GhiChuDuyet).HasMaxLength(255);
            entity.Property(e => e.LyDo).HasMaxLength(500);
            entity.Property(e => e.NgayYeuCau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThoiGianDuyet).HasColumnType("datetime");
            entity.Property(e => e.TrangThaiDuyet).HasDefaultValue(0);

            entity.HasOne(d => d.MaLichNavigation).WithMany(p => p.DangKyLiches)
                .HasForeignKey(d => d.MaLich)
                .HasConstraintName("FK__DangKyLic__MaLic__4BAC3F29");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.DangKyLichMaNhanVienNavigations)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DangKyLic__MaNha__4AB81AF0");

            entity.HasOne(d => d.MaNhanVienDuyetNavigation).WithMany(p => p.DangKyLichMaNhanVienDuyetNavigations)
                .HasForeignKey(d => d.MaNhanVienDuyet)
                .HasConstraintName("FK__DangKyLic__MaNha__4E88ABD4");
        });

        modelBuilder.Entity<DatCho>(entity =>
        {
            entity.HasKey(e => e.MaDatCho).HasName("PK__DatCho__707DAE6B49D1BC69");

            entity.ToTable("DatCho");

            entity.Property(e => e.ThoiGianDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThoiGianDenDuKien).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianHetHan).HasColumnType("datetime");
            entity.Property(e => e.TrangThaiDatCho).HasDefaultValue(0);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DatChos)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DatCho__MaKhachH__6383C8BA");

            entity.HasOne(d => d.MaViTriNavigation).WithMany(p => p.DatChos)
                .HasForeignKey(d => d.MaViTri)
                .HasConstraintName("FK__DatCho__MaViTri__6477ECF3");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E5BEDD8A5F");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.SoDienThoai, "UQ__KhachHan__0389B7BD4CF38D96").IsUnique();

            entity.Property(e => e.BienSoXeMacDinh)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK__KhachHang__MaTai__37A5467C");
        });

        modelBuilder.Entity<KhuVuc>(entity =>
        {
            entity.HasKey(e => e.MaKhuVuc).HasName("PK__KhuVuc__0676EB838DC871BE");

            entity.ToTable("KhuVuc");

            entity.Property(e => e.TenKhuVuc).HasMaxLength(50);

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.KhuVucs)
                .HasForeignKey(d => d.MaLoaiXe)
                .HasConstraintName("FK__KhuVuc__MaLoaiXe__5CD6CB2B");
        });

        modelBuilder.Entity<LichLamViec>(entity =>
        {
            entity.HasKey(e => e.MaLich).HasName("PK__LichLamV__728A9AE9F5084C89");

            entity.ToTable("LichLamViec");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.MaCaNavigation).WithMany(p => p.LichLamViecs)
                .HasForeignKey(d => d.MaCa)
                .HasConstraintName("FK__LichLamVie__MaCa__44FF419A");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.LichLamViecs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichLamVi__MaNha__440B1D61");
        });

        modelBuilder.Entity<LichSuGiaHanThe>(entity =>
        {
            entity.HasKey(e => e.MaGiaHan).HasName("PK__LichSuGi__C3260BA4FEC1F1F7");

            entity.ToTable("LichSuGiaHanThe");

            entity.Property(e => e.NgayGiaHan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.MaNhanVienThucHienNavigation).WithMany(p => p.LichSuGiaHanThes)
                .HasForeignKey(d => d.MaNhanVienThucHien)
                .HasConstraintName("FK__LichSuGia__MaNha__778AC167");

            entity.HasOne(d => d.MaTheThangNavigation).WithMany(p => p.LichSuGiaHanThes)
                .HasForeignKey(d => d.MaTheThang)
                .HasConstraintName("FK__LichSuGia__MaThe__74AE54BC");
        });

        modelBuilder.Entity<LoaiXe>(entity =>
        {
            entity.HasKey(e => e.MaLoaiXe).HasName("PK__LoaiXe__122512B5E62EF716");

            entity.ToTable("LoaiXe");

            entity.Property(e => e.GiaThang).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MoTa).HasMaxLength(100);
            entity.Property(e => e.TenLoaiXe).HasMaxLength(50);
        });

        modelBuilder.Entity<LuotGui>(entity =>
        {
            entity.HasKey(e => e.MaLuotGui).HasName("PK__LuotGui__C99FAC5C0DAE4BC6");

            entity.ToTable("LuotGui", tb =>
                {
                    tb.HasTrigger("Trg_UpdateViTri_CheckIn");
                    tb.HasTrigger("Trg_UpdateViTri_CheckOut");
                });

            entity.Property(e => e.BienSoRa)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BienSoVao)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.HinhAnhRa)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.HinhAnhVao)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ThoiGianRa).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianVao).HasColumnType("datetime");
            entity.Property(e => e.TongTien)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaCaRaNavigation).WithMany(p => p.LuotGuiMaCaRaNavigations)
                .HasForeignKey(d => d.MaCaRa)
                .HasConstraintName("FK__LuotGui__MaCaRa__7E37BEF6");

            entity.HasOne(d => d.MaCaVaoNavigation).WithMany(p => p.LuotGuiMaCaVaoNavigations)
                .HasForeignKey(d => d.MaCaVao)
                .HasConstraintName("FK__LuotGui__MaCaVao__7C4F7684");

            entity.HasOne(d => d.MaDatChoNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaDatCho)
                .HasConstraintName("FK__LuotGui__MaDatCh__7B5B524B");

            entity.HasOne(d => d.MaTheNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaThe)
                .HasConstraintName("FK__LuotGui__MaThe__7A672E12");

            entity.HasOne(d => d.MaViTriNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaViTri)
                .HasConstraintName("FK__LuotGui__MaViTri__7D439ABD");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA47CE8916DA");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.Cccd, "UQ__NhanVien__A955A0AAA691E35B").IsUnique();

            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.ChucVu).HasDefaultValue(1);
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayVaoLam).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThaiLamViec).HasDefaultValue(true);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK__NhanVien__MaTaiK__30F848ED");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3214EC07624D8D5C");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(100);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK_PasswordResetTokens_TaiKhoan");
        });

        modelBuilder.Entity<SuCo>(entity =>
        {
            entity.HasKey(e => e.MaSuCo).HasName("PK__SuCo__A69DF79FE3197618");

            entity.ToTable("SuCo");

            entity.Property(e => e.LoaiSuCo).HasMaxLength(50);
            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MoTaChiTiet).HasMaxLength(500);
            entity.Property(e => e.ThoiGianGhiNhan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThaiXuLy).HasDefaultValue(0);

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.SuCos)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__SuCo__MaNhanVien__05D8E0BE");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529CFD0EE72");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC02ABD937F").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D1053450F6298F").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.QuyenHan)
                .HasMaxLength(50)
                .HasDefaultValue("Khách hàng");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<TheThang>(entity =>
        {
            entity.HasKey(e => e.MaTheThang).HasName("PK__TheThang__28FC659BA6B6195D");

            entity.ToTable("TheThang");

            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDau).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoTienDong).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.TheThangs)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__TheThang__MaKhac__6E01572D");

            entity.HasOne(d => d.MaTheNavigation).WithMany(p => p.TheThangs)
                .HasForeignKey(d => d.MaThe)
                .HasConstraintName("FK__TheThang__MaThe__6EF57B66");
        });

        modelBuilder.Entity<TheXe>(entity =>
        {
            entity.HasKey(e => e.MaThe).HasName("PK__TheXe__314EEAAFAAADC29D");

            entity.ToTable("TheXe");

            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoaiThe).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.TheXes)
                .HasForeignKey(d => d.MaLoaiXe)
                .HasConstraintName("FK__TheXe__MaLoaiXe__693CA210");
        });

        modelBuilder.Entity<ViTriDo>(entity =>
        {
            entity.HasKey(e => e.MaViTri).HasName("PK__ViTriDo__B08B247F572E30DD");

            entity.ToTable("ViTriDo");

            entity.Property(e => e.TenViTri)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaKhuVucNavigation).WithMany(p => p.ViTriDos)
                .HasForeignKey(d => d.MaKhuVuc)
                .HasConstraintName("FK__ViTriDo__MaKhuVu__5FB337D6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
