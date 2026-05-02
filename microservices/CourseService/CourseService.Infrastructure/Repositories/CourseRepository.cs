using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using CourseService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly CourseDbContext _context;

    public CourseRepository(CourseDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await _context.Courses.FindAsync(id);
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetByStatusAsync(SharedKernel.Enums.CourseStatus status)
    {
        return await _context.Courses.Where(c => c.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetByInstructorIdAsync(Guid instructorId)
    {
        return await _context.Courses.Where(c => c.InstructorId == instructorId).ToListAsync();
    }

    public async Task AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}
