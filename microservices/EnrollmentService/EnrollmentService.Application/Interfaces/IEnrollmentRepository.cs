using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);
    Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task<int> GetCountByCourseIdAsync(Guid courseId);
    Task AddAsync(Enrollment enrollment);
    Task UpdateAsync(Enrollment enrollment);
}
