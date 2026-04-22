using ProgressService.Application.DTOs;
using ProgressService.Application.Interfaces;
using ProgressService.Domain.Entities;

namespace ProgressService.Application.Services;

public class ProgressService : IProgressService
{
    private readonly IProgressRepository _repo;

    public ProgressService(IProgressRepository repo)
    {
        _repo = repo;
    }

    public async Task MarkLessonAsCompleteAsync(Guid studentId, ProgressRequestDto request)
    {
        var progress = await _repo.GetProgressAsync(studentId, request.LessonId);

        if (progress == null)
        {
            progress = new UserProgress
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                CourseId = request.CourseId,
                LessonId = request.LessonId,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? DateTime.UtcNow : null,
                CreatedBy = studentId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(progress);
        }
        else
        {
            progress.IsCompleted = request.IsCompleted;
            progress.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;
            progress.LastModifiedBy = studentId.ToString();
            progress.LastModifiedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(progress);
        }
        
        // Note: In a real scenario, we might want to publish an event here 
        // so the EnrollmentService can update the overall ProgressPercentage.
    }

    public async Task<CourseProgressResponseDto> GetCourseProgressAsync(Guid studentId, Guid courseId)
    {
        var userProgress = await _repo.GetCourseProgressAsync(studentId, courseId);
        
        var response = new CourseProgressResponseDto
        {
            CourseId = courseId,
            CompletedLessons = userProgress.Select(p => new ProgressResponseDto
            {
                LessonId = p.LessonId,
                IsCompleted = p.IsCompleted,
                CompletedAt = p.CompletedAt
            }).ToList()
        };

        // For now, we return 0 for percentage or calculate based on recorded items.
        // Accurate percentage requires knowing the total number of lessons in the course.
        int completedCount = response.CompletedLessons.Count(l => l.IsCompleted);
        response.CompletionPercentage = response.CompletedLessons.Count > 0 
            ? (double)completedCount / response.CompletedLessons.Count * 100 
            : 0;

        return response;
    }
}
