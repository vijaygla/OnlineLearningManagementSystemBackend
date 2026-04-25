namespace Shared.Contracts.Events;

public record EnrollmentCreatedEvent
{
    public Guid EnrollmentId { get; init; }
    public Guid StudentId { get; init; }
    public Guid CourseId { get; init; }
    public string StudentEmail { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
