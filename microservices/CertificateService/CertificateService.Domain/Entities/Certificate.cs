namespace CertificateService.Domain.Entities;

public class Certificate
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public string IssuedBy { get; set; } = "System";
    public bool IsRevoked { get; set; } = false;
}
