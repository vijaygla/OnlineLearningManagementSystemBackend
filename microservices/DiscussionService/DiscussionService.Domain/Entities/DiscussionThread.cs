using SharedKernel.Base;

namespace DiscussionService.Domain.Entities;

public class DiscussionThread : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public Guid CourseId { get; set; }
    public Guid? LessonId { get; set; }
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    
    public bool IsPinned { get; set; } = false;
    public bool IsClosed { get; set; } = false;
    
    public int Views { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public ICollection<DiscussionComment> Comments { get; set; } = new List<DiscussionComment>();
}
