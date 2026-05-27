namespace CertificateService.Application.DTOs;

public record CertificateDto(
    Guid Id, 
    Guid StudentId, 
    Guid CourseId, 
    string StudentName,
    string CourseTitle,
    string CertificateNumber, 
    DateTime IssueDate, 
    bool IsRevoked);

public record IssueCertificateRequest(
    Guid StudentId, 
    Guid CourseId,
    string StudentName,
    string CourseTitle);

public record CertificateVerificationResponse(bool IsValid, CertificateDto? Certificate);
