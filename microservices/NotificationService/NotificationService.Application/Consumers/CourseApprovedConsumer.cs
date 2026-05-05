using MassTransit;
using Shared.Contracts.Events;
using NotificationService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Consumers;

public class CourseApprovedConsumer : IConsumer<CourseApprovedEvent>
{
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CourseApprovedConsumer> _logger;

    public CourseApprovedConsumer(IEmailService emailService, INotificationService notificationService, ILogger<CourseApprovedConsumer> logger)
    {
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CourseApprovedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing CourseApprovedEvent for CourseId: {CourseId}", message.CourseId);

        // 1. Send Email
        var subject = "Congratulations! Your course has been approved";
        var body = $@"
            <h1>Course Approved</h1>
            <p>Hello,</p>
            <p>Your course <strong>{message.CourseTitle}</strong> has been reviewed and approved by our moderation team.</p>
            <p>It is now live on the platform and students can start enrolling.</p>
            <br/>
            <p>Best regards,<br/>The OLMS Team</p>";

        await _emailService.SendEmailAsync(message.InstructorEmail, subject, body);

        // 2. Create In-App Notification
        await _notificationService.CreateNotificationAsync(new Notification
        {
            UserId = message.InstructorId,
            Title = "Course Approved",
            Message = $"Your course '{message.CourseTitle}' has been approved and is now live.",
            Type = "Success",
            RelatedId = message.CourseId.ToString()
        });
        
        _logger.LogInformation("Approval notification sent to instructor for course {CourseId}", message.CourseId);
    }
}
