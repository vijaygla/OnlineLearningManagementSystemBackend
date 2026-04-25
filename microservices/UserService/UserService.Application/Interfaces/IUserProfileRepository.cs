using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid userId);
    Task AddAsync(UserProfile userProfile);
    Task UpdateAsync(UserProfile userProfile);
    Task DeleteAsync(Guid userId);
    Task<bool> ExistsAsync(Guid userId);
}
