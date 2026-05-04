using MassTransit;
using Shared.Contracts.Events;
using SearchService.Application.Interfaces;
using SearchService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SearchService.Application.Consumers;

public class CourseApprovedConsumer : IConsumer<CourseApprovedEvent>
{
    private readonly ISearchService _searchService;
    private readonly ILogger<CourseApprovedConsumer> _logger;

    public CourseApprovedConsumer(ISearchService searchService, ILogger<CourseApprovedConsumer> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CourseApprovedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing CourseApprovedEvent for CourseId: {CourseId}", message.CourseId);

        var searchIndex = new CourseSearchIndex
        {
            Id = message.CourseId.ToString(),
            Title = message.CourseTitle,
            Description = message.CourseDescription,
            Price = message.Price,
            ThumbnailUrl = message.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow,
            // These would ideally be fetched or included in the event
            CategoryName = "Development", 
            InstructorName = "Expert Instructor"
        };

        await _searchService.UpsertCourseIndexAsync(searchIndex);
        _logger.LogInformation("Successfully indexed course {CourseId} in search engine", message.CourseId);
    }
}
