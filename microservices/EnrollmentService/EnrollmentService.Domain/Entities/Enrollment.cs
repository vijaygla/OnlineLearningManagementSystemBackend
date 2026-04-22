using SharedKernel.Base;
using SharedKernel.Enums;

namespace EnrollmentService.Domain.Entities;

public class Enrollment : BaseAuditableEntity
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public double ProgressPercentage { get; set; } = 0;
}
