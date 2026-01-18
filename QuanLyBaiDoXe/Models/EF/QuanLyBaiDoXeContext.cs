using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.Entities;

namespace QuanLyBaiDoXe.Models.EF;

public partial class QuanLyBaiDoXeContext : DbContext
{
    public QuanLyBaiDoXeContext()
    {
    }

    public QuanLyBaiDoXeContext(DbContextOptions<QuanLyBaiDoXeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CaLamViec> CaLamViecs { get; set; }

    public virtual DbSet<CauHinhGium> CauHinhGia { get; set; }

    public virtual DbSet<ChiTietGium> ChiTietGia { get; set; }

    public virtual DbSet<DatCho> DatChos { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhuVuc> KhuVucs { get; set; }

    public virtual DbSet<LichLamViec> LichLamViecs { get; set; }

    public virtual DbSet<LoaiXe> LoaiXes { get; set; }

    public virtual DbSet<LuotGui> LuotGuis { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<SuCo> SuCos { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TheXe> TheXes { get; set; }

    public virtual DbSet<VeThang> VeThangs { get; set; }

    public virtual DbSet<ViTriDo> ViTriDos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-3Q3UNK4\\MSSQLSERVER01;Initial Catalog=QuanLyBaiDoXe;Persist Security Info=True;User ID=sa;Password=@Abcd@1234;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaLamViec>(entity =>
        {
            entity.HasKey(e => e.MaCa).HasName("PK__CaLamVie__27258E7B104183FA");

            entity.ToTable("CaLamViec");

            entity.Property(e => e.ThoiGianGiaoCa).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianNhanCa)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TienDauCa)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TongTienThu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThaiCa).HasDefaultValue(0);

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.CaLamViecs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__CaLamViec__MaNha__4AB81AF0");
        });

        modelBuilder.Entity<CauHinhGium>(entity =>
        {
            entity.HasKey(e => e.MaCauHinh).HasName("PK__CauHinhG__F0685B7D1DC4157D");

            entity.Property(e => e.IsUuTien).HasDefaultValue(false);
            entity.Property(e => e.TenCauHinh).HasMaxLength(100);

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.CauHinhGia)
                .HasForeignKey(d => d.MaLoaiXe)
                .HasConstraintName("FK__CauHinhGi__MaLoa__534D60F1");
        });

        modelBuilder.Entity<ChiTietGium>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietG__CDF0A1143D332C52");

            entity.Property(e => e.GiaTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.IsLuyTien).HasDefaultValue(false);

            entity.HasOne(d => d.MaCauHinhNavigation).WithMany(p => p.ChiTietGia)
                .HasForeignKey(d => d.MaCauHinh)
                .HasConstraintName("FK__ChiTietGi__MaCau__571DF1D5");
        });

        modelBuilder.Entity<DatCho>(entity =>
        {
            entity.HasKey(e => e.MaDatCho).HasName("PK__DatCho__707DAE6B0FB03B5C");

            entity.ToTable("DatCho");

            entity.Property(e => e.ThoiGianDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThoiGianDenDuKien).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianHetHan).HasColumnType("datetime");
            entity.Property(e => e.TrangThaiDatCho).HasDefaultValue(0);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DatChos)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DatCho__MaKhachH__60A75C0F");

            entity.HasOne(d => d.MaViTriNavigation).WithMany(p => p.DatChos)
                .HasForeignKey(d => d.MaViTri)
                .HasConstraintName("FK__DatCho__MaViTri__619B8048");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E51FC00C3B");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.SoDienThoai, "UQ__KhachHan__0389B7BD710660E6").IsUnique();

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__KhachHan__AD7C6528E29E7D82").IsUnique();

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

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.KhachHang)
                .HasForeignKey<KhachHang>(d => d.MaTaiKhoan)
                .HasConstraintName("FK__KhachHang__MaTai__44FF419A");
        });

        modelBuilder.Entity<KhuVuc>(entity =>
        {
            entity.HasKey(e => e.MaKhuVuc).HasName("PK__KhuVuc__0676EB839A8C7818");

            entity.ToTable("KhuVuc");

            entity.Property(e => e.TenKhuVuc).HasMaxLength(50);
        });

        modelBuilder.Entity<LichLamViec>(entity =>
        {
            entity.HasKey(e => e.MaLich).HasName("PK__LichLamV__728A9AE96631BF39");

            entity.ToTable("LichLamViec");

            entity.Property(e => e.GhiChu).HasMaxLength(200);

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.LichLamViecs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__LichLamVi__MaNha__47DBAE45");
        });

        modelBuilder.Entity<LoaiXe>(entity =>
        {
            entity.HasKey(e => e.MaLoaiXe).HasName("PK__LoaiXe__122512B51FA60CA7");

            entity.ToTable("LoaiXe");

            entity.Property(e => e.MoTa).HasMaxLength(100);
            entity.Property(e => e.TenLoaiXe).HasMaxLength(50);
        });

        modelBuilder.Entity<LuotGui>(entity =>
        {
            entity.HasKey(e => e.MaLuotGui).HasName("PK__LuotGui__C99FAC5C6EC871F6");

            entity.ToTable("LuotGui");

            entity.Property(e => e.BienSoRa)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BienSoVao)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.HinhAnhVao)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.HinhAnhRa)
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
                .HasConstraintName("FK__LuotGui__MaCaRa__74AE54BC");

            entity.HasOne(d => d.MaCaVaoNavigation).WithMany(p => p.LuotGuiMaCaVaoNavigations)
                .HasForeignKey(d => d.MaCaVao)
                .HasConstraintName("FK__LuotGui__MaCaVao__72C60C4A");

            entity.HasOne(d => d.MaDatChoNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaDatCho)
                .HasConstraintName("FK__LuotGui__MaDatCh__71D1E811");

            entity.HasOne(d => d.MaTheNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaThe)
                .HasConstraintName("FK__LuotGui__MaThe__70DDC3D8");

            entity.HasOne(d => d.MaViTriNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaViTri)
                .HasConstraintName("FK__LuotGui__MaViTri__73BA3083");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA47A83831F0");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.Cccd, "UQ__NhanVien__A955A0AADF4F9985").IsUnique();

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__NhanVien__AD7C65284D84AE19").IsUnique();

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

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.NhanVien)
                .HasForeignKey<NhanVien>(d => d.MaTaiKhoan)
                .HasConstraintName("FK__NhanVien__MaTaiK__3D5E1FD2");
        });

        modelBuilder.Entity<SuCo>(entity =>
        {
            entity.HasKey(e => e.MaSuCo).HasName("PK__SuCo__A69DF79F5D968962");

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
                .HasConstraintName("FK__SuCo__MaNhanVien__7A672E12");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C65291528254B");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC037E28EFE").IsUnique();

            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<TheXe>(entity =>
        {
            entity.HasKey(e => e.MaThe).HasName("PK__TheXe__314EEAAF40697924");

            entity.ToTable("TheXe");

            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoaiThe).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.TheXes)
                .HasForeignKey(d => d.MaLoaiXe)
                .HasConstraintName("FK__TheXe__MaLoaiXe__66603565");
        });

        modelBuilder.Entity<VeThang>(entity =>
        {
            entity.HasKey(e => e.MaVeThang).HasName("PK__VeThang__CA25BB9546B689D9");

            entity.ToTable("VeThang");

            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDau).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoTienDong).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.VeThangs)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__VeThang__MaKhach__6B24EA82");

            entity.HasOne(d => d.MaTheNavigation).WithMany(p => p.VeThangs)
                .HasForeignKey(d => d.MaThe)
                .HasConstraintName("FK__VeThang__MaThe__6C190EBB");
        });

        modelBuilder.Entity<ViTriDo>(entity =>
        {
            entity.HasKey(e => e.MaViTri).HasName("PK__ViTriDo__B08B247F0C7FA255");

            entity.ToTable("ViTriDo");

            entity.Property(e => e.TenViTri)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaKhuVucNavigation).WithMany(p => p.ViTriDos)
                .HasForeignKey(d => d.MaKhuVuc)
                .HasConstraintName("FK__ViTriDo__MaKhuVu__5CD6CB2B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
