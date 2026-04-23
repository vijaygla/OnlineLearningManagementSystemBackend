namespace AssessmentService.Domain.Entities;

public class Quiz
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PassingScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class Question
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty; // Semicolon separated options
    public int CorrectOptionIndex { get; set; }
    
    public Quiz? Quiz { get; set; }
}

public class QuizSubmission
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Guid StudentId { get; set; }
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Quiz? Quiz { get; set; }
}
