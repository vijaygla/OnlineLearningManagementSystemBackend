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

    public async Task<QuizDto> GetQuizByIdAsync(Guid id)
    {
        var quiz = await _repo.GetQuizByIdAsync(id);
        if (quiz == null) throw new KeyNotFoundException("Quiz not found");

        return MapToDto(quiz);
    }

    public async Task<IEnumerable<QuizDto>> GetQuizzesForCourseAsync(Guid courseId)
    {
        var quizzes = await _repo.GetQuizzesByCourseIdAsync(courseId);
        return quizzes.Select(MapToDto);
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
            CreatedBy = instructorId
        };

        await _repo.AddQuizAsync(quiz);
        return MapToDto(quiz);
    }

    public async Task UpdateQuizAsync(Guid id, UpdateQuizRequest request)
    {
        var quiz = await _repo.GetQuizByIdAsync(id);
        if (quiz == null) throw new KeyNotFoundException("Quiz not found");

        quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.PassingScore = request.PassingScore;

        await _repo.UpdateQuizAsync(quiz);
    }

    public async Task DeleteQuizAsync(Guid id)
    {
        await _repo.DeleteQuizAsync(id);
    }

    public async Task<QuestionDto> GetQuestionByIdAsync(Guid id)
    {
        var q = await _repo.GetQuestionByIdAsync(id);
        if (q == null) throw new KeyNotFoundException("Question not found");

        return MapQuestionToDto(q);
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

    public async Task UpdateQuestionAsync(Guid id, UpdateQuestionRequest request)
    {
        var q = await _repo.GetQuestionByIdAsync(id);
        if (q == null) throw new KeyNotFoundException("Question not found");

        q.Text = request.Text;
        q.Options = string.Join(";", request.Options);
        q.CorrectOptionIndex = request.CorrectOptionIndex;

        await _repo.UpdateQuestionAsync(q);
    }

    public async Task DeleteQuestionAsync(Guid id)
    {
        await _repo.DeleteQuestionAsync(id);
    }

    public async Task<QuizSubmissionResponse> SubmitQuizAsync(Guid quizId, Guid studentId, SubmitQuizRequest request)
    {
        var quiz = await _repo.GetQuizByIdAsync(quizId);
        if (quiz == null) throw new KeyNotFoundException("Quiz not found");

        int correctAnswers = 0;
        var questions = quiz.Questions.OrderBy(q => q.CreatedAt).ToList();

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

        return new QuizSubmissionResponse(submission.Id, score, isPassed, questions.Count, submission.SubmittedAt);
    }

    public async Task<IEnumerable<QuizSubmissionResponse>> GetStudentSubmissionsAsync(Guid studentId)
    {
        var subs = await _repo.GetSubmissionsByStudentIdAsync(studentId);
        return subs.Select(s => new QuizSubmissionResponse(s.Id, s.Score, s.IsPassed, 0, s.SubmittedAt));
    }

    private static QuizDto MapToDto(Quiz q)
    {
        return new QuizDto(
            q.Id, 
            q.CourseId, 
            q.Title, 
            q.Description, 
            q.PassingScore,
            q.Questions.Select(MapQuestionToDto).ToList());
    }

    private static QuestionDto MapQuestionToDto(Question q)
    {
        return new QuestionDto(
            q.Id, 
            q.QuizId, 
            q.Text, 
            q.Options.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
            q.CorrectOptionIndex);
    }
}
