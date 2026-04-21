using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _service;
    public LessonsController(ILessonService service) => _service = service;

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetByCourse(Guid courseId) => Ok(await _service.GetLessonsByCourseAsync(courseId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var lesson = await _service.GetLessonAsync(id);
        return lesson != null ? Ok(lesson) : NotFound();
    }

    [HttpPost]
    [Authorize] // Instructors only (Roles can be checked in claims)
    public async Task<IActionResult> Create(CreateLessonRequest request)
    {
        var lesson = new Lesson
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            ContentUrl = request.ContentUrl,
            ContentType = request.ContentType,
            Order = request.Order
        };
        var created = await _service.CreateLessonAsync(lesson);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateLessonRequest request)
    {
        var existing = await _service.GetLessonAsync(id);
        if (existing == null) return NotFound();

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.ContentUrl = request.ContentUrl;
        existing.ContentType = request.ContentType;
        existing.Order = request.Order;

        await _service.UpdateLessonAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteLessonAsync(id);
        return NoContent();
    }
}

public record CreateLessonRequest(Guid CourseId, string Title, string Description, string ContentUrl, string ContentType, int Order);
public record UpdateLessonRequest(string Title, string Description, string ContentUrl, string ContentType, int Order);
