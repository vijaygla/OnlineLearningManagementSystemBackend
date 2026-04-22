using Microsoft.EntityFrameworkCore;
using ProgressService.Domain.Entities;

namespace ProgressService.Infrastructure.Data;

public class ProgressDbContext : DbContext
{
    public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options) { }

    public DbSet<UserProgress> UserProgresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<UserProgress>()
            .HasIndex(p => new { p.StudentId, p.LessonId })
            .IsUnique();
    }
}
