using Microsoft.AspNetCore.Mvc;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;

namespace ContentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly IContentService _contentService;

    public SectionsController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var sections = await _contentService.GetSectionsByCourseIdAsync(courseId);
        return Ok(sections);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var section = await _contentService.GetSectionByIdAsync(id);
        if (section == null) return NotFound();
        return Ok(section);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Section section)
    {
        var created = await _contentService.CreateSectionAsync(section);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Section section)
    {
        if (id != section.Id) return BadRequest();
        await _contentService.UpdateSectionAsync(section);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _contentService.DeleteSectionAsync(id);
        return NoContent();
    }
}
