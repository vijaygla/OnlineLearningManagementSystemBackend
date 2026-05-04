using MassTransit;
using Shared.Contracts.Events;
using NotificationService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace NotificationService.Application.Consumers;

public class CourseApprovedConsumer : IConsumer<CourseApprovedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<CourseApprovedConsumer> _logger;

    public CourseApprovedConsumer(IEmailService emailService, ILogger<CourseApprovedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CourseApprovedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing CourseApprovedEvent for CourseId: {CourseId}", message.CourseId);

        var subject = "Congratulations! Your course has been approved";
        var body = $@"
            <h1>Course Approved</h1>
            <p>Hello,</p>
            <p>Your course <strong>{message.CourseTitle}</strong> has been reviewed and approved by our moderation team.</p>
            <p>It is now live on the platform and students can start enrolling.</p>
            <br/>
            <p>Best regards,<br/>The OLMS Team</p>";

        await _emailService.SendEmailAsync(message.InstructorEmail, subject, body);
        
        _logger.LogInformation("Approval notification sent to instructor for course {CourseId}", message.CourseId);
    }
}
