using System.Security.Claims;
using EnrollmentService.Application.DTOs;
using EnrollmentService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnrollmentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentController(IEnrollmentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(EnrollmentRequestDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var emailClaim = User.FindFirst(ClaimTypes.Email);

        if (userIdClaim == null || emailClaim == null) return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var studentId))
            return BadRequest("Invalid student ID in token.");

        try
        {
            var result = await _service.EnrollStudentAsync(studentId, emailClaim.Value, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-enrollments")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var studentId))
            return BadRequest("Invalid student ID in token.");

        var results = await _service.GetStudentEnrollmentsAsync(studentId);
        return Ok(results);
    }

    [HttpGet("course/{courseId}/count")]
    [AllowAnonymous] // Allow instructors or anyone to see enrollment count for a course
    public async Task<IActionResult> GetCourseEnrollmentCount(Guid courseId)
    {
        var count = await _service.GetEnrollmentCountAsync(courseId);
        return Ok(count);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetEnrollmentDetailsAsync(id);
        if (result == null) return NotFound();

        // Security check: Only the student themselves or an admin should see enrollment details
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userRoleClaim = User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null) return Unauthorized();

        if (result.StudentId.ToString() != userIdClaim.Value && userRoleClaim?.Value != "Admin")
            return Forbid();

        return Ok(result);
    }
}
