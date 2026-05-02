using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<int> GetTotalUserCountAsync()
    {
        return await _repository.GetCountAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
