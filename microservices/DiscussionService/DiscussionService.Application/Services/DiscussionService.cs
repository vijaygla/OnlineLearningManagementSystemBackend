using DiscussionService.Application.DTOs;
using DiscussionService.Application.Interfaces;
using DiscussionService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DiscussionService.Application.Services;

public class DiscussionService : IDiscussionService
{
    private readonly IDiscussionRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DiscussionService> _logger;

    public DiscussionService(
        IDiscussionRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<DiscussionService> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<IEnumerable<ThreadResponse>> GetCourseThreadsAsync(Guid courseId)
    {
        var threads = await _repository.GetThreadsByCourseAsync(courseId);
        return threads.Select(MapToThreadResponse);
    }

    public async Task<ThreadResponse?> GetThreadDetailsAsync(Guid threadId)
    {
        var thread = await _repository.GetThreadByIdAsync(threadId);
        if (thread == null) return null;

        return MapToThreadResponse(thread);
    }

    public async Task<ThreadResponse> CreateThreadAsync(CreateThreadRequest request, Guid userId, string userName)
    {
        var thread = new DiscussionThread
        {
            Title = request.Title,
            Content = request.Content,
            CourseId = request.CourseId,
            LessonId = request.LessonId,
            AuthorId = userId,
            AuthorName = userName
        };

        await _repository.AddThreadAsync(thread);
        
        _logger.LogInformation("New thread created: {ThreadId} by {UserName}", thread.Id, userName);
        
        return MapToThreadResponse(thread);
    }

    public async Task<CommentResponse> AddCommentAsync(CreateCommentRequest request, Guid userId, string userName)
    {
        var comment = new DiscussionComment
        {
            ThreadId = request.ThreadId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content,
            AuthorId = userId,
            AuthorName = userName
        };

        await _repository.AddCommentAsync(comment);
        
        _logger.LogInformation("New comment added to thread: {ThreadId} by {UserName}", comment.ThreadId, userName);
        
        return MapToCommentResponse(comment);
    }

    public async Task DeleteThreadAsync(Guid threadId, Guid userId, string role)
    {
        var thread = await _repository.GetThreadByIdAsync(threadId);
        if (thread == null) return;

        // Only author or instructor/admin can delete
        if (thread.AuthorId != userId && role != "Instructor" && role != "Admin")
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this thread.");
        }

        await _repository.DeleteThreadAsync(threadId);
    }

    private static ThreadResponse MapToThreadResponse(DiscussionThread thread)
    {
        return new ThreadResponse
        {
            Id = thread.Id,
            Title = thread.Title,
            Content = thread.Content,
            CourseId = thread.CourseId,
            AuthorId = thread.AuthorId,
            AuthorName = thread.AuthorName,
            IsPinned = thread.IsPinned,
            IsClosed = thread.IsClosed,
            CommentCount = thread.CommentCount,
            CreatedAt = thread.CreatedAt,
            Comments = thread.Comments?.Select(MapToCommentResponse).ToList() ?? new()
        };
    }

    private static CommentResponse MapToCommentResponse(DiscussionComment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            ThreadId = comment.ThreadId,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            AuthorId = comment.AuthorId,
            AuthorName = comment.AuthorName,
            CreatedAt = comment.CreatedAt,
            Replies = comment.Replies?.Select(MapToCommentResponse).ToList() ?? new()
        };
    }
}
