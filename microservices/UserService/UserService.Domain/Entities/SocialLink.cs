using System.ComponentModel.DataAnnotations;

namespace UserService.Domain.Entities;

public class SocialLink
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    public string Platform { get; set; } = string.Empty; // e.g. LinkedIn, Twitter, GitHub
    
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    // Navigation property
    public UserProfile UserProfile { get; set; } = null!;
}
