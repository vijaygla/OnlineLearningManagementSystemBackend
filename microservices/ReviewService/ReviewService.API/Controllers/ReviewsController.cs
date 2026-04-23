using ReviewService.Application.DTOs;
using ReviewService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ReviewService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateReviewRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var studentId = Guid.Parse(userIdClaim);
        
        try
        {
            var result = await _service.CreateReviewAsync(studentId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateReviewRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var studentId = Guid.Parse(userIdClaim);

        try
        {
            await _service.UpdateReviewAsync(id, studentId, request);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Review not found." }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var studentId = Guid.Parse(userIdClaim);

        try
        {
            await _service.DeleteReviewAsync(id, studentId, roleClaim ?? "");
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Review not found." }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetCourseReviews(Guid courseId)
    {
        var result = await _service.GetCourseReviewsAsync(courseId);
        return Ok(result);
    }

    [HttpGet("course/{courseId}/rating")]
    public async Task<IActionResult> GetCourseRating(Guid courseId)
    {
        var result = await _service.GetCourseRatingAsync(courseId);
        return Ok(result);
    }
}
