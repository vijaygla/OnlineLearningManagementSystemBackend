using CertificateService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CertificateService.Infrastructure.Data;

public class CertificateDbContext : DbContext
{
    public CertificateDbContext(DbContextOptions<CertificateDbContext> options) : base(options) { }

    public DbSet<Certificate> Certificates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Certificate>()
            .HasIndex(c => c.CertificateNumber)
            .IsUnique();

        modelBuilder.Entity<Certificate>()
            .HasIndex(c => new { c.StudentId, c.CourseId })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
