using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.GetUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        return Ok(profile);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProfileById(Guid id)
    {
        var profile = await _userService.GetUserProfileAsync(id);
        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profile = await _userService.UpdateUserProfileAsync(userId, dto);
        if (profile == null)
        {
            return NotFound("User profile not found.");
        }

        return Ok(profile);
    }

    [HttpPost("profile/picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _userService.UploadProfilePictureAsync(userId, file);
            if (result == null)
            {
                return BadRequest("Failed to upload profile picture.");
            }

            return Ok(new { profilePictureUrl = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        var success = await _userService.DeleteUserAccountAsync(userId);
        if (!success)
        {
            return NotFound("User profile not found.");
        }

        return Ok("User account and profile deleted successfully.");
    }
}
