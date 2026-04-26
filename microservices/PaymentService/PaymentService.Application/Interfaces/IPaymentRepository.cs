using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentTransaction?> GetByIdAsync(Guid id);
    Task<PaymentTransaction?> GetByStripePaymentIntentIdAsync(string paymentIntentId);
    Task AddAsync(PaymentTransaction transaction);
    Task UpdateAsync(PaymentTransaction transaction);
}
