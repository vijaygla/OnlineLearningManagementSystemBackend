using CertificateService.Application.DTOs;
using CertificateService.Application.Interfaces;
using CertificateService.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace CertificateService.Application.Services;

public class CertificateService : ICertificateService
{
    private readonly ICertificateRepository _repo;

    public CertificateService(ICertificateRepository repo)
    {
        _repo = repo;
    }

    public async Task<CertificateDto> IssueCertificateAsync(IssueCertificateRequest request, string issuedBy)
    {
        var existing = await _repo.GetByStudentAndCourseAsync(request.StudentId, request.CourseId);
        if (existing != null)
        {
            return MapToDto(existing);
        }

        var certificate = new Certificate
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            CertificateNumber = GenerateCertificateNumber(request.StudentId, request.CourseId),
            IssueDate = DateTime.UtcNow,
            IssuedBy = issuedBy,
            IsRevoked = false
        };

        await _repo.AddAsync(certificate);
        return MapToDto(certificate);
    }

    public async Task<IEnumerable<CertificateDto>> GetStudentCertificatesAsync(Guid studentId)
    {
        var certificates = await _repo.GetByStudentIdAsync(studentId);
        return certificates.Select(MapToDto);
    }

    public async Task<CertificateDto?> GetCertificateByIdAsync(Guid id)
    {
        var certificate = await _repo.GetByIdAsync(id);
        return certificate == null ? null : MapToDto(certificate);
    }

    public async Task<CertificateVerificationResponse> VerifyCertificateAsync(string certificateNumber)
    {
        var certificate = await _repo.GetByNumberAsync(certificateNumber);
        if (certificate == null || certificate.IsRevoked)
        {
            return new CertificateVerificationResponse(false, null);
        }

        return new CertificateVerificationResponse(true, MapToDto(certificate));
    }

    private static string GenerateCertificateNumber(Guid studentId, Guid courseId)
    {
        var rawData = $"{studentId}-{courseId}-{DateTime.UtcNow.Ticks}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder("CERT-");
        for (int i = 0; i < 8; i++)
        {
            builder.Append(bytes[i].ToString("X2"));
        }
        return builder.ToString();
    }

    private static CertificateDto MapToDto(Certificate c)
    {
        return new CertificateDto(c.Id, c.StudentId, c.CourseId, c.CertificateNumber, c.IssueDate, c.IssuedBy, c.IsRevoked);
    }
}
