using Microsoft.EntityFrameworkCore;
using ContentService.Domain.Entities;

namespace ContentService.Infrastructure.Data;

public class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) { }

    public DbSet<Lesson> Lessons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Lesson>().HasKey(l => l.Id);
        modelBuilder.Entity<Lesson>().Property(l => l.Title).IsRequired().HasMaxLength(200);
    }
}
