using CertificateService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CertificateService.Infrastructure.Data;

public class CertificateDbContext : DbContext
{
    public CertificateDbContext(DbContextOptions<CertificateDbContext> options) : base(options) { }

    public DbSet<Certificate> Certificates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CertificateNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StudentName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CourseTitle).IsRequired().HasMaxLength(200);
            
            entity.HasIndex(c => c.CertificateNumber).IsUnique();
            entity.HasIndex(c => new { c.StudentId, c.CourseId }).IsUnique();
        });
    }
}
