using ProgressService.Domain.Entities;

namespace ProgressService.Application.Interfaces;

public interface IProgressRepository
{
    Task<UserProgress?> GetProgressAsync(Guid studentId, Guid lessonId);
    Task<IEnumerable<UserProgress>> GetCourseProgressAsync(Guid studentId, Guid courseId);
    Task AddAsync(UserProgress progress);
    Task UpdateAsync(UserProgress progress);
}
