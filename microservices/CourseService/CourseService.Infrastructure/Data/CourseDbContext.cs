using Microsoft.EntityFrameworkCore;
using CourseService.Domain.Entities;

namespace CourseService.Infrastructure.Data;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options) { }

    public DbSet<Course> Courses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Course>().HasKey(c => c.Id);
        modelBuilder.Entity<Course>().Property(c => c.Title).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Course>().Property(c => c.Price).HasPrecision(18, 2);
    }
}
