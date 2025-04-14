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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK_Location");

            entity.Property(e => e.LocationId)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
