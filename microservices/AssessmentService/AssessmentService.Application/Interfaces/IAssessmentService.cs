using AssessmentService.Application.DTOs;

namespace AssessmentService.Application.Interfaces;

public interface IAssessmentService
{
    Task<QuizDto> GetQuizByIdAsync(Guid id);
    Task<IEnumerable<QuizDto>> GetQuizzesForCourseAsync(Guid courseId);
    Task<QuizDto> CreateQuizAsync(CreateQuizRequest request, string instructorId);
    Task UpdateQuizAsync(Guid id, UpdateQuizRequest request);
    Task DeleteQuizAsync(Guid id);

    Task<QuestionDto> GetQuestionByIdAsync(Guid id);
    Task AddQuestionAsync(Guid quizId, AddQuestionRequest request);
    Task UpdateQuestionAsync(Guid id, UpdateQuestionRequest request);
    Task DeleteQuestionAsync(Guid id);

    Task<QuizSubmissionResponse> SubmitQuizAsync(Guid quizId, Guid studentId, SubmitQuizRequest request);
    Task<IEnumerable<QuizSubmissionResponse>> GetStudentSubmissionsAsync(Guid studentId);
}
