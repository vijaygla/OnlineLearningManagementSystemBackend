namespace AssessmentService.Application.DTOs;

public record QuizDto(Guid Id, Guid CourseId, string Title, string Description, int PassingScore);
public record CreateQuizRequest(Guid CourseId, string Title, string Description, int PassingScore);

public record QuestionDto(Guid Id, string Text, List<string> Options);
public record AddQuestionRequest(string Text, List<string> Options, int CorrectOptionIndex);

public record SubmitQuizRequest(List<int> Answers);
public record QuizSubmissionResponse(Guid SubmissionId, int Score, bool IsPassed, int TotalQuestions);
