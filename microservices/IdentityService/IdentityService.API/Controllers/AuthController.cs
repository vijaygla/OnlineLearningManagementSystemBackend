using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IdentityService.Application.Interfaces;
using IdentityService.Application.DTOs;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        try
        {
            var result = await _auth.RegisterAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        try
        {
            var result = await _auth.LoginAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok("Authorized user");
    }
}
