namespace ProgressService.Application.DTOs;

public class ProgressRequestDto
{
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }
    public bool IsCompleted { get; set; }
}

public class ProgressResponseDto
{
    public Guid LessonId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CourseProgressResponseDto
{
    public Guid CourseId { get; set; }
    public double CompletionPercentage { get; set; }
    public List<ProgressResponseDto> CompletedLessons { get; set; } = new();
}
