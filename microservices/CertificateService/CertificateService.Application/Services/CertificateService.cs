using CertificateService.Application.DTOs;
using CertificateService.Application.Interfaces;
using CertificateService.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CertificateService.Application.Services;

public class CertificateService : ICertificateService
{
    private readonly ICertificateRepository _repo;

    public CertificateService(ICertificateRepository repo)
    {
        _repo = repo;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<CertificateDto> IssueCertificateAsync(IssueCertificateRequest request)
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
            StudentName = request.StudentName,
            CourseTitle = request.CourseTitle,
            CertificateNumber = GenerateCertificateNumber(request.StudentId, request.CourseId),
            IssueDate = DateTime.UtcNow,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.StudentId.ToString()
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

    public async Task<byte[]> GenerateCertificatePdfAsync(Guid certificateId)
    {
        var cert = await _repo.GetByIdAsync(certificateId);
        if (cert == null) throw new KeyNotFoundException("Certificate not found");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Inch);
                page.PageColor(Colors.White);
                
                // Border
                page.Content().Border(10).BorderColor(Colors.Grey.Lighten4).Padding(20).Column(col =>
                {
                    col.Spacing(20);
                    col.Item().Text("CERTIFICATE OF COMPLETION")
                       .FontSize(32).FontColor(Colors.Blue.Medium).ExtraBold().AlignCenter();
                    
                    col.Item().PaddingTop(20).Text("This is to certify that")
                       .FontSize(14).Italic().AlignCenter();
                    
                    col.Item().Text(cert.StudentName)
                       .FontSize(42).Bold().FontColor(Colors.Grey.Darken4).AlignCenter();
                    
                    col.Item().Text("has successfully completed the course")
                       .FontSize(14).Italic().AlignCenter();
                    
                    col.Item().Text(cert.CourseTitle)
                       .FontSize(28).Bold().FontColor(Colors.Blue.Medium).AlignCenter();
                    
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Issued on").FontSize(10).FontColor(Colors.Grey.Medium);
                            c.Item().Text(cert.IssueDate.ToString("MMMM dd, yyyy")).FontSize(12).Bold();
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().AlignRight().Text("Certificate ID").FontSize(10).FontColor(Colors.Grey.Medium);
                            c.Item().AlignRight().Text(cert.CertificateNumber).FontSize(12).Bold().FontColor(Colors.Blue.Medium);
                        });
                    });

                    col.Item().PaddingTop(40).AlignCenter().Column(c => {
                         c.Item().Width(150).Height(1).Background(Colors.Grey.Lighten2);
                         c.Item().PaddingTop(5).Text("LMS ACADEMY OFFICIAL").FontSize(10).FontColor(Colors.Grey.Lighten1).AlignCenter();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string GenerateCertificateNumber(Guid studentId, Guid courseId)
    {
        var rawData = $"{studentId}-{courseId}-{DateTime.UtcNow.Ticks}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder("OLMS-");
        for (int i = 0; i < 6; i++)
        {
            builder.Append(bytes[i].ToString("X2"));
        }
        return builder.ToString();
    }

    private static CertificateDto MapToDto(Certificate c)
    {
        return new CertificateDto(c.Id, c.StudentId, c.CourseId, c.StudentName, c.CourseTitle, c.CertificateNumber, c.IssueDate, c.IsRevoked);
    }
}
