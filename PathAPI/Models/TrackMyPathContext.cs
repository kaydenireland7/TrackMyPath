using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PathAPI.Models;

public partial class TrackMyPathContext : DbContext
{
    public TrackMyPathContext()
    {
    }

    public TrackMyPathContext(DbContextOptions<TrackMyPathContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Photo> Photos { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Location");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Photos__3214EC07B822DE27");

            entity.Property(e => e.Caption).HasMaxLength(255);
            entity.Property(e => e.FileUrl).HasMaxLength(1000);

            /*entity.HasOne(d => d.Location).WithOne(p => p.Photo)
                .HasForeignKey<Photo>(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Photos__Locations");*/
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trips__3214EC07C85A6E20");

            entity.Property(e => e.TripName).HasMaxLength(255);

            /*entity.HasOne(d => d.User).WithMany(p => p.Trips)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Trips__UserId__72C60C4A");*/
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07F7954F2E");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341C4B53C8").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
