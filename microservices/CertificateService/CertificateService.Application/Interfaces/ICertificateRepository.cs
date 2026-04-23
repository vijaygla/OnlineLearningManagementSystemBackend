using CertificateService.Domain.Entities;

namespace CertificateService.Application.Interfaces;

public interface ICertificateRepository
{
    Task<Certificate?> GetByIdAsync(Guid id);
    Task<Certificate?> GetByNumberAsync(string certificateNumber);
    Task<IEnumerable<Certificate>> GetByStudentIdAsync(Guid studentId);
    Task<Certificate?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task AddAsync(Certificate certificate);
    Task UpdateAsync(Certificate certificate);
}
