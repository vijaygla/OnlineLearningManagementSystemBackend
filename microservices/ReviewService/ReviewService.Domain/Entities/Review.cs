using SharedKernel.Base;

namespace ReviewService.Domain.Entities;

public class Review : BaseAuditableEntity
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
}
