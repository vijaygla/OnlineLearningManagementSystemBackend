using CertificateService.Application.DTOs;

namespace CertificateService.Application.Interfaces;

public interface ICertificateService
{
    Task<CertificateDto> IssueCertificateAsync(IssueCertificateRequest request, string issuedBy);
    Task<IEnumerable<CertificateDto>> GetStudentCertificatesAsync(Guid studentId);
    Task<CertificateDto?> GetCertificateByIdAsync(Guid id);
    Task<CertificateVerificationResponse> VerifyCertificateAsync(string certificateNumber);
}
