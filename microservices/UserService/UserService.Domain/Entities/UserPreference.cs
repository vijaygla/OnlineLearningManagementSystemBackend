using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities;

public class UserPreference
{
    [Key, ForeignKey("UserProfile")]
    public Guid UserId { get; set; }
    
    public string Language { get; set; } = "English";
    
    public string Theme { get; set; } = "Light";
    
    public bool EmailNotifications { get; set; } = true;
    
    public bool PushNotifications { get; set; } = true;
    
    public bool MarketingEmails { get; set; } = false;

    // Navigation property
    public UserProfile UserProfile { get; set; } = null!;
}
