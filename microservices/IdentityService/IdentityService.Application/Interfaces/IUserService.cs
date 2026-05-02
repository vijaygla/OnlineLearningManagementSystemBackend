using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<int> GetTotalUserCountAsync();
    Task DeleteUserAsync(Guid id);
}
