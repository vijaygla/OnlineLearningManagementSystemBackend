using SharedKernel.Base;

namespace ContentService.Domain.Entities;

public class Section : BaseAuditableEntity
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    
    // Navigation property
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Lesson>? Lessons { get; set; } = new List<Lesson>();
}
