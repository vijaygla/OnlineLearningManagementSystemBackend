using FluentAssertions;
using MassTransit;
using Moq;
using Shared.Contracts.Events;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Entities;
using Xunit;

namespace UserService.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserProfileRepository> _repositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Application.Services.UserService _userService;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IUserProfileRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _userService = new Application.Services.UserService(_repositoryMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task GetUserProfileAsync_ShouldReturnProfile_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            UserId = userId,
            FullName = "Test User",
            Preference = new UserPreference { UserId = userId },
            SocialLinks = new List<SocialLink>(),
            EnrolledCourses = new List<EnrolledCourse>()
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(userProfile);

        // Act
        var result = await _userService.GetUserProfileAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task UpdateUserProfileAsync_ShouldUpdateProfile_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            UserId = userId,
            FullName = "Old Name",
            SocialLinks = new List<SocialLink>()
        };

        var updateDto = new UpdateUserProfileDto
        {
            FullName = "New Name",
            SocialLinks = new List<SocialLinkDto>
            {
                new SocialLinkDto { Platform = "LinkedIn", Url = "https://linkedin.com/in/test" }
            }
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(userProfile);

        // Act
        var result = await _userService.UpdateUserProfileAsync(userId, updateDto);

        // Assert
        userProfile.FullName.Should().Be("New Name");
        userProfile.SocialLinks.Should().HaveCount(1);
        userProfile.SocialLinks.First().Platform.Should().Be("LinkedIn");
        _repositoryMock.Verify(r => r.UpdateAsync(userProfile), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAccountAsync_ShouldDeleteAndPublishEvent_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExistsAsync(userId)).ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAccountAsync(userId);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(userId), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.Is<UserDeletedEvent>(e => e.UserId == userId), default), Times.Once);
    }
}
