using EnrollmentService.Application.DTOs;

namespace EnrollmentService.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> EnrollStudentAsync(Guid studentId, string studentEmail, EnrollmentRequestDto request);
    Task<IEnumerable<EnrollmentResponseDto>> GetStudentEnrollmentsAsync(Guid studentId);    Task<int> GetEnrollmentCountAsync(Guid courseId);
    Task<EnrollmentResponseDto?> GetEnrollmentDetailsAsync(Guid id);
}
