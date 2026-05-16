using NUnit.Framework;
using Moq;
using FluentAssertions;
using DiscussionService.Application.Services;
using DiscussionService.Application.Interfaces;
using DiscussionService.Application.DTOs;
using DiscussionService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DiscussionService.Tests;

[TestFixture]
public class DiscussionServiceTests
{
    private Mock<IDiscussionRepository> _repoMock;
    private Mock<IPublishEndpoint> _publishMock;
    private Mock<ILogger<Application.Services.DiscussionService>> _loggerMock;
    private Application.Services.DiscussionService _service;

    [SetUp]
    public void Setup()
    {
        _repoMock = new Mock<IDiscussionRepository>();
        _publishMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<Application.Services.DiscussionService>>();
        _service = new Application.Services.DiscussionService(_repoMock.Object, _publishMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateThreadAsync_ShouldReturnThreadResponse_AndCallRepository()
    {
        // Arrange
        var request = new CreateThreadRequest { Title = "Title", Content = "Content", CourseId = Guid.NewGuid() };
        var userId = Guid.NewGuid();
        var userName = "User1";

        // Act
        var result = await _service.CreateThreadAsync(request, userId, userName);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.AuthorName.Should().Be(userName);
        _repoMock.Verify(r => r.AddThreadAsync(It.IsAny<DiscussionThread>()), Times.Once);
    }

    [Test]
    public async Task AddCommentAsync_ShouldReturnCommentResponse_AndCallRepository()
    {
        // Arrange
        var request = new CreateCommentRequest { ThreadId = Guid.NewGuid(), Content = "Nice!" };
        var userId = Guid.NewGuid();
        var userName = "User2";

        // Act
        var result = await _service.AddCommentAsync(request, userId, userName);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(request.Content);
        _repoMock.Verify(r => r.AddCommentAsync(It.IsAny<DiscussionComment>()), Times.Once);
    }

    [Test]
    public async Task DeleteThreadAsync_ShouldDelete_WhenUserIsAuthor()
    {
        // Arrange
        var threadId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var thread = new DiscussionThread { Id = threadId, AuthorId = userId };
        _repoMock.Setup(r => r.GetThreadByIdAsync(threadId)).ReturnsAsync(thread);

        // Act
        await _service.DeleteThreadAsync(threadId, userId, "Student");

        // Assert
        _repoMock.Verify(r => r.DeleteThreadAsync(threadId), Times.Once);
    }

    [Test]
    public void DeleteThreadAsync_ShouldThrowUnauthorized_WhenUserIsNotAuthorOrStaff()
    {
        // Arrange
        var threadId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var thread = new DiscussionThread { Id = threadId, AuthorId = authorId };
        _repoMock.Setup(r => r.GetThreadByIdAsync(threadId)).ReturnsAsync(thread);

        // Act & Assert
        Func<Task> act = async () => await _service.DeleteThreadAsync(threadId, otherUserId, "Student");
        act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task DeleteThreadAsync_ShouldDelete_WhenUserIsInstructor()
    {
        // Arrange
        var threadId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();
        var thread = new DiscussionThread { Id = threadId, AuthorId = authorId };
        _repoMock.Setup(r => r.GetThreadByIdAsync(threadId)).ReturnsAsync(thread);

        // Act
        await _service.DeleteThreadAsync(threadId, instructorId, "Instructor");

        // Assert
        _repoMock.Verify(r => r.DeleteThreadAsync(threadId), Times.Once);
    }
}
