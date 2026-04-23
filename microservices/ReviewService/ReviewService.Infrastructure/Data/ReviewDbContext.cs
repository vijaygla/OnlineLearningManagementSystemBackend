using ReviewService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ReviewService.Infrastructure.Data;

public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.StudentId, r.CourseId })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
