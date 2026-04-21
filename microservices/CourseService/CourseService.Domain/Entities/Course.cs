using SharedKernel.Base;
using SharedKernel.Enums;

namespace CourseService.Domain.Entities;

public class Course : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid InstructorId { get; set; }
    public decimal Price { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Pending;
}
