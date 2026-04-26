using Microsoft.EntityFrameworkCore;
using DiscussionService.Application.Interfaces;
using DiscussionService.Domain.Entities;
using DiscussionService.Infrastructure.Data;

namespace DiscussionService.Infrastructure.Repositories;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly DiscussionDbContext _context;

    public DiscussionRepository(DiscussionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DiscussionThread>> GetThreadsByCourseAsync(Guid courseId)
    {
        return await _context.Threads
            .Where(t => t.CourseId == courseId)
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.LastActivityAt)
            .ToListAsync();
    }

    public async Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId)
    {
        return await _context.Threads
            .Include(t => t.Comments.Where(c => c.ParentCommentId == null))
            .ThenInclude(c => c.Replies)
            .FirstOrDefaultAsync(t => t.Id == threadId);
    }

    public async Task AddThreadAsync(DiscussionThread thread)
    {
        await _context.Threads.AddAsync(thread);
        await _context.SaveChangesAsync();
    }

    public async Task AddCommentAsync(DiscussionComment comment)
    {
        await _context.Comments.AddAsync(comment);
        
        // Update thread activity
        var thread = await _context.Threads.FindAsync(comment.ThreadId);
        if (thread != null)
        {
            thread.CommentCount++;
            thread.LastActivityAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task UpdateThreadAsync(DiscussionThread thread)
    {
        _context.Threads.Update(thread);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteThreadAsync(Guid threadId)
    {
        var thread = await _context.Threads.FindAsync(threadId);
        if (thread != null)
        {
            _context.Threads.Remove(thread);
            await _context.SaveChangesAsync();
        }
    }
}
