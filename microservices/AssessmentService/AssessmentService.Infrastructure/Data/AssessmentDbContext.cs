using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Data;

public class AssessmentDbContext : DbContext
{
    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options) { }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuizSubmission> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Questions)
            .WithOne(q => q.Quiz)
            .HasForeignKey(q => q.QuizId);

        base.OnModelCreating(modelBuilder);
    }
}
