using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;

namespace ContentService.Application.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _repo;
    public LessonService(ILessonRepository repo) => _repo = repo;

    public async Task<Lesson?> GetLessonAsync(Guid id) => await _repo.GetByIdAsync(id);

    public async Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(Guid courseId) => await _repo.GetByCourseIdAsync(courseId);

    public async Task<Lesson> CreateLessonAsync(Lesson lesson) { await _repo.AddAsync(lesson); return lesson; }

    public async Task UpdateLessonAsync(Lesson lesson) => await _repo.UpdateAsync(lesson);

    public async Task DeleteLessonAsync(Guid id) => await _repo.DeleteAsync(id);
}
