using NUnit.Framework;
using Moq;
using FluentAssertions;
using ReviewService.Application.Services;
using ReviewService.Application.Interfaces;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Entities;

namespace ReviewService.Tests;

[TestFixture]
public class ReviewServiceTests
{
    private Mock<IReviewRepository> _repoMock;
    private Mock<IEnrollmentClient> _enrollmentMock;
    private Application.Services.ReviewService _service;

    [SetUp]
    public void Setup()
    {
        _repoMock = new Mock<IReviewRepository>();
        _enrollmentMock = new Mock<IEnrollmentClient>();
        _service = new Application.Services.ReviewService(_repoMock.Object, _enrollmentMock.Object);
    }

    [Test]
    public async Task CreateReviewAsync_ShouldReturnReviewDto_WhenEnrolledAndNoPreviousReview()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateReviewRequest(Guid.NewGuid(), 5, "Great!");
        var token = "test-token";
        _enrollmentMock.Setup(e => e.IsEnrolledAsync(studentId, request.CourseId, token)).ReturnsAsync(true);
        _repoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, request.CourseId)).ReturnsAsync((Review)null!);

        // Act
        var result = await _service.CreateReviewAsync(studentId, request, token);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
    }

    [Test]
    public void CreateReviewAsync_ShouldThrowInvalidOperation_WhenReviewAlreadyExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateReviewRequest(Guid.NewGuid(), 5, "Great!");
        var token = "test-token";
        _enrollmentMock.Setup(e => e.IsEnrolledAsync(studentId, request.CourseId, token)).ReturnsAsync(true);
        _repoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, request.CourseId)).ReturnsAsync(new Review());

        // Act & Assert
        Func<Task> act = async () => await _service.CreateReviewAsync(studentId, request, token);
        act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already reviewed*");
    }

    [Test]
    public void CreateReviewAsync_ShouldThrowInvalidOperation_WhenNotEnrolled()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateReviewRequest(Guid.NewGuid(), 5, "Great!");
        var token = "test-token";
        _enrollmentMock.Setup(e => e.IsEnrolledAsync(studentId, request.CourseId, token)).ReturnsAsync(false);

        // Act & Assert
        Func<Task> act = async () => await _service.CreateReviewAsync(studentId, request, token);
        act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Only enrolled students*");
    }

    [Test]
    public async Task GetCourseRatingAsync_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            new Review { Rating = 5 },
            new Review { Rating = 4 },
            new Review { Rating = 1 }
        };
        _repoMock.Setup(r => r.GetByCourseIdAsync(courseId)).ReturnsAsync(reviews);

        // Act
        var result = await _service.GetCourseRatingAsync(courseId);

        // Assert
        result.AverageRating.Should().Be(3.3); // (5+4+1)/3 = 3.333
        result.TotalReviews.Should().Be(3);
    }

    [Test]
    public async Task DeleteReviewAsync_ShouldCallRepo_WhenUserIsAuthor()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var review = new Review { Id = reviewId, StudentId = studentId };
        _repoMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

        // Act
        await _service.DeleteReviewAsync(reviewId, studentId, "Student");

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Review>()), Times.Once);
    }

    [Test]
    public void DeleteReviewAsync_ShouldThrowUnauthorized_WhenUserIsNotAuthorOrAdmin()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var review = new Review { Id = reviewId, StudentId = authorId };
        _repoMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

        // Act & Assert
        Func<Task> act = async () => await _service.DeleteReviewAsync(reviewId, otherUserId, "Student");
        act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
