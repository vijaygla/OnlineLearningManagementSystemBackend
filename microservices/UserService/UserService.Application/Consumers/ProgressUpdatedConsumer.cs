using MassTransit;
using Shared.Contracts.Events;
using UserService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace UserService.Application.Consumers;

public class ProgressUpdatedConsumer : IConsumer<ProgressUpdatedEvent>
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<ProgressUpdatedConsumer> _logger;

    public ProgressUpdatedConsumer(IUserProfileRepository repository, ILogger<ProgressUpdatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProgressUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ProgressUpdatedEvent for UserId: {UserId}, CourseId: {CourseId}", message.UserId, message.CourseId);

        var profile = await _repository.GetByIdAsync(message.UserId);
        if (profile == null)
        {
            _logger.LogWarning("UserProfile for UserId: {UserId} not found. Cannot update statistics.", message.UserId);
            return;
        }

        // Update total hours spent
        profile.TotalLearningHours += message.AdditionalHoursSpent;

        // Find the enrolled course record
        var enrolledCourse = profile.EnrolledCourses.FirstOrDefault(c => c.CourseId == message.CourseId);
        
        // If course is newly completed, update the record and the profile counter
        if (message.IsCompleted && (enrolledCourse == null || !enrolledCourse.IsCompleted))
        {
            if (enrolledCourse != null)
            {
                enrolledCourse.IsCompleted = true;
                enrolledCourse.CompletedAt = DateTime.UtcNow;
            }
            
            profile.CoursesCompletedCount++;
            _logger.LogInformation("UserId: {UserId} completed CourseId: {CourseId}. Total completions: {Count}", 
                message.UserId, message.CourseId, profile.CoursesCompletedCount);
        }

        profile.LastUpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(profile);
    }
}
