using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;

namespace ContentService.Application.Services;

public class ContentServiceImplementation : IContentService
{
    private readonly IContentRepository _repo;

    public ContentServiceImplementation(IContentRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(Guid courseId) => await _repo.GetSectionsByCourseIdAsync(courseId);
    public async Task<Section?> GetSectionByIdAsync(Guid id) => await _repo.GetSectionByIdAsync(id);
    public async Task<Section> CreateSectionAsync(Section section) { await _repo.AddSectionAsync(section); return section; }
    public async Task UpdateSectionAsync(Section section) => await _repo.UpdateSectionAsync(section);
    public async Task DeleteSectionAsync(Guid id) => await _repo.DeleteSectionAsync(id);

    public async Task<IEnumerable<Lesson>> GetLessonsBySectionIdAsync(Guid sectionId) => await _repo.GetLessonsBySectionIdAsync(sectionId);
    public async Task<Lesson?> GetLessonByIdAsync(Guid id) => await _repo.GetLessonByIdAsync(id);
    public async Task<Lesson> CreateLessonAsync(Lesson lesson) { await _repo.AddLessonAsync(lesson); return lesson; }
    public async Task UpdateLessonAsync(Lesson lesson) => await _repo.UpdateLessonAsync(lesson);
    public async Task DeleteLessonAsync(Guid id) => await _repo.DeleteLessonAsync(id);
}
