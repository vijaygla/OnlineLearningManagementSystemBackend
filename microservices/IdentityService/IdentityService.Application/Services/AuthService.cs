using Google.Apis.Auth;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace IdentityService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly ITokenService _token;
    private readonly IConfiguration _config;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repo, ITokenService token, IConfiguration config, IPublishEndpoint publishEndpoint, ILogger<AuthService> logger)
    {
        _repo = repo;
        _token = token;
        _config = config;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        ValidateRegisterRequest(request);

        var existing = await _repo.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new InvalidOperationException("User already exists.");
        }

        // Generate a strict 6-digit OTP
        var otp = new Random().Next(100000, 1000000).ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "Student" : request.Role.Trim(),
            IsEmailVerified = false,
            EmailOtp = otp,
            OtpExpiry = DateTime.UtcNow.AddMinutes(15)
        };

        await _repo.AddAsync(user);

        try
        {
            // Publish UserCreatedEvent with OTP
            _logger.LogInformation("📢 Publishing UserCreatedEvent for {Email} with OTP {Otp}", user.Email, otp);
            await _publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.Name,
                Otp = otp,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            var isDevelopment = _config["ASPNETCORE_ENVIRONMENT"] == "Development" || _config["Environment"] == "Development";
            _logger.LogError(ex, "⚠️ Messaging Error (Registration): {Message}", ex.Message);
            if (!isDevelopment) throw;
        }

        return new AuthResponseDto
        {
            Name = user.Name,
            Email = user.Email,
            Token = _token.GenerateToken(user),
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var user = await _repo.GetByEmailAsync(email);
        if (user == null || user.EmailOtp != otp || user.OtpExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.IsEmailVerified = true;
        user.EmailOtp = null;
        user.OtpExpiry = null;
        await _repo.UpdateAsync(user);
        return true;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        ValidateLoginRequest(request);

        var user = await _repo.GetByEmailAsync(request.Email.Trim());

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("This account was created using Google. Please use Google Login.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // --- MANDATORY: Enforce email verification for all users ---
        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedAccessException("Please verify your email address before logging in.");
        }

        return new AuthResponseDto
        {
            Name = user.Name,
            Email = user.Email,
            Token = _token.GenerateToken(user),
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _repo.GetByEmailAsync(email.Trim());
        if (user == null) return; 

        var otp = new Random().Next(100000, 1000000).ToString();
        user.EmailOtp = otp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
        await _repo.UpdateAsync(user);

        try 
        {
            await _publishEndpoint.Publish(new ForgotPasswordEvent
            {
                Email = user.Email,
                FullName = user.Name,
                Otp = otp,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            var isDevelopment = _config["ASPNETCORE_ENVIRONMENT"] == "Development" || _config["Environment"] == "Development";
            Console.WriteLine($"⚠️ Messaging Error (Forgot Password): {ex.Message}");
            if (!isDevelopment) throw;
        }
    }

    public async Task ResendVerificationOtpAsync(string email)
    {
        var user = await _repo.GetByEmailAsync(email.Trim());
        if (user == null)
        {
            throw new ArgumentException("No account found with this email address.");
        }

        if (user.IsEmailVerified)
        {
            throw new InvalidOperationException("This email address is already verified.");
        }

        var otp = new Random().Next(100000, 1000000).ToString();
        user.EmailOtp = otp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
        await _repo.UpdateAsync(user);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            _logger.LogInformation("📢 Publishing UserCreatedEvent (Resend) for {Email} with OTP {Otp}", user.Email, otp);
            await _publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.Name,
                Otp = otp,
                CreatedAt = DateTime.UtcNow
            }, cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("🕒 Resend OTP timed out while publishing to RabbitMQ for {Email}", user.Email);
            throw new Exception("The messaging service is taking too long to respond. Please try again later.");
        }
        catch (Exception ex)
        {
            var isDevelopment = _config["ASPNETCORE_ENVIRONMENT"] == "Development" || _config["Environment"] == "Development";
            _logger.LogError(ex, "⚠️ Messaging Error (Resend OTP): {Message}", ex.Message);
            if (!isDevelopment) throw;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _repo.GetByEmailAsync(request.Email);
        if (user == null || user.EmailOtp != request.Otp || user.OtpExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.EmailOtp = null;
        user.OtpExpiry = null;
        user.IsEmailVerified = true; // Resetting password via OTP also verifies email
        await _repo.UpdateAsync(user);
        return true;
    }

    public async Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto request)
    {
        try
        {
            var clientId = _config["Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured.");
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            
            var user = await _repo.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                // Register the user if they don't exist
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = payload.Name,
                    Email = payload.Email,
                    PasswordHash = string.Empty, // Google users don't have a password hash in our DB
                    Role = "Student",
                    ProfilePictureUrl = payload.Picture,
                    IsEmailVerified = true
                };
                await _repo.AddAsync(user);

                // Publish UserCreatedEvent for Google users
                await _publishEndpoint.Publish(new UserCreatedEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.Name,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (string.IsNullOrEmpty(user.ProfilePictureUrl) && !string.IsNullOrEmpty(payload.Picture))
            {
                // Update profile picture if user exists but has none
                user.ProfilePictureUrl = payload.Picture;
                await _repo.UpdateAsync(user);
            }

            return new AuthResponseDto
            {
                Name = user.Name,
                Email = user.Email,
                Token = _token.GenerateToken(user),
                Role = user.Role,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid Google token.", ex);
        }
    }

    private static void ValidateRegisterRequest(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !System.Text.RegularExpressions.Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ArgumentException("A valid email is required.");
        }

        var password = request.Password;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.");
        }

        if (!password.Any(char.IsUpper))
        {
            throw new ArgumentException("Password must contain at least one uppercase letter.");
        }

        if (!password.Any(char.IsLower))
        {
            throw new ArgumentException("Password must contain at least one lowercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            throw new ArgumentException("Password must contain at least one number.");
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            throw new ArgumentException("Password must contain at least one special character.");
        }
    }

    private static void ValidateLoginRequest(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.");
        }
    }
}
