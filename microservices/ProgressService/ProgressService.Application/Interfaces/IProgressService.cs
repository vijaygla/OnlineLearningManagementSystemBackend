using ProgressService.Application.DTOs;

namespace ProgressService.Application.Interfaces;

public interface IProgressService
{
    Task MarkLessonAsCompleteAsync(Guid studentId, ProgressRequestDto request);
    Task<CourseProgressResponseDto> GetCourseProgressAsync(Guid studentId, Guid courseId);
}
