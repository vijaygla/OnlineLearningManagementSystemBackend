using AssessmentService.Application.DTOs;
using AssessmentService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssessmentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _service;

    public AssessmentsController(IAssessmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> CreateQuiz(CreateQuizRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var quiz = await _service.CreateQuizAsync(request, userId);
        return Ok(quiz);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UpdateQuiz(Guid id, CreateQuizRequest request)
    {
        try
        {
            await _service.UpdateQuizAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> DeleteQuiz(Guid id)
    {
        await _service.DeleteQuizAsync(id);
        return NoContent();
    }

    [HttpPost("{quizId}/questions")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> AddQuestion(Guid quizId, AddQuestionRequest request)
    {
        await _service.AddQuestionAsync(quizId, request);
        return NoContent();
    }

    [HttpPut("questions/{questionId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UpdateQuestion(Guid questionId, AddQuestionRequest request)
    {
        try
        {
            await _service.UpdateQuestionAsync(questionId, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("questions/{questionId}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> DeleteQuestion(Guid questionId)
    {
        await _service.DeleteQuestionAsync(questionId);
        return NoContent();
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetCourseQuizzes(Guid courseId)
    {
        var quizzes = await _service.GetQuizzesForCourseAsync(courseId);
        return Ok(quizzes);
    }

    [HttpGet("{quizId}")]
    public async Task<IActionResult> GetQuizQuestions(Guid quizId)
    {
        try
        {
            var questions = await _service.GetQuizQuestionsAsync(quizId);
            return Ok(questions);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{quizId}/submit")]
    public async Task<IActionResult> SubmitQuiz(Guid quizId, SubmitQuizRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var studentId = Guid.Parse(userIdClaim);

        try
        {
            var result = await _service.SubmitQuizAsync(quizId, studentId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
