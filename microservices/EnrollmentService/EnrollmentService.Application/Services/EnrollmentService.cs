using EnrollmentService.Application.DTOs;
using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using MassTransit;
using Shared.Contracts.Events;
using SharedKernel.Enums;

namespace EnrollmentService.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;
    private readonly IPublishEndpoint _publishEndpoint;

    public EnrollmentService(IEnrollmentRepository repo, IPublishEndpoint publishEndpoint)
    {
        _repo = repo;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<EnrollmentResponseDto> EnrollStudentAsync(Guid studentId, string studentEmail, EnrollmentRequestDto request)
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

        // Publish event to RabbitMQ
        await _publishEndpoint.Publish(new EnrollmentCreatedEvent
        {
            EnrollmentId = enrollment.Id,
            StudentId = studentId,
            CourseId = request.CourseId,
            StudentEmail = studentEmail,
            CourseName = string.IsNullOrWhiteSpace(request.CourseName) ? "Unknown Course" : request.CourseName,
            CreatedAt = enrollment.CreatedAt
        });

        return MapToDto(enrollment);
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetStudentEnrollmentsAsync(Guid studentId)
    {
        var enrollments = await _repo.GetByStudentIdAsync(studentId);
        return enrollments.Select(MapToDto);
    }

    public async Task<int> GetEnrollmentCountAsync(Guid courseId)
    {
        return await _repo.GetCountByCourseIdAsync(courseId);
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
