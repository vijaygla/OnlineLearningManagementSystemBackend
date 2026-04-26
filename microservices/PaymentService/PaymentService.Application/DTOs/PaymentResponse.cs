namespace PaymentService.Application.DTOs;

public record PaymentResponse
{
    public Guid TransactionId { get; init; }
    public string? ClientSecret { get; init; }
    public string Status { get; init; } = string.Empty;
}
