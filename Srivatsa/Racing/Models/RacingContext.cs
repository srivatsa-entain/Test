using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Racing.Models;

public partial class RacingContext : DbContext
{
    public RacingContext()
    {
    }

    public RacingContext(DbContextOptions<RacingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Race> Races { get; set; }

    //public virtual DbSet<Race> Racesa GetRaces(bool visibleRacesOnly  = false)
    //{
    //    if(visibleRacesOnly)
    //    {
    //        Races = Races.Select(r => r.Visible == 1);

    //    }
    //}
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=Db/racing.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Race>(entity =>
        {
            entity.ToTable("races");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AdvertisedStartTime)
                .HasColumnType("DATETIME")
                .HasColumnName("advertised_start_time");
            entity.Property(e => e.MeetingId).HasColumnName("meeting_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Visible).HasColumnName("visible");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
