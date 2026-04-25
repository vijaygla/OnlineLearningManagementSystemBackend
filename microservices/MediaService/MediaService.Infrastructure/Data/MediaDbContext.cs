using MediaService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Data
{
    public class MediaDbContext : DbContext
    {
        public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
        {
        }

        public DbSet<Media> Media { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
            });
        }
    }
}
