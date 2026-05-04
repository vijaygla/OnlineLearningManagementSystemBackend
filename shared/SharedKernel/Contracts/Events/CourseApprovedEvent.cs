using System;

namespace Shared.Contracts.Events;

public record CourseApprovedEvent
{
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string CourseDescription { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Guid InstructorId { get; init; }
    public string InstructorEmail { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string ThumbnailUrl { get; init; } = string.Empty;
    public DateTime ApprovedAt { get; init; }
}
