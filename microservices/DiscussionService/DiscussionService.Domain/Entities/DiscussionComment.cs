using SharedKernel.Base;

namespace DiscussionService.Domain.Entities;

public class DiscussionComment : BaseAuditableEntity
{
    public Guid ThreadId { get; set; }
    public DiscussionThread Thread { get; set; } = null!;
    
    public Guid? ParentCommentId { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    
    public int Likes { get; set; } = 0;

    public ICollection<DiscussionComment> Replies { get; set; } = new List<DiscussionComment>();
}
