using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId);
    Task AddAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task DeleteAsync(Guid id);
}
