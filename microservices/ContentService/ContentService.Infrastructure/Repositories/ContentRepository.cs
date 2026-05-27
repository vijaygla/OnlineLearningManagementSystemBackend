using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ContentService.Infrastructure.Data;

namespace ContentService.Infrastructure.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly ContentDbContext _context;

    public ContentRepository(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(Guid courseId)
    {
        return await _context.Sections
            .Include(s => s.Lessons)
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.Order)
            .ToListAsync();
    }

    public async Task<Section?> GetSectionByIdAsync(Guid id)
    {
        return await _context.Sections
            .Include(s => s.Lessons)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddSectionAsync(Section section)
    {
        section.Id = Guid.NewGuid();
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSectionAsync(Section section)
    {
        _context.Entry(section).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSectionAsync(Guid id)
    {
        var section = await _context.Sections.FindAsync(id);
        if (section != null)
        {
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Lesson>> GetLessonsBySectionIdAsync(Guid sectionId)
    {
        return await _context.Lessons
            .Where(l => l.SectionId == sectionId)
            .OrderBy(l => l.Order)
            .ToListAsync();
    }

    public async Task<Lesson?> GetLessonByIdAsync(Guid id)
    {
        return await _context.Lessons.FindAsync(id);
    }

    public async Task AddLessonAsync(Lesson lesson)
    {
        lesson.Id = Guid.NewGuid();
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLessonAsync(Lesson lesson)
    {
        _context.Entry(lesson).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLessonAsync(Guid id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson != null)
        {
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
        }
    }
}
