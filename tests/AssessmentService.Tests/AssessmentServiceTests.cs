using NUnit.Framework;
using Moq;
using FluentAssertions;
using AssessmentService.Application.Services;
using AssessmentService.Application.Interfaces;
using AssessmentService.Application.DTOs;
using AssessmentService.Domain.Entities;

namespace AssessmentService.Tests;

[TestFixture]
public class AssessmentServiceTests
{
    private Mock<IAssessmentRepository> _repoMock;
    private Application.Services.AssessmentService _service;

    [SetUp]
    public void Setup()
    {
        _repoMock = new Mock<IAssessmentRepository>();
        _service = new Application.Services.AssessmentService(_repoMock.Object);
    }

    [Test]
    public async Task CreateQuizAsync_ShouldReturnQuizDto_AndCallRepository()
    {
        // Arrange
        var request = new CreateQuizRequest(Guid.NewGuid(), "Title", "Desc", 70);
        var instructorId = "instructor1";

        // Act
        var result = await _service.CreateQuizAsync(request, instructorId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        _repoMock.Verify(r => r.AddQuizAsync(It.IsAny<Quiz>()), Times.Once);
    }

    [Test]
    public async Task UpdateQuizAsync_ShouldUpdateQuiz_WhenQuizExists()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var existingQuiz = new Quiz { Id = quizId, Title = "Old" };
        _repoMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync(existingQuiz);
        var request = new CreateQuizRequest(Guid.NewGuid(), "New Title", "New Desc", 80);

        // Act
        await _service.UpdateQuizAsync(quizId, request);

        // Assert
        existingQuiz.Title.Should().Be("New Title");
        _repoMock.Verify(r => r.UpdateQuizAsync(existingQuiz), Times.Once);
    }

    [Test]
    public void UpdateQuizAsync_ShouldThrowKeyNotFoundException_WhenQuizDoesNotExist()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync((Quiz)null!);
        var request = new CreateQuizRequest(Guid.NewGuid(), "Title", "Desc", 70);

        // Act & Assert
        Func<Task> act = async () => await _service.UpdateQuizAsync(quizId, request);
        act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Test]
    public async Task SubmitQuizAsync_ShouldReturnPassingResult_WhenAnswersAreCorrect()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var quiz = new Quiz 
        { 
            Id = quizId, 
            PassingScore = 70,
            Questions = new List<Question> 
            { 
                new Question { CorrectOptionIndex = 0 },
                new Question { CorrectOptionIndex = 1 }
            }
        };
        _repoMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync(quiz);
        var request = new SubmitQuizRequest(new List<int> { 0, 1 });

        // Act
        var result = await _service.SubmitQuizAsync(quizId, studentId, request);

        // Assert
        result.Score.Should().Be(100);
        result.IsPassed.Should().BeTrue();
        _repoMock.Verify(r => r.AddSubmissionAsync(It.IsAny<QuizSubmission>()), Times.Once);
    }

    [Test]
    public async Task SubmitQuizAsync_ShouldReturnFailingResult_WhenAnswersAreIncorrect()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var quiz = new Quiz 
        { 
            Id = quizId, 
            PassingScore = 70,
            Questions = new List<Question> 
            { 
                new Question { CorrectOptionIndex = 0 },
                new Question { CorrectOptionIndex = 1 }
            }
        };
        _repoMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync(quiz);
        var request = new SubmitQuizRequest(new List<int> { 1, 1 }); // 1 correct, 1 incorrect = 50%

        // Act
        var result = await _service.SubmitQuizAsync(quizId, studentId, request);

        // Assert
        result.Score.Should().Be(50);
        result.IsPassed.Should().BeFalse();
    }
}
