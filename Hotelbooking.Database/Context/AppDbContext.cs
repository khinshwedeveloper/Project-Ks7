using System;
using System.Collections.Generic;
using Hotelbooking.Database.Models;
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

    // Master Tables
    public virtual DbSet<TblCustomer> TblCustomers { get; set; }
    public virtual DbSet<TblFloor> TblFloors { get; set; }
    public virtual DbSet<TblRoomType> TblRoomTypes { get; set; }
    public virtual DbSet<TblRoomStatus> TblRoomStatuses { get; set; }
    public virtual DbSet<TblRoom> TblRooms { get; set; }
    public virtual DbSet<TblRoomPrice> TblRoomPrices { get; set; }
    public virtual DbSet<TblRoomPriceDetail> TblRoomPriceDetails { get; set; }
    public virtual DbSet<TblServiceCategory> TblServiceCategories { get; set; }
    public virtual DbSet<TblService> TblServices { get; set; }
    public virtual DbSet<TblExtraBedPrice> TblExtraBedPrices { get; set; }
    public virtual DbSet<TblBanking> TblBankings { get; set; }

    // Transaction Tables
    public virtual DbSet<TblBooking> TblBookings { get; set; }
    public virtual DbSet<TblBookingDetail> TblBookingDetails { get; set; }
    public virtual DbSet<TblBookingService> TblBookingServices { get; set; }
    public virtual DbSet<TblRoomTransfer> TblRoomTransfers { get; set; }
    public virtual DbSet<TblPayment> TblPayments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=HotelBookingDB;User Id=sa;Password=sasa@123;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. TblCustomer
        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.ToTable("TblCustomer");
            entity.HasIndex(e => e.CustomerCode).IsUnique();

            entity.Property(e => e.CustomerCode).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Phone).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Nrc).HasMaxLength(100).IsUnicode(false).HasColumnName("NRC");
            entity.Property(e => e.Gender).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.DateOfBirth).HasColumnType("date");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        // 2. TblFloor
        modelBuilder.Entity<TblFloor>(entity =>
        {
            entity.HasKey(e => e.FloorId);
            entity.ToTable("TblFloor");
            entity.HasIndex(e => e.FloorName).IsUnique();

            entity.Property(e => e.FloorName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        // 3. TblRoomType
        modelBuilder.Entity<TblRoomType>(entity =>
        {
            entity.HasKey(e => e.RoomTypeId);
            entity.ToTable("TblRoomType");
            entity.HasIndex(e => e.RoomTypeName).IsUnique();

            entity.Property(e => e.RoomTypeName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.MaxGuest);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        // 4. TblRoomStatus
        modelBuilder.Entity<TblRoomStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId);
            entity.ToTable("TblRoomStatus");
            entity.HasIndex(e => e.StatusName).IsUnique();

            entity.Property(e => e.StatusName).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
        });

        // 5. TblRoom
        modelBuilder.Entity<TblRoom>(entity =>
        {
            entity.HasKey(e => e.RoomId);
            entity.ToTable("TblRoom");
            entity.HasIndex(e => e.RoomNo).IsUnique();

            entity.Property(e => e.RoomNo).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(300).IsUnicode(false);

            entity.HasOne(d => d.Floor)
                .WithMany(p => p.TblRooms)
                .HasForeignKey(d => d.FloorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRoom_TblFloor");

            entity.HasOne(d => d.RoomType)
                .WithMany(p => p.TblRooms)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRoom_TblRoomType");

            entity.HasOne(d => d.Status)
                .WithMany(p => p.TblRooms)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRoom_TblRoomStatus");
        });

        // 6. TblRoomPrice
        modelBuilder.Entity<TblRoomPrice>(entity =>
        {
            entity.HasKey(e => e.RoomPriceId);
            entity.ToTable("TblRoomPrice");

            entity.Property(e => e.PriceName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.EffectiveDate).HasColumnType("date");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.RoomType)
                .WithMany(p => p.TblRoomPrices)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRoomPrice_TblRoomType");
        });

        // 7. TblRoomPriceDetail
        modelBuilder.Entity<TblRoomPriceDetail>(entity =>
        {
            entity.HasKey(e => e.RoomPriceDetailId);
            entity.ToTable("TblRoomPriceDetail");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.RoomPrice)
                .WithMany(p => p.TblRoomPriceDetails)
                .HasForeignKey(d => d.RoomPriceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TblRoomPriceDetail_TblRoomPrice");
        });

        // 8. TblServiceCategory
        modelBuilder.Entity<TblServiceCategory>(entity =>
        {
            entity.HasKey(e => e.ServiceCategoryId);
            entity.ToTable("TblServiceCategory");
            entity.HasIndex(e => e.CategoryName).IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        // 9. TblService
        modelBuilder.Entity<TblService>(entity =>
        {
            entity.HasKey(e => e.ServiceId);
            entity.ToTable("TblService");

            entity.Property(e => e.ServiceName).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.ServiceCategory)
                .WithMany(p => p.TblServices)
                .HasForeignKey(d => d.ServiceCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblService_TblServiceCategory");
        });

        // 10. TblExtraBedPrice
        modelBuilder.Entity<TblExtraBedPrice>(entity =>
        {
            entity.HasKey(e => e.ExtraBedPriceId);
            entity.ToTable("TblExtraBedPrice");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EffectiveDate).HasColumnType("date");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.RoomType)
                .WithMany(p => p.TblExtraBedPrices)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblExtraBedPrice_TblRoomType");
        });

        // 11. TblBanking
        modelBuilder.Entity<TblBanking>(entity =>
        {
            entity.HasKey(e => e.BankingId);
            entity.ToTable("TblBanking");

            entity.Property(e => e.BankName).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.AccountName).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.AccountNumber).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.BranchName).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.AccountType).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        // 12. TblBooking
        modelBuilder.Entity<TblBooking>(entity =>
        {
            entity.HasKey(e => e.BookingId);
            entity.ToTable("TblBooking");
            entity.HasIndex(e => e.BookingNo).IsUnique();

            entity.Property(e => e.BookingNo).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.BookingDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(500).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.TblBookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblBooking_TblCustomer");
        });

        // 13. TblBookingDetail
        modelBuilder.Entity<TblBookingDetail>(entity =>
        {
            entity.HasKey(e => e.BookingDetailId);
            entity.ToTable("TblBookingDetail");

            entity.Property(e => e.RoomPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExtraBedQty).HasDefaultValue(0);
            entity.Property(e => e.ExtraBedPrice).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalRoomPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Remarks).HasMaxLength(500).IsUnicode(false);

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.TblBookingDetails)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblBookingDetail_TblBooking");

            entity.HasOne(d => d.Room)
                .WithMany(p => p.TblBookingDetails)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblBookingDetail_TblRoom");
        });

        // 14. TblBookingService
        modelBuilder.Entity<TblBookingService>(entity =>
        {
            entity.HasKey(e => e.BookingServiceId);
            entity.ToTable("TblBookingService");

            entity.Property(e => e.Qty).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.TblBookingServices)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblBookingService_TblBooking");

            entity.HasOne(d => d.BookingDetail)
                .WithMany(p => p.TblBookingServices)
                .HasForeignKey(d => d.BookingDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblBookingService_TblBookingDetail");

            entity.HasOne(d => d.Service)
                .WithMany(p => p.TblBookingServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblBookingService_TblService");
        });

        // 15. TblRoomTransfer
        modelBuilder.Entity<TblRoomTransfer>(entity =>
        {
            entity.HasKey(e => e.RoomTransferId);
            entity.ToTable("TblRoomTransfer");

            entity.Property(e => e.TransferDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Reason).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(500).IsUnicode(false);

            entity.HasOne(d => d.BookingDetail)
                .WithMany(p => p.TblRoomTransfers)
                .HasForeignKey(d => d.BookingDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRoomTransfer_TblBookingDetail");

            entity.HasOne(d => d.FromRoom)
                .WithMany(p => p.TblRoomTransferFromRooms)
                .HasForeignKey(d => d.FromRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRoomTransfer_FromRoom");

            entity.HasOne(d => d.ToRoom)
                .WithMany(p => p.TblRoomTransferToRooms)
                .HasForeignKey(d => d.ToRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRoomTransfer_ToRoom");
        });

        // 16. TblPayment
        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            entity.ToTable("TblPayment");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.ReferenceNo).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(500).IsUnicode(false);

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPayment_TblBooking");

            entity.HasOne(d => d.Banking)
                .WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.BankingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPayment_TblBanking");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
