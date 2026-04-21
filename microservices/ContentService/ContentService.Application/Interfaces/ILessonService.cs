using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface ILessonService
{
    Task<Lesson?> GetLessonAsync(Guid id);
    Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(Guid courseId);
    Task<Lesson> CreateLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(Guid id);
}
