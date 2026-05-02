using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto request);
    Task<bool> VerifyOtpAsync(string email, string otp);
    Task ForgotPasswordAsync(string email);
    Task ResendVerificationOtpAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
}
