using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgressService.Application.DTOs;
using ProgressService.Application.Interfaces;

namespace ProgressService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _service;

    public ProgressController(IProgressService service)
    {
        _service = service;
    }

    [HttpPost("mark-complete")]
    public async Task<IActionResult> MarkComplete(ProgressRequestDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var studentId))
            return BadRequest("Invalid student ID in token.");

        await _service.MarkLessonAsCompleteAsync(studentId, dto);
        return Ok(new { message = "Progress updated successfully." });
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetCourseProgress(Guid courseId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var studentId))
            return BadRequest("Invalid student ID in token.");

        var result = await _service.GetCourseProgressAsync(studentId, courseId);
        return Ok(result);
    }
}
