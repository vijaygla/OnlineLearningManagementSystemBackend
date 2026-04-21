using Microsoft.EntityFrameworkCore;
using CategoryService.Domain.Entities;

namespace CategoryService.Infrastructure.Data;

public class CategoryDbContext : DbContext
{
    public CategoryDbContext(DbContextOptions<CategoryDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasKey(c => c.Id);
        modelBuilder.Entity<Category>().Property(c => c.Name).IsRequired().HasMaxLength(100);
    }
}
