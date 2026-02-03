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

    public virtual DbSet<DangKyLich> DangKyLiches { get; set; }

    public virtual DbSet<DatCho> DatChos { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhuVuc> KhuVucs { get; set; }

    public virtual DbSet<LichLamViec> LichLamViecs { get; set; }

    public virtual DbSet<LichSuGiaHanVe> LichSuGiaHanVes { get; set; }

    public virtual DbSet<LoaiXe> LoaiXes { get; set; }

    public virtual DbSet<LuotGui> LuotGuis { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<SuCo> SuCos { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TheXe> TheXes { get; set; }

    public virtual DbSet<VeThang> VeThangs { get; set; }

    public virtual DbSet<ViTriDo> ViTriDos { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=QuanLyBaiDoXe;Persist Security Info=True;User ID=sa;Password=1234;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaLamViec>(entity =>
        {
            entity.HasKey(e => e.MaCa).HasName("PK__CaLamVie__27258E7B3962BF67");

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
                .HasConstraintName("FK__CaLamViec__MaNha__5CD6CB2B");
        });

        modelBuilder.Entity<CauHinhGium>(entity =>
        {
            entity.HasKey(e => e.MaCauHinh).HasName("PK__CauHinhG__F0685B7D831D04A0");

            entity.Property(e => e.IsUuTien).HasDefaultValue(false);
            entity.Property(e => e.TenCauHinh).HasMaxLength(100);

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.CauHinhGia)
                .HasForeignKey(d => d.MaLoaiXe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CauHinhGi__MaLoa__75A278F5");
        });

        modelBuilder.Entity<ChiTietGium>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietG__CDF0A114B9C9F767");

            entity.Property(e => e.GiaTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.IsLuyTien).HasDefaultValue(false);

            entity.HasOne(d => d.MaCauHinhNavigation).WithMany(p => p.ChiTietGia)
                .HasForeignKey(d => d.MaCauHinh)
                .HasConstraintName("FK__ChiTietGi__MaCau__797309D9");
        });

        modelBuilder.Entity<DangKyLich>(entity =>
        {
            entity.HasKey(e => e.MaDangKy).HasName("PK__DangKyLi__BA90F02D45227B29");

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
                .HasConstraintName("FK__DangKyLic__MaLic__6E01572D");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.DangKyLichMaNhanVienNavigations)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DangKyLic__MaNha__6D0D32F4");

            entity.HasOne(d => d.MaNhanVienDuyetNavigation).WithMany(p => p.DangKyLichMaNhanVienDuyetNavigations)
                .HasForeignKey(d => d.MaNhanVienDuyet)
                .HasConstraintName("FK__DangKyLic__MaNha__70DDC3D8");
        });

        modelBuilder.Entity<DatCho>(entity =>
        {
            entity.HasKey(e => e.MaDatCho).HasName("PK__DatCho__707DAE6B0094E8F6");

            entity.ToTable("DatCho");

            entity.Property(e => e.ThoiGianDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ThoiGianDenDuKien).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianHetHan).HasColumnType("datetime");
            entity.Property(e => e.TrangThaiDatCho).HasDefaultValue(0);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DatChos)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DatCho__MaKhachH__03F0984C");

            entity.HasOne(d => d.MaViTriNavigation).WithMany(p => p.DatChos)
                .HasForeignKey(d => d.MaViTri)
                .HasConstraintName("FK__DatCho__MaViTri__04E4BC85");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E577551E1D");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.SoDienThoai, "UQ__KhachHan__0389B7BDB908E2D7").IsUnique();

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__KhachHan__AD7C6528617A2439").IsUnique();

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
                .HasConstraintName("FK__KhachHang__MaTai__59FA5E80");
        });

        modelBuilder.Entity<KhuVuc>(entity =>
        {
            entity.HasKey(e => e.MaKhuVuc).HasName("PK__KhuVuc__0676EB837C451EA3");

            entity.ToTable("KhuVuc");

            entity.Property(e => e.TenKhuVuc).HasMaxLength(50);
        });

        modelBuilder.Entity<LichLamViec>(entity =>
        {
            entity.HasKey(e => e.MaLich).HasName("PK__LichLamV__728A9AE93990AFEC");

            entity.ToTable("LichLamViec");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.LoaiCa).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.MaCaNavigation).WithMany(p => p.LichLamViecs)
                .HasForeignKey(d => d.MaCa)
                .HasConstraintName("FK__LichLamVie__MaCa__6754599E");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.LichLamViecs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichLamVi__MaNha__66603565");
        });

        modelBuilder.Entity<LichSuGiaHanVe>(entity =>
        {
            entity.HasKey(e => e.MaGiaHan).HasName("PK__LichSuGi__C3260BA48E5F221D");

            entity.ToTable("LichSuGiaHanVe");

            entity.Property(e => e.NgayGiaHan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.MaNhanVienThucHienNavigation).WithMany(p => p.LichSuGiaHanVes)
                .HasForeignKey(d => d.MaNhanVienThucHien)
                .HasConstraintName("FK__LichSuGia__MaNha__17F790F9");

            entity.HasOne(d => d.MaVeThangNavigation).WithMany(p => p.LichSuGiaHanVes)
                .HasForeignKey(d => d.MaVeThang)
                .HasConstraintName("FK__LichSuGia__MaVeT__151B244E");
        });

        modelBuilder.Entity<LoaiXe>(entity =>
        {
            entity.HasKey(e => e.MaLoaiXe).HasName("PK__LoaiXe__122512B5850C2DB5");

            entity.ToTable("LoaiXe");

            entity.Property(e => e.MoTa).HasMaxLength(100);
            entity.Property(e => e.TenLoaiXe).HasMaxLength(50);
        });

        modelBuilder.Entity<LuotGui>(entity =>
        {
            entity.HasKey(e => e.MaLuotGui).HasName("PK__LuotGui__C99FAC5CFE8744F3");

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
                .HasConstraintName("FK__LuotGui__MaCaRa__1EA48E88");

            entity.HasOne(d => d.MaCaVaoNavigation).WithMany(p => p.LuotGuiMaCaVaoNavigations)
                .HasForeignKey(d => d.MaCaVao)
                .HasConstraintName("FK__LuotGui__MaCaVao__1CBC4616");

            entity.HasOne(d => d.MaDatChoNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaDatCho)
                .HasConstraintName("FK__LuotGui__MaDatCh__1BC821DD");

            entity.HasOne(d => d.MaTheNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaThe)
                .HasConstraintName("FK__LuotGui__MaThe__1AD3FDA4");

            entity.HasOne(d => d.MaViTriNavigation).WithMany(p => p.LuotGuis)
                .HasForeignKey(d => d.MaViTri)
                .HasConstraintName("FK__LuotGui__MaViTri__1DB06A4F");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA477121151E");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.Cccd, "UQ__NhanVien__A955A0AAE37E262F").IsUnique();

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__NhanVien__AD7C6528CA5D77C3").IsUnique();

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
                .HasConstraintName("FK__NhanVien__MaTaiK__52593CB8");
        });

        modelBuilder.Entity<SuCo>(entity =>
        {
            entity.HasKey(e => e.MaSuCo).HasName("PK__SuCo__A69DF79F1CA2E090");

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
                .HasConstraintName("FK__SuCo__MaNhanVien__2645B050");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529047825DD");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0EEE0D0D0").IsUnique();

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

        modelBuilder.Entity<TheXe>(entity =>
        {
            entity.HasKey(e => e.MaThe).HasName("PK__TheXe__314EEAAF31A72C32");

            entity.ToTable("TheXe");

            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoaiThe).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.MaLoaiXeNavigation).WithMany(p => p.TheXes)
                .HasForeignKey(d => d.MaLoaiXe)
                .HasConstraintName("FK__TheXe__MaLoaiXe__09A971A2");
        });

        modelBuilder.Entity<VeThang>(entity =>
        {
            entity.HasKey(e => e.MaVeThang).HasName("PK__VeThang__CA25BB95E52653BC");

            entity.ToTable("VeThang");

            entity.Property(e => e.MaThe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDau).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoTienDong).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.VeThangs)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__VeThang__MaKhach__0E6E26BF");

            entity.HasOne(d => d.MaTheNavigation).WithMany(p => p.VeThangs)
                .HasForeignKey(d => d.MaThe)
                .HasConstraintName("FK__VeThang__MaThe__0F624AF8");
        });

        modelBuilder.Entity<ViTriDo>(entity =>
        {
            entity.HasKey(e => e.MaViTri).HasName("PK__ViTriDo__B08B247F945FC94D");

            entity.ToTable("ViTriDo");

            entity.Property(e => e.TenViTri)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(0);

            entity.HasOne(d => d.MaKhuVucNavigation).WithMany(p => p.ViTriDos)
                .HasForeignKey(d => d.MaKhuVuc)
                .HasConstraintName("FK__ViTriDo__MaKhuVu__00200768");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
