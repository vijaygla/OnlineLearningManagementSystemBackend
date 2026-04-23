using ReviewService.Application.DTOs;
using ReviewService.Application.Interfaces;
using ReviewService.Domain.Entities;

namespace ReviewService.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repo;

    public ReviewService(IReviewRepository repo)
    {
        _repo = repo;
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid studentId, CreateReviewRequest request)
    {
        var existing = await _repo.GetByStudentAndCourseAsync(studentId, request.CourseId);
        if (existing != null)
        {
            throw new InvalidOperationException("You have already reviewed this course.");
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = request.CourseId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(review);
        return MapToDto(review);
    }

    public async Task UpdateReviewAsync(Guid id, Guid studentId, UpdateReviewRequest request)
    {
        var review = await _repo.GetByIdAsync(id);
        if (review == null) throw new KeyNotFoundException("Review not found.");
        if (review.StudentId != studentId) throw new UnauthorizedAccessException("You can only update your own reviews.");

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.LastModifiedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(review);
    }

    public async Task DeleteReviewAsync(Guid id, Guid studentId, string userRole)
    {
        var review = await _repo.GetByIdAsync(id);
        if (review == null) throw new KeyNotFoundException("Review not found.");
        
        if (review.StudentId != studentId && userRole != "Admin")
            throw new UnauthorizedAccessException("You don't have permission to delete this review.");

        await _repo.DeleteAsync(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetCourseReviewsAsync(Guid courseId)
    {
        var reviews = await _repo.GetByCourseIdAsync(courseId);
        return reviews.Select(MapToDto);
    }

    public async Task<CourseRatingResponse> GetCourseRatingAsync(Guid courseId)
    {
        var reviews = await _repo.GetByCourseIdAsync(courseId);
        var reviewList = reviews.ToList();
        
        double avg = reviewList.Count > 0 ? reviewList.Average(r => r.Rating) : 0;
        
        return new CourseRatingResponse(courseId, Math.Round(avg, 1), reviewList.Count);
    }

    private static ReviewDto MapToDto(Review r)
    {
        return new ReviewDto(r.Id, r.StudentId, r.CourseId, r.Rating, r.Comment, r.CreatedAt);
    }
}
