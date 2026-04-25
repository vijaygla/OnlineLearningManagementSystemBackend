using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.Entities;

public class UserProfile
{
    [Key]
    public Guid UserId { get; set; } // Matches Identity UserId
    
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public string? ProfilePictureUrl { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public string? Location { get; set; }
    
    // many things from my side:
    public List<string> Skills { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    // Statistics
    public int CoursesCompletedCount { get; set; }
    public double TotalLearningHours { get; set; }

    // Navigation properties
    public UserPreference Preference { get; set; } = null!;
    public ICollection<SocialLink> SocialLinks { get; set; } = new List<SocialLink>();
    public ICollection<EnrolledCourse> EnrolledCourses { get; set; } = new List<EnrolledCourse>();
}
