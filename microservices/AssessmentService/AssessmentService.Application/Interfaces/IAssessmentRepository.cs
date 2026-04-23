using AssessmentService.Domain.Entities;

namespace AssessmentService.Application.Interfaces;

public interface IAssessmentRepository
{
    Task<Quiz?> GetQuizByIdAsync(Guid id);
    Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(Guid courseId);
    Task AddQuizAsync(Quiz quiz);
    Task AddQuestionAsync(Question question);
    Task AddSubmissionAsync(QuizSubmission submission);
    Task<IEnumerable<QuizSubmission>> GetSubmissionsByStudentIdAsync(Guid studentId);
}
