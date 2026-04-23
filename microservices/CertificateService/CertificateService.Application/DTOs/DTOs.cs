namespace CertificateService.Application.DTOs;

public record CertificateDto(Guid Id, Guid StudentId, Guid CourseId, string CertificateNumber, DateTime IssueDate, string IssuedBy, bool IsRevoked);
public record IssueCertificateRequest(Guid StudentId, Guid CourseId);
public record CertificateVerificationResponse(bool IsValid, CertificateDto? Certificate);
