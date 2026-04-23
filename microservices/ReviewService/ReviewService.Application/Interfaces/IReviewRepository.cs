using ReviewService.Domain.Entities;

namespace ReviewService.Application.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id);
    Task<Review?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task<IEnumerable<Review>> GetByCourseIdAsync(Guid courseId);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
}
