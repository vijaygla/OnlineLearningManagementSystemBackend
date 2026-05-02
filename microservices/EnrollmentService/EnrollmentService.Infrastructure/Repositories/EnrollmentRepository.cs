using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentService.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly EnrollmentDbContext _context;

    public EnrollmentRepository(EnrollmentDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await _context.Enrollments.FindAsync(id);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<int> GetCountByCourseIdAsync(Guid courseId)
    {
        return await _context.Enrollments.CountAsync(e => e.CourseId == courseId);
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
    }
}
