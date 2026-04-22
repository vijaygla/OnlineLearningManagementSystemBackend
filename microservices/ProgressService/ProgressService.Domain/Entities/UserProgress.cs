using SharedKernel.Base;

namespace ProgressService.Domain.Entities;

public class UserProgress : BaseAuditableEntity
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
