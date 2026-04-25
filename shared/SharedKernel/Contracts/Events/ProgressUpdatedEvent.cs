namespace Shared.Contracts.Events;

public record ProgressUpdatedEvent
{
    public Guid UserId { get; init; }
    public Guid CourseId { get; init; }
    public double ProgressPercentage { get; init; }
    public bool IsCompleted { get; init; }
    public double AdditionalHoursSpent { get; init; }
}
