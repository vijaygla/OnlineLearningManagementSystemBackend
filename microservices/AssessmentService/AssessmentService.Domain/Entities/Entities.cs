using SharedKernel.Base;

namespace AssessmentService.Domain.Entities;

public class Quiz : BaseAuditableEntity
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PassingScore { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class Question : BaseAuditableEntity
{
    public Guid QuizId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty; // Store as JSON or semicolon separated
    public int CorrectOptionIndex { get; set; }
    
    public Quiz? Quiz { get; set; }
}

public class QuizSubmission : BaseAuditableEntity
{
    public Guid QuizId { get; set; }
    public Guid StudentId { get; set; }
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Quiz? Quiz { get; set; }
}
