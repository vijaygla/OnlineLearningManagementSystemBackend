using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly ITokenService _token;

    public AuthService(IUserRepository repo, ITokenService token)
    {
        _repo = repo;
        _token = token;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        ValidateRegisterRequest(request);

        var existing = await _repo.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new InvalidOperationException("User already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "Student" : request.Role.Trim()
        };

        await _repo.AddAsync(user);

        return new AuthResponseDto
        {
            Email = user.Email,
            Token = _token.GenerateToken(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        ValidateLoginRequest(request);

        var user = await _repo.GetByEmailAsync(request.Email.Trim());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return new AuthResponseDto
        {
            Email = user.Email,
            Token = _token.GenerateToken(user)
        };
    }

    private static void ValidateRegisterRequest(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters long.");
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
