using AssessmentService.Domain.Entities;

namespace AssessmentService.Application.Interfaces;

public interface IAssessmentRepository
{
    Task<Quiz?> GetQuizByIdAsync(Guid id);
    Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(Guid courseId);
    Task AddQuizAsync(Quiz quiz);
    Task UpdateQuizAsync(Quiz quiz);
    Task DeleteQuizAsync(Guid id);
    Task<Question?> GetQuestionByIdAsync(Guid id);
    Task AddQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(Guid id);
    Task AddSubmissionAsync(QuizSubmission submission);
    Task<IEnumerable<QuizSubmission>> GetSubmissionsByStudentIdAsync(Guid studentId);
}
