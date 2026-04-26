using PaymentService.Domain.Enums;
using SharedKernel.Base;

namespace PaymentService.Domain.Entities;

public class PaymentTransaction : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? StripePaymentIntentId { get; set; }
    public string? StripeSessionId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ErrorMessage { get; set; }
}
