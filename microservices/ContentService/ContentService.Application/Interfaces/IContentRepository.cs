using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface IContentRepository
{
    // Section Operations
    Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(Guid courseId);
    Task<Section?> GetSectionByIdAsync(Guid id);
    Task AddSectionAsync(Section section);
    Task UpdateSectionAsync(Section section);
    Task DeleteSectionAsync(Guid id);

    // Lesson Operations
    Task<IEnumerable<Lesson>> GetLessonsBySectionIdAsync(Guid sectionId);
    Task<Lesson?> GetLessonByIdAsync(Guid id);
    Task AddLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(Guid id);
}
