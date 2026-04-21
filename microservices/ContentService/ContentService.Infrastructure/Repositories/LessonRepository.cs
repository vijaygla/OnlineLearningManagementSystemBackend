using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ContentService.Infrastructure.Data;

namespace ContentService.Infrastructure.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly ContentDbContext _context;
    public LessonRepository(ContentDbContext context) => _context = context;

    public async Task<Lesson?> GetByIdAsync(Guid id) => await _context.Lessons.FindAsync(id);

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId) => 
        await _context.Lessons.Where(l => l.CourseId == courseId).OrderBy(l => l.Order).ToListAsync();

    public async Task AddAsync(Lesson lesson) { await _context.Lessons.AddAsync(lesson); await _context.SaveChangesAsync(); }

    public async Task UpdateAsync(Lesson lesson) { _context.Update(lesson); await _context.SaveChangesAsync(); }

    public async Task DeleteAsync(Guid id) 
    { 
        var lesson = await GetByIdAsync(id);
        if (lesson != null) { _context.Lessons.Remove(lesson); await _context.SaveChangesAsync(); }
    }
}
