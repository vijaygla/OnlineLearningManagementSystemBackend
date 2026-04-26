using DiscussionService.Domain.Entities;

namespace DiscussionService.Application.Interfaces;

public interface IDiscussionRepository
{
    Task<IEnumerable<DiscussionThread>> GetThreadsByCourseAsync(Guid courseId);
    Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId);
    Task AddThreadAsync(DiscussionThread thread);
    Task AddCommentAsync(DiscussionComment comment);
    Task UpdateThreadAsync(DiscussionThread thread);
    Task DeleteThreadAsync(Guid threadId);
}
