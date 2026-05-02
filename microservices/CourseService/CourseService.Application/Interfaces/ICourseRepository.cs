using CourseService.Domain.Entities;

namespace CourseService.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<IEnumerable<Course>> GetByStatusAsync(SharedKernel.Enums.CourseStatus status);
    Task<IEnumerable<Course>> GetByInstructorIdAsync(Guid instructorId);
    Task AddAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(Guid id);
}
