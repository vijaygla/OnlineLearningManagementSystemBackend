using ReviewService.Application.DTOs;

namespace ReviewService.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(Guid studentId, CreateReviewRequest request);
    Task UpdateReviewAsync(Guid id, Guid studentId, UpdateReviewRequest request);
    Task DeleteReviewAsync(Guid id, Guid studentId, string userRole);
    Task<IEnumerable<ReviewDto>> GetCourseReviewsAsync(Guid courseId);
    Task<CourseRatingResponse> GetCourseRatingAsync(Guid courseId);
}
