using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePaymentIntentAsync(CreatePaymentRequest request, Guid userId);
    Task ProcessWebhookAsync(string json, string stripeSignature);
}
