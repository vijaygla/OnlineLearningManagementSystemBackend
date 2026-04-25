using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs;

public class UpdateUserProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public string? ProfilePictureUrl { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public string? Location { get; set; }
    
    public List<string> Skills { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public List<SocialLinkDto> SocialLinks { get; set; } = new();
}
