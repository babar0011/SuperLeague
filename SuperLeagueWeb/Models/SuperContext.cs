using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SuperLeagueWeb.Models
{
    public partial class SuperContext : DbContext
    {
        public SuperContext()
        {
        }

        public SuperContext(DbContextOptions<SuperContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Country> Countries { get; set; } = null!;
        public virtual DbSet<Player> Players { get; set; } = null!;
        public virtual DbSet<Team> Teams { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(e => e.FlagUrl)
                    .HasMaxLength(255)
                    .HasColumnName("FlagURL");

                entity.Property(e => e.KitUrl)
                    .HasMaxLength(255)
                    .HasColumnName("KitURL");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .HasColumnName("ImageURL");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Skill).HasMaxLength(50);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Players_Category");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Players_Country");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.Property(e => e.FlagUrl)
                    .HasMaxLength(255)
                    .HasColumnName("FlagURL");

                entity.Property(e => e.KitUrl)
                    .HasMaxLength(255)
                    .HasColumnName("KitURL");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasMany(d => d.Players)
                    .WithMany(p => p.Teams)
                    .UsingEntity<Dictionary<string, object>>(
                        "TeamPlayer",
                        l => l.HasOne<Player>().WithMany().HasForeignKey("PlayerId").HasConstraintName("FK_TeamPlayers_Player"),
                        r => r.HasOne<Team>().WithMany().HasForeignKey("TeamId").HasConstraintName("FK_TeamPlayers_Team"),
                        j =>
                        {
                            j.HasKey("TeamId", "PlayerId");

                            j.ToTable("TeamPlayers");
                        });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
