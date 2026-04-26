using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiscussionService.Application.DTOs;
using DiscussionService.Application.Interfaces;

namespace DiscussionService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThreadsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;

    public ThreadsController(IDiscussionService discussionService)
    {
        _discussionService = discussionService;
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetCourseThreads(Guid courseId)
    {
        var threads = await _discussionService.GetCourseThreadsAsync(courseId);
        return Ok(threads);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetThreadDetails(Guid id)
    {
        var thread = await _discussionService.GetThreadDetailsAsync(id);
        if (thread == null) return NotFound();
        return Ok(thread);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name") ?? "Unknown User";
        
        var thread = await _discussionService.CreateThreadAsync(request, userId, userName);
        return CreatedAtAction(nameof(GetThreadDetails), new { id = thread.Id }, thread);
    }

    [HttpPost("comments")]
    [Authorize]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name") ?? "Unknown User";
        
        var comment = await _discussionService.AddCommentAsync(request, userId, userName);
        return Ok(comment);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteThread(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Student";
        
        await _discussionService.DeleteThreadAsync(id, userId, role);
        return NoContent();
    }
}
