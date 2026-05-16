using NUnit.Framework;
using Moq;
using FluentAssertions;
using ProgressService.Application.Services;
using ProgressService.Application.Interfaces;
using ProgressService.Application.DTOs;
using ProgressService.Domain.Entities;

namespace ProgressService.Tests;

[TestFixture]
public class ProgressServiceTests
{
    private Mock<IProgressRepository> _repoMock;
    private Application.Services.ProgressService _service;

    [SetUp]
    public void Setup()
    {
        _repoMock = new Mock<IProgressRepository>();
        _service = new Application.Services.ProgressService(_repoMock.Object);
    }

    [Test]
    public async Task MarkLessonAsCompleteAsync_ShouldCreateNewProgress_WhenNotExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new ProgressRequestDto { CourseId = Guid.NewGuid(), LessonId = Guid.NewGuid(), IsCompleted = true };
        _repoMock.Setup(r => r.GetProgressAsync(studentId, request.LessonId)).ReturnsAsync((UserProgress)null!);

        // Act
        await _service.MarkLessonAsCompleteAsync(studentId, request);

        // Assert
        _repoMock.Verify(r => r.AddAsync(It.IsAny<UserProgress>()), Times.Once);
    }

    [Test]
    public async Task MarkLessonAsCompleteAsync_ShouldUpdateProgress_WhenExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var existing = new UserProgress { StudentId = studentId, LessonId = lessonId, IsCompleted = false };
        var request = new ProgressRequestDto { CourseId = Guid.NewGuid(), LessonId = lessonId, IsCompleted = true };
        _repoMock.Setup(r => r.GetProgressAsync(studentId, lessonId)).ReturnsAsync(existing);

        // Act
        await _service.MarkLessonAsCompleteAsync(studentId, request);

        // Assert
        existing.IsCompleted.Should().BeTrue();
        _repoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Test]
    public async Task GetCourseProgressAsync_ShouldCalculatePercentage()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var progressList = new List<UserProgress>
        {
            new UserProgress { LessonId = Guid.NewGuid(), IsCompleted = true },
            new UserProgress { LessonId = Guid.NewGuid(), IsCompleted = false }
        };
        _repoMock.Setup(r => r.GetCourseProgressAsync(studentId, courseId)).ReturnsAsync(progressList);

        // Act
        var result = await _service.GetCourseProgressAsync(studentId, courseId);

        // Assert
        result.CompletionPercentage.Should().Be(50);
        result.CompletedLessons.Should().HaveCount(2);
    }
}
