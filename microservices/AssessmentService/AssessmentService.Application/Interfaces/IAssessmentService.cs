using AssessmentService.Application.DTOs;

namespace AssessmentService.Application.Interfaces;

public interface IAssessmentService
{
    Task<QuizDto> CreateQuizAsync(CreateQuizRequest request, string instructorId);
    Task UpdateQuizAsync(Guid id, CreateQuizRequest request);
    Task DeleteQuizAsync(Guid id);
    Task AddQuestionAsync(Guid quizId, AddQuestionRequest request);
    Task UpdateQuestionAsync(Guid questionId, AddQuestionRequest request);
    Task DeleteQuestionAsync(Guid questionId);
    Task<IEnumerable<QuizDto>> GetQuizzesForCourseAsync(Guid courseId);
    Task<IEnumerable<QuestionDto>> GetQuizQuestionsAsync(Guid quizId);
    Task<QuizSubmissionResponse> SubmitQuizAsync(Guid quizId, Guid studentId, SubmitQuizRequest request);
}
