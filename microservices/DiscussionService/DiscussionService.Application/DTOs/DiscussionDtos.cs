namespace DiscussionService.Application.DTOs;

public record CreateThreadRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CourseId { get; init; }
    public Guid? LessonId { get; init; }
}

public record CreateCommentRequest
{
    public Guid ThreadId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public record ThreadResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid CourseId { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public bool IsPinned { get; init; }
    public bool IsClosed { get; init; }
    public int CommentCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<CommentResponse> Comments { get; init; } = new();
}

public record CommentResponse
{
    public Guid Id { get; init; }
    public Guid ThreadId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<CommentResponse> Replies { get; init; } = new();
}
