using Microsoft.AspNetCore.Http;
using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
    Task<UserProfileDto?> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto);
    Task<string?> UploadProfilePictureAsync(Guid userId, IFormFile file);
    Task<bool> DeleteUserAccountAsync(Guid userId);
}
