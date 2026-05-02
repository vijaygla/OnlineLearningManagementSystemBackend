namespace IdentityService.Application.DTOs;

public record ResetPasswordRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Otp { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
