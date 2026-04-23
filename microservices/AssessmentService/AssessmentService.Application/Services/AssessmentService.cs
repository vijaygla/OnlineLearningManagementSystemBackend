using AssessmentService.Application.DTOs;
using AssessmentService.Application.Interfaces;
using AssessmentService.Domain.Entities;

namespace AssessmentService.Application.Services;

public class AssessmentService : IAssessmentService
{
    private readonly IAssessmentRepository _repo;

    public AssessmentService(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<QuizDto> CreateQuizAsync(CreateQuizRequest request, string instructorId)
    {
        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            PassingScore = request.PassingScore,
            CreatedBy = instructorId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddQuizAsync(quiz);
        return new QuizDto(quiz.Id, quiz.CourseId, quiz.Title, quiz.Description, quiz.PassingScore);
    }

    public async Task AddQuestionAsync(Guid quizId, AddQuestionRequest request)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            Text = request.Text,
            Options = string.Join(";", request.Options),
            CorrectOptionIndex = request.CorrectOptionIndex
        };

        await _repo.AddQuestionAsync(question);
    }

    public async Task<IEnumerable<QuizDto>> GetQuizzesForCourseAsync(Guid courseId)
    {
        var quizzes = await _repo.GetQuizzesByCourseIdAsync(courseId);
        return quizzes.Select(q => new QuizDto(q.Id, q.CourseId, q.Title, q.Description, q.PassingScore));
    }

    public async Task<IEnumerable<QuestionDto>> GetQuizQuestionsAsync(Guid quizId)
    {
        var quiz = await _repo.GetQuizByIdAsync(quizId);
        if (quiz == null) throw new KeyNotFoundException("Quiz not found");

        return quiz.Questions.Select(q => new QuestionDto(
            q.Id, 
            q.Text, 
            q.Options.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()));
    }

    public async Task<QuizSubmissionResponse> SubmitQuizAsync(Guid quizId, Guid studentId, SubmitQuizRequest request)
    {
        var quiz = await _repo.GetQuizByIdAsync(quizId);
        if (quiz == null) throw new KeyNotFoundException("Quiz not found");

        int correctAnswers = 0;
        var questions = quiz.Questions.ToList();

        for (int i = 0; i < questions.Count; i++)
        {
            if (i < request.Answers.Count && request.Answers[i] == questions[i].CorrectOptionIndex)
            {
                correctAnswers++;
            }
        }

        int score = questions.Count > 0 ? (int)((double)correctAnswers / questions.Count * 100) : 0;
        bool isPassed = score >= quiz.PassingScore;

        var submission = new QuizSubmission
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            StudentId = studentId,
            Score = score,
            IsPassed = isPassed,
            SubmittedAt = DateTime.UtcNow
        };

        await _repo.AddSubmissionAsync(submission);

        return new QuizSubmissionResponse(submission.Id, score, isPassed, questions.Count);
    }
}
