using ExamBuilder.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExamBuilder.Data;

public class ExamBuilderContext : IdentityDbContext<ExamBuilderUser>
{
    public ExamBuilderContext(DbContextOptions<ExamBuilderContext> options) : base(options)
    {
    }

    public DbSet<Lehrveranstaltung> Lehrveranstaltungen { get; set; }
    public DbSet<Kapitel> Kapitel { get; set; }
    public DbSet<McFrage> McFragen { get; set; }
    public DbSet<McAntwort> McAntworten { get; set; }
    public DbSet<Pruefung> Pruefungen { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Lehrveranstaltung -> Kapitel (1:n, Cascade Delete)
        modelBuilder.Entity<Kapitel>()
            .HasOne(k => k.Lehrveranstaltung)
            .WithMany(lv => lv.Kapitel)
            .HasForeignKey(k => k.LehrveranstaltungId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lehrveranstaltung -> Pruefung (1:n, Cascade Delete)
        modelBuilder.Entity<Pruefung>()
            .HasOne(p => p.Lehrveranstaltung)
            .WithMany(lv => lv.Pruefungen)
            .HasForeignKey(p => p.LehrveranstaltungId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kapitel -> McFrage (1:n, Cascade Delete)
        modelBuilder.Entity<McFrage>()
            .HasOne(f => f.Kapitel)
            .WithMany(k => k.McFragen)
            .HasForeignKey(f => f.KapitelId)
            .OnDelete(DeleteBehavior.Cascade);

        // McFrage -> McAntwort (1:n, Cascade Delete)
        modelBuilder.Entity<McAntwort>()
            .HasOne(a => a.McFrage)
            .WithMany(f => f.McAntworten)
            .HasForeignKey(a => a.McFrageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pruefung <-> McFrage (m:n)
        modelBuilder.Entity<Pruefung>()
            .HasMany(p => p.McFragen)
            .WithMany(f => f.Pruefungen);
    }
}
