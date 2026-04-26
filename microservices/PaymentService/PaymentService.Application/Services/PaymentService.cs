using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Contracts.Events;
using StripeLib = Stripe;

namespace PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository repository,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _logger = logger;
        
        StripeLib.StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<PaymentResponse> CreatePaymentIntentAsync(CreatePaymentRequest request, Guid userId)
    {
        var options = new StripeLib.PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100), // Stripe expects amounts in cents
            Currency = request.Currency,
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId.ToString() },
                { "CourseId", request.CourseId.ToString() }
            }
        };

        var service = new StripeLib.PaymentIntentService();
        var intent = await service.CreateAsync(options);

        var transaction = new PaymentTransaction
        {
            UserId = userId,
            CourseId = request.CourseId,
            Amount = request.Amount,
            Currency = request.Currency,
            StripePaymentIntentId = intent.Id,
            Status = PaymentStatus.Pending
        };

        await _repository.AddAsync(transaction);

        return new PaymentResponse
        {
            TransactionId = transaction.Id,
            ClientSecret = intent.ClientSecret,
            Status = transaction.Status.ToString()
        };
    }

    public async Task ProcessWebhookAsync(string json, string stripeSignature)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            StripeLib.Event stripeEvent;

            // Logic to allow local testing without valid Stripe signatures
            if (webhookSecret == "whsec_test_dummy")
            {
                _logger.LogInformation("Bypassing Stripe signature verification for local testing.");
                stripeEvent = StripeLib.EventUtility.ParseEvent(json);
            }
            else
            {
                stripeEvent = StripeLib.EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
            }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as StripeLib.PaymentIntent;
                if (paymentIntent != null)
                {
                    await HandlePaymentSucceeded(paymentIntent);
                }
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as StripeLib.PaymentIntent;
                if (paymentIntent != null)
                {
                    await HandlePaymentFailed(paymentIntent);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Stripe webhook processing error");
            throw;
        }
    }

    private async Task HandlePaymentSucceeded(StripeLib.PaymentIntent intent)
    {
        var transaction = await _repository.GetByStripePaymentIntentIdAsync(intent.Id);
        if (transaction != null && transaction.Status != PaymentStatus.Succeeded)
        {
            transaction.Status = PaymentStatus.Succeeded;
            await _repository.UpdateAsync(transaction);

            // Publish Event for EnrollmentService
            await _publishEndpoint.Publish(new PaymentCompletedEvent
            {
                UserId = transaction.UserId,
                CourseId = transaction.CourseId,
                Amount = transaction.Amount,
                TransactionId = transaction.Id,
                PaymentDate = DateTime.UtcNow
            });

            _logger.LogInformation("Payment succeeded for Transaction: {TransactionId}", transaction.Id);
        }
    }

    private async Task HandlePaymentFailed(StripeLib.PaymentIntent intent)
    {
        var transaction = await _repository.GetByStripePaymentIntentIdAsync(intent.Id);
        if (transaction != null)
        {
            transaction.Status = PaymentStatus.Failed;
            transaction.ErrorMessage = intent.LastPaymentError?.Message;
            await _repository.UpdateAsync(transaction);
            
            _logger.LogWarning("Payment failed for Transaction: {TransactionId}. Error: {Error}", 
                transaction.Id, transaction.ErrorMessage);
        }
    }
}
