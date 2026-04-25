namespace UserService.Application.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public int CoursesCompletedCount { get; set; }
    public double TotalLearningHours { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; } = new();
    public List<EnrolledCourseDto> EnrolledCourses { get; set; } = new();
}
