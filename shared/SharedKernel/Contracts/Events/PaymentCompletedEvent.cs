namespace Shared.Contracts.Events;

public record PaymentCompletedEvent
{
    public Guid TransactionId { get; init; }
    public Guid UserId { get; init; }
    public Guid CourseId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaymentDate { get; init; }
}
