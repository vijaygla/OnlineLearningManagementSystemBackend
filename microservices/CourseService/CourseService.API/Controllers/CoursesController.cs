using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using SharedKernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [Authorize] // Should be restricted to Admin
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

public record CreateCourseRequest(string Title, string Description, Guid CategoryId, decimal Price);
public record UpdateCourseRequest(string Title, string Description, decimal Price);
public record UpdateCourseStatusRequest(CourseStatus Status);
