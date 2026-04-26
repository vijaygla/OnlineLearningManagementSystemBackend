using Microsoft.EntityFrameworkCore;
using DiscussionService.Domain.Entities;

namespace DiscussionService.Infrastructure.Data;

public class DiscussionDbContext : DbContext
{
    public DiscussionDbContext(DbContextOptions<DiscussionDbContext> options) : base(options) { }

    public DbSet<DiscussionThread> Threads { get; set; }
    public DbSet<DiscussionComment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<DiscussionThread>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Comments)
                  .WithOne(e => e.Thread)
                  .HasForeignKey(e => e.ThreadId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscussionComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Thread)
                  .WithMany(e => e.Comments)
                  .HasForeignKey(e => e.ThreadId);
                  
            entity.HasMany(e => e.Replies)
                  .WithOne()
                  .HasForeignKey(e => e.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
