namespace Shared.Contracts.Events;

public record UserDeletedEvent
{
    public Guid UserId { get; init; }
    public DateTime DeletedAt { get; init; }
}
