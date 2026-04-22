using Microsoft.EntityFrameworkCore;
using ProgressService.Application.Interfaces;
using ProgressService.Domain.Entities;
using ProgressService.Infrastructure.Data;

namespace ProgressService.Infrastructure.Repositories;

public class ProgressRepository : IProgressRepository
{
    private readonly ProgressDbContext _context;

    public ProgressRepository(ProgressDbContext context)
    {
        _context = context;
    }

    public async Task<UserProgress?> GetProgressAsync(Guid studentId, Guid lessonId)
    {
        return await _context.UserProgresses
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId);
    }

    public async Task<IEnumerable<UserProgress>> GetCourseProgressAsync(Guid studentId, Guid courseId)
    {
        return await _context.UserProgresses
            .Where(p => p.StudentId == studentId && p.CourseId == courseId)
            .ToListAsync();
    }

    public async Task AddAsync(UserProgress progress)
    {
        await _context.UserProgresses.AddAsync(progress);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserProgress progress)
    {
        _context.UserProgresses.Update(progress);
        await _context.SaveChangesAsync();
    }
}
