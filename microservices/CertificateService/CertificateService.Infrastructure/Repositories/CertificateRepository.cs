using CertificateService.Application.Interfaces;
using CertificateService.Domain.Entities;
using CertificateService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CertificateService.Infrastructure.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly CertificateDbContext _context;

    public CertificateRepository(CertificateDbContext context)
    {
        _context = context;
    }

    public async Task<Certificate?> GetByIdAsync(Guid id)
    {
        return await _context.Certificates.FindAsync(id);
    }

    public async Task<Certificate?> GetByNumberAsync(string certificateNumber)
    {
        return await _context.Certificates
            .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);
    }

    public async Task<IEnumerable<Certificate>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.Certificates
            .Where(c => c.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<Certificate?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _context.Certificates
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);
    }

    public async Task AddAsync(Certificate certificate)
    {
        await _context.Certificates.AddAsync(certificate);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Certificate certificate)
    {
        _context.Certificates.Update(certificate);
        await _context.SaveChangesAsync();
    }
}
