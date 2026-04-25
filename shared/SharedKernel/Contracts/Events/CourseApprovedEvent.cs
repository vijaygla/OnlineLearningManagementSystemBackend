namespace Shared.Contracts.Events;

public record CourseApprovedEvent
{
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string InstructorEmail { get; init; } = string.Empty;
    public DateTime ApprovedAt { get; init; }
}
