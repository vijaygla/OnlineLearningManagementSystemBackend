using Microsoft.AspNetCore.Mvc;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;

namespace ContentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly IContentService _contentService;

    public LessonsController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("section/{sectionId}")]
    public async Task<IActionResult> GetBySection(Guid sectionId)
    {
        var lessons = await _contentService.GetLessonsBySectionIdAsync(sectionId);
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var lesson = await _contentService.GetLessonByIdAsync(id);
        if (lesson == null) return NotFound();
        return Ok(lesson);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Lesson lesson)
    {
        var created = await _contentService.CreateLessonAsync(lesson);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Lesson lesson)
    {
        if (id != lesson.Id) return BadRequest();
        await _contentService.UpdateLessonAsync(lesson);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _contentService.DeleteLessonAsync(id);
        return NoContent();
    }
}
