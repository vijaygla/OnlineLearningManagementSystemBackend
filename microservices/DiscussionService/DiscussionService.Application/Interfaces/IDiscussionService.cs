using DiscussionService.Application.DTOs;

namespace DiscussionService.Application.Interfaces;

public interface IDiscussionService
{
    Task<IEnumerable<ThreadResponse>> GetCourseThreadsAsync(Guid courseId);
    Task<ThreadResponse?> GetThreadDetailsAsync(Guid threadId);
    Task<ThreadResponse> CreateThreadAsync(CreateThreadRequest request, Guid userId, string userName);
    Task<CommentResponse> AddCommentAsync(CreateCommentRequest request, Guid userId, string userName);
    Task DeleteThreadAsync(Guid threadId, Guid userId, string role);
}
