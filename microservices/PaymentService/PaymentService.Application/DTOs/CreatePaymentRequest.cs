namespace PaymentService.Application.DTOs;

public record CreatePaymentRequest
{
    public Guid CourseId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "usd";
}
