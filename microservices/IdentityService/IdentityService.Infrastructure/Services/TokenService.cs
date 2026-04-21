using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly string _key;
    private readonly string? _issuer;
    private readonly string? _audience;

    public TokenService(string key, string? issuer = null, string? audience = null)
    {
        _key = key;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
