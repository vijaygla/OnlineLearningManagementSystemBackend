using SharedKernel.Enums;

namespace EnrollmentService.Application.DTOs;

public class EnrollmentRequestDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
}

public class EnrollmentResponseDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public EnrollmentStatus Status { get; set; }
    public double ProgressPercentage { get; set; }
}
