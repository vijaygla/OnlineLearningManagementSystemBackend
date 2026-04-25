using MassTransit;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Events;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserProfileRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public UserService(IUserProfileRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        var profile = await _repository.GetByIdAsync(userId);
        if (profile == null) return null;

        return new UserProfileDto
        {
            UserId = profile.UserId,
            FullName = profile.FullName,
            Bio = profile.Bio,
            ProfilePictureUrl = profile.ProfilePictureUrl,
            PhoneNumber = profile.PhoneNumber,
            Location = profile.Location,
            Skills = profile.Skills,
            Interests = profile.Interests,
            CreatedAt = profile.CreatedAt,
            LastUpdatedAt = profile.LastUpdatedAt,
            CoursesCompletedCount = profile.CoursesCompletedCount,
            TotalLearningHours = profile.TotalLearningHours,
            SocialLinks = profile.SocialLinks.Select(s => new SocialLinkDto { Platform = s.Platform, Url = s.Url }).ToList(),
            EnrolledCourses = profile.EnrolledCourses.Select(e => new EnrolledCourseDto 
            { 
                CourseId = e.CourseId, 
                CourseName = e.CourseName, 
                EnrolledAt = e.EnrolledAt,
                IsCompleted = e.IsCompleted,
                CompletedAt = e.CompletedAt
            }).ToList()
        };
    }

    public async Task<UserProfileDto?> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto)
    {
        var profile = await _repository.GetByIdAsync(userId);
        if (profile == null) return null;

        profile.FullName = dto.FullName;
        profile.Bio = dto.Bio;
        profile.ProfilePictureUrl = dto.ProfilePictureUrl;
        profile.PhoneNumber = dto.PhoneNumber;
        profile.Location = dto.Location;
        profile.Skills = dto.Skills;
        profile.Interests = dto.Interests;
        profile.LastUpdatedAt = DateTime.UtcNow;

        // Update Social Links
        profile.SocialLinks.Clear();
        foreach (var link in dto.SocialLinks)
        {
            profile.SocialLinks.Add(new Domain.Entities.SocialLink
            {
                UserId = userId,
                Platform = link.Platform,
                Url = link.Url
            });
        }

        await _repository.UpdateAsync(profile);

        return await GetUserProfileAsync(userId);
    }

    public async Task<string?> UploadProfilePictureAsync(Guid userId, IFormFile file)
    {
        var profile = await _repository.GetByIdAsync(userId);
        if (profile == null) return null;

        if (file == null || file.Length == 0) return null;

        // Validation
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Invalid file type. Only JPG, JPEG, and PNG are allowed.");
        }

        if (file.Length > 2 * 1024 * 1024) // 2MB limit
        {
            throw new InvalidOperationException("File size exceeds 2MB limit.");
        }

        // Create Directory
        var folderName = Path.Combine("wwwroot", "profile-pictures");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
        if (!Directory.Exists(pathToSave))
        {
            Directory.CreateDirectory(pathToSave);
        }

        // Save File
        var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{extension}";
        var fullPath = Path.Combine(pathToSave, fileName);
        var dbPath = $"/profile-pictures/{fileName}";

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update Profile
        profile.ProfilePictureUrl = dbPath;
        profile.LastUpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(profile);

        return dbPath;
    }

    public async Task<bool> DeleteUserAccountAsync(Guid userId)
    {
        var exists = await _repository.ExistsAsync(userId);
        if (!exists) return false;

        await _repository.DeleteAsync(userId);

        // Publish event for other services to clean up
        await _publishEndpoint.Publish(new UserDeletedEvent
        {
            UserId = userId,
            DeletedAt = DateTime.UtcNow
        });

        return true;
    }
}
