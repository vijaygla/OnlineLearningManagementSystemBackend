using AssessmentService.Application.Interfaces;
using AssessmentService.Domain.Entities;
using AssessmentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repositories;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AssessmentDbContext _context;

    public AssessmentRepository(AssessmentDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetQuizByIdAsync(Guid id)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(Guid courseId)
    {
        return await _context.Quizzes
            .Where(q => q.CourseId == courseId)
            .ToListAsync();
    }

    public async Task AddQuizAsync(Quiz quiz)
    {
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task AddQuestionAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
    }

    public async Task AddSubmissionAsync(QuizSubmission submission)
    {
        await _context.Submissions.AddAsync(submission);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<QuizSubmission>> GetSubmissionsByStudentIdAsync(Guid studentId)
    {
        return await _context.Submissions
            .Where(s => s.StudentId == studentId)
            .ToListAsync();
    }
}
