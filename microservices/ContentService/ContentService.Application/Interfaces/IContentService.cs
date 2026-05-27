using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface IContentService
{
    // Section Operations
    Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(Guid courseId);
    Task<Section?> GetSectionByIdAsync(Guid id);
    Task<Section> CreateSectionAsync(Section section);
    Task UpdateSectionAsync(Section section);
    Task DeleteSectionAsync(Guid id);

    // Lesson Operations
    Task<IEnumerable<Lesson>> GetLessonsBySectionIdAsync(Guid sectionId);
    Task<Lesson?> GetLessonByIdAsync(Guid id);
    Task<Lesson> CreateLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(Guid id);
}
