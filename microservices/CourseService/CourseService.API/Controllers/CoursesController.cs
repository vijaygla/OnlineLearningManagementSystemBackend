using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using SharedKernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using System.Text.Json.Serialization;

namespace CourseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("instructor")]
    [Authorize]
    public async Task<IActionResult> GetByInstructor()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var instructorId = Guid.Parse(userIdClaim);
        var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
        return Ok(courses);
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByStatus(CourseStatus status)
    {
        var courses = await _courseService.GetCoursesByStatusAsync(status);
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound();
        return Ok(course);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        // In a real scenario, instructorId should come from the JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        
        var instructorId = Guid.Parse(userIdClaim);

        var course = await _courseService.CreateCourseAsync(
            request.Title, 
            request.Description, 
            request.CategoryId, 
            instructorId, 
            request.Price);
            
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest request)
    {
        await _courseService.UpdateCourseAsync(id, request.Title, request.Description, request.Price);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCourseStatusRequest request)
    {
        await _courseService.UpdateCourseStatusAsync(id, request.Status);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _courseService.DeleteCourseAsync(id);
        return NoContent();
    }
}

public record CreateCourseRequest(
    [property: JsonPropertyName("title")] string Title, 
    [property: JsonPropertyName("description")] string Description, 
    [property: JsonPropertyName("categoryId")] Guid CategoryId, 
    [property: JsonPropertyName("price")] decimal Price);

public record UpdateCourseRequest(
    [property: JsonPropertyName("title")] string Title, 
    [property: JsonPropertyName("description")] string Description, 
    [property: JsonPropertyName("price")] decimal Price);

public record UpdateCourseStatusRequest(
    [property: JsonPropertyName("status")] CourseStatus Status);
