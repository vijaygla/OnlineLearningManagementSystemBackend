using MassTransit;
using Shared.Contracts.Events;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace UserService.Application.Consumers;

public class EnrollmentCreatedConsumer : IConsumer<EnrollmentCreatedEvent>
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<EnrollmentCreatedConsumer> _logger;

    public EnrollmentCreatedConsumer(IUserProfileRepository repository, ILogger<EnrollmentCreatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EnrollmentCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received EnrollmentCreatedEvent for StudentId: {StudentId}, CourseId: {CourseId}", message.StudentId, message.CourseId);

        var profile = await _repository.GetByIdAsync(message.StudentId);
        if (profile == null)
        {
            _logger.LogWarning("UserProfile for StudentId: {StudentId} not found. Cannot add enrolled course.", message.StudentId);
            return;
        }

        // Check if already exists to avoid duplicates (idempotency)
        if (profile.EnrolledCourses.Any(e => e.CourseId == message.CourseId))
        {
            _logger.LogInformation("Student {StudentId} already has course {CourseId} in their profile.", message.StudentId, message.CourseId);
            return;
        }

        profile.EnrolledCourses.Add(new EnrolledCourse
        {
            UserId = message.StudentId,
            CourseId = message.CourseId,
            CourseName = message.CourseName,
            EnrolledAt = message.CreatedAt == default ? DateTime.UtcNow : message.CreatedAt
        });

        await _repository.UpdateAsync(profile);
        _logger.LogInformation("Enrolled course {CourseName} added to profile of StudentId: {StudentId}", message.CourseName, message.StudentId);
    }
}
