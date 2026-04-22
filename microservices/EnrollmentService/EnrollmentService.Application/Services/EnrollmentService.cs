using EnrollmentService.Application.DTOs;
using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using SharedKernel.Enums;

namespace EnrollmentService.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;

    public EnrollmentService(IEnrollmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<EnrollmentResponseDto> EnrollStudentAsync(Guid studentId, EnrollmentRequestDto request)
    {
        var existing = await _repo.GetByStudentAndCourseAsync(studentId, request.CourseId);
        if (existing != null)
        {
            throw new InvalidOperationException("Student is already enrolled in this course.");
        }

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = request.CourseId,
            EnrollmentDate = DateTime.UtcNow,
            Status = EnrollmentStatus.Active,
            ProgressPercentage = 0,
            CreatedBy = studentId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(enrollment);

        return MapToDto(enrollment);
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetStudentEnrollmentsAsync(Guid studentId)
    {
        var enrollments = await _repo.GetByStudentIdAsync(studentId);
        return enrollments.Select(MapToDto);
    }

    public async Task<EnrollmentResponseDto?> GetEnrollmentDetailsAsync(Guid id)
    {
        var enrollment = await _repo.GetByIdAsync(id);
        return enrollment == null ? null : MapToDto(enrollment);
    }

    private static EnrollmentResponseDto MapToDto(Enrollment enrollment)
    {
        return new EnrollmentResponseDto
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollmentDate = enrollment.EnrollmentDate,
            Status = enrollment.Status,
            ProgressPercentage = enrollment.ProgressPercentage
        };
    }
}
