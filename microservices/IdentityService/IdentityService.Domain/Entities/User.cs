namespace IdentityService.Domain.Entities;

public class User
{
    // globally unique id guid
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Student";

    public string? ProfilePictureUrl { get; set; }

    public bool IsEmailVerified { get; set; } = false;

    public string? EmailOtp { get; set; }

    public DateTime? OtpExpiry { get; set; }
}
