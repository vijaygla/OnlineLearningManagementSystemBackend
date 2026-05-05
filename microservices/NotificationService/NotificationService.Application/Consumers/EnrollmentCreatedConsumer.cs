using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

public class EnrollmentCreatedConsumer : IConsumer<EnrollmentCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<EnrollmentCreatedConsumer> _logger;

    public EnrollmentCreatedConsumer(IEmailService emailService, INotificationService notificationService, ILogger<EnrollmentCreatedConsumer> logger)
    {
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EnrollmentCreatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing enrollment for student: {StudentEmail}, course: {CourseName}", @event.StudentEmail, @event.CourseName);
        
        try
        {
            // 1. Send Email
            string subject = "Welcome to the Course!";
            string body = $"<h1>Enrollment Confirmed</h1>" +
                          $"<p>Hello,</p>" +
                          $"<p>You have successfully enrolled in <strong>{@event.CourseName}</strong>.</p>" +
                          $"<p>Happy Learning!</p>";

            await _emailService.SendEmailAsync(@event.StudentEmail, subject, body);

            // 2. Create In-App Notification
            await _notificationService.CreateNotificationAsync(new Notification
            {
                UserId = @event.StudentId,
                Title = "New Enrollment",
                Message = $"You have successfully enrolled in {@event.CourseName}.",
                Type = "Success",
                RelatedId = @event.CourseId.ToString()
            });

            _logger.LogInformation("Notification email and in-app alert sent successfully to {StudentEmail}", @event.StudentEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process enrollment notification for {StudentEmail}", @event.StudentEmail);
            throw;
        }
    }
}
