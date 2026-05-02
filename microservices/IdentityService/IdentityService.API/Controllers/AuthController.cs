using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IdentityService.Application.Interfaces;
using IdentityService.Application.DTOs;
using MassTransit;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message });
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
    {
        var result = await _auth.VerifyOtpAsync(dto.Email, dto.Otp);
        if (result)
        {
            return Ok(new { message = "Email verified successfully." });
        }
        return BadRequest(new { message = "Invalid or expired OTP." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        await _auth.ForgotPasswordAsync(email);
        return Ok(new { message = "If an account exists, a reset OTP has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        var result = await _auth.ResetPasswordAsync(dto);
        if (result)
        {
            return Ok(new { message = "Password reset successfully." });
        }
        return BadRequest(new { message = "Invalid or expired OTP." });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequestDto dto)
    {
        try
        {
            var result = await _auth.LoginWithGoogleAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok("Authorized user");
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto)
    {
        try
        {
            _logger.LogInformation("🔄 Resending verification OTP for: {Email}", dto.Email);
            await _auth.ResendVerificationOtpAsync(dto.Email);
            return Ok(new { message = "OTP sent successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to resend OTP for {Email}: {Message}", dto.Email, ex.Message);
            return StatusCode(500, new { message = "An error occurred while re-sending the verification email. Please ensure the messaging service is available." });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromServices] IdentityService.Infrastructure.Data.AppDbContext dbContext, [FromServices] IBusControl busControl)
    {
        var status = new
        {
            Database = "Unknown",
            RabbitMQ = "Unknown",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            status = status with { Database = canConnect ? "Connected" : "Failed" };
        }
        catch (Exception ex)
        {
            status = status with { Database = $"Error: {ex.Message}" };
        }

        try
        {
            var busStatus = busControl.CheckHealth();
            status = status with { RabbitMQ = busStatus.Status.ToString() };
        }
        catch (Exception ex)
        {
            status = status with { RabbitMQ = $"Error: {ex.Message}" };
        }

        return Ok(status);
    }
}
