using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using SharedKernel.Enums;
using MassTransit;
using Shared.Contracts.Events;

namespace CourseService.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CourseService(ICourseRepository courseRepository, IPublishEndpoint publishEndpoint)
    {
        _courseRepository = courseRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        return await _courseRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _courseRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesByStatusAsync(CourseStatus status)
    {
        return await _courseRepository.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId)
    {
        return await _courseRepository.GetByInstructorIdAsync(instructorId);
    }

    public async Task<Course> CreateCourseAsync(string title, string description, Guid categoryId, Guid instructorId, decimal price)
    {
        var course = new Course
        {
            Title = title,
            Description = description,
            CategoryId = categoryId,
            InstructorId = instructorId,
            Price = price,
            Status = CourseStatus.Pending // Default status
        };
        await _courseRepository.AddAsync(course);
        return course;
    }

    public async Task UpdateCourseAsync(Guid id, string title, string description, decimal price)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course != null)
        {
            course.Title = title;
            course.Description = description;
            course.Price = price;
            course.LastModifiedAt = DateTime.UtcNow;
            await _courseRepository.UpdateAsync(course);
        }
    }

    public async Task UpdateCourseStatusAsync(Guid id, CourseStatus status)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course != null)
        {
            var oldStatus = course.Status;
            course.Status = status;
            course.LastModifiedAt = DateTime.UtcNow;
            await _courseRepository.UpdateAsync(course);

            if (oldStatus != CourseStatus.Approved && status == CourseStatus.Approved)
            {
                await _publishEndpoint.Publish(new CourseApprovedEvent
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    CourseDescription = course.Description,
                    CategoryId = course.CategoryId,
                    InstructorId = course.InstructorId,
                    InstructorEmail = "instructor@example.com", // In a real app, we might fetch this
                    Price = course.Price,
                    ThumbnailUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085", // Placeholder
                    ApprovedAt = DateTime.UtcNow
                });
            }
        }
    }

    public async Task DeleteCourseAsync(Guid id)
    {
        await _courseRepository.DeleteAsync(id);
    }
}
