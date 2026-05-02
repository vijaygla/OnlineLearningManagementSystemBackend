using CourseService.Domain.Entities;
using SharedKernel.Enums;

namespace CourseService.Application.Interfaces;

public interface ICourseService
{
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<IEnumerable<Course>> GetCoursesByStatusAsync(CourseStatus status);
    Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId);
    Task<Course> CreateCourseAsync(string title, string description, Guid categoryId, Guid instructorId, decimal price);
    Task UpdateCourseAsync(Guid id, string title, string description, decimal price);
    Task UpdateCourseStatusAsync(Guid id, CourseStatus status);
    Task DeleteCourseAsync(Guid id);
}
