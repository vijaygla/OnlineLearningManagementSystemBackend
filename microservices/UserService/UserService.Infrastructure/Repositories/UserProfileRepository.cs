using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UserDbContext _context;

    public UserProfileRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByIdAsync(Guid userId)
    {
        return await _context.UserProfiles
            .Include(u => u.Preference)
            .Include(u => u.SocialLinks)
            .Include(u => u.EnrolledCourses)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task AddAsync(UserProfile userProfile)
    {
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserProfile userProfile)
    {
        _context.UserProfiles.Update(userProfile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId)
    {
        var profile = await _context.UserProfiles.FindAsync(userId);
        if (profile != null)
        {
            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid userId)
    {
        return await _context.UserProfiles.AnyAsync(u => u.UserId == userId);
    }
}
