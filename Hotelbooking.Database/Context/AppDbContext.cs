using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hotelbooking.Database.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblBooking> TblBookings { get; set; }

    public virtual DbSet<TblBookingDetail> TblBookingDetails { get; set; }

    public virtual DbSet<TblBookingService> TblBookingServices { get; set; }

    public virtual DbSet<TblCustomer> TblCustomers { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblRoom> TblRooms { get; set; }

    public virtual DbSet<TblRoomType> TblRoomTypes { get; set; }

    public virtual DbSet<TblService> TblServices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=HotelBookingDB;User Id=sa;Password=sasa@123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblBooking>(entity =>
        {
            entity.HasKey(e => e.BookingId);

            entity.ToTable("Tbl_Booking");

            entity.Property(e => e.BookingId).ValueGeneratedNever();
            entity.Property(e => e.BookingDate).HasColumnType("datetime");
            entity.Property(e => e.BookingNo).HasMaxLength(30);
            entity.Property(e => e.CreateDateTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<TblBookingDetail>(entity =>
        {
            entity.HasKey(e => e.BookingDetailId);

            entity.ToTable("Tbl_BookingDetail");

            entity.Property(e => e.BookingDetailId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<TblBookingService>(entity =>
        {
            entity.HasKey(e => e.BookingserviceId);

            entity.ToTable("TblBookingService");

            entity.Property(e => e.BookingserviceId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.ServiceDate).HasColumnType("datetime");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);

            entity.ToTable("Tbl_Customer");

            entity.Property(e => e.CustomerId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.CreateDatetime).HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nrc)
                .HasMaxLength(50)
                .HasColumnName("NRC");
            entity.Property(e => e.Phone).HasMaxLength(30);
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);

            entity.ToTable("Tbl_Payment");

            entity.Property(e => e.PaymentId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(30);
            entity.Property(e => e.PaymentStatus).HasMaxLength(30);
        });

        modelBuilder.Entity<TblRoom>(entity =>
        {
            entity.HasKey(e => e.RoomId);

            entity.ToTable("Tbl_Room");

            entity.Property(e => e.RoomId).ValueGeneratedNever();
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.RoomNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<TblRoomType>(entity =>
        {
            entity.HasKey(e => e.RoomTypeId);

            entity.ToTable("Tbl_RoomType");

            entity.Property(e => e.RoomTypeId).ValueGeneratedNever();
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RoomTypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<TblService>(entity =>
        {
            entity.HasKey(e => e.ServiceId);

            entity.ToTable("Tbl_Service");

            entity.Property(e => e.ServiceId).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
