using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.Entities;

public class EnrolledCourse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation property
    public UserProfile UserProfile { get; set; } = null!;
}
