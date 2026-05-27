namespace AssessmentService.Application.DTOs;

public record QuizDto(
    Guid Id, 
    Guid CourseId, 
    string Title, 
    string Description, 
    int PassingScore,
    List<QuestionDto> Questions);

public record CreateQuizRequest(Guid CourseId, string Title, string Description, int PassingScore);
public record UpdateQuizRequest(string Title, string Description, int PassingScore);

public record QuestionDto(Guid Id, Guid QuizId, string Text, List<string> Options, int CorrectOptionIndex);
public record AddQuestionRequest(string Text, List<string> Options, int CorrectOptionIndex);
public record UpdateQuestionRequest(string Text, List<string> Options, int CorrectOptionIndex);

public record SubmitQuizRequest(List<int> Answers);
public record QuizSubmissionResponse(
    Guid SubmissionId, 
    int Score, 
    bool IsPassed, 
    int TotalQuestions, 
    DateTime? SubmittedAt = null);
