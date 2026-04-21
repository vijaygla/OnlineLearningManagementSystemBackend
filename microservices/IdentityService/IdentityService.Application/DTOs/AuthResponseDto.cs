namespace IdentityService.Application.DTOs;

public class AuthResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
