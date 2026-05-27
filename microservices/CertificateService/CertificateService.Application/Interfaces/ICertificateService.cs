using CertificateService.Application.DTOs;

namespace CertificateService.Application.Interfaces;

public interface ICertificateService
{
    Task<CertificateDto> IssueCertificateAsync(IssueCertificateRequest request);
    Task<IEnumerable<CertificateDto>> GetStudentCertificatesAsync(Guid studentId);
    Task<CertificateDto?> GetCertificateByIdAsync(Guid id);
    Task<CertificateVerificationResponse> VerifyCertificateAsync(string certificateNumber);
    Task<byte[]> GenerateCertificatePdfAsync(Guid certificateId);
}
