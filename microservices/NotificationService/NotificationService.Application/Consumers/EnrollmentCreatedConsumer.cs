using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

public class EnrollmentCreatedConsumer : IConsumer<EnrollmentCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EnrollmentCreatedConsumer> _logger;

    public EnrollmentCreatedConsumer(IEmailService emailService, ILogger<EnrollmentCreatedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EnrollmentCreatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing enrollment for student: {StudentEmail}, course: {CourseName}", @event.StudentEmail, @event.CourseName);
        
        try
        {
            string subject = "Welcome to the Course!";
            string body = $"<h1>Enrollment Confirmed</h1>" +
                          $"<p>Hello,</p>" +
                          $"<p>You have successfully enrolled in <strong>{@event.CourseName}</strong>.</p>" +
                          $"<p>Happy Learning!</p>";

            await _emailService.SendEmailAsync(@event.StudentEmail, subject, body);
            _logger.LogInformation("Notification email sent successfully to {StudentEmail}", @event.StudentEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification email to {StudentEmail}", @event.StudentEmail);
            throw; // Re-throw to let MassTransit handle retries
        }
    }
}
