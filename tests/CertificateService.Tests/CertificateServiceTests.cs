using NUnit.Framework;
using Moq;
using FluentAssertions;
using CertificateService.Application.Services;
using CertificateService.Application.Interfaces;
using CertificateService.Application.DTOs;
using CertificateService.Domain.Entities;

namespace CertificateService.Tests;

[TestFixture]
public class CertificateServiceTests
{
    private Mock<ICertificateRepository> _repoMock;
    private Application.Services.CertificateService _service;

    [SetUp]
    public void Setup()
    {
        _repoMock = new Mock<ICertificateRepository>();
        _service = new Application.Services.CertificateService(_repoMock.Object);
    }

    [Test]
    public async Task IssueCertificateAsync_ShouldReturnNewCertificate_WhenNotExists()
    {
        // Arrange
        var request = new IssueCertificateRequest(Guid.NewGuid(), Guid.NewGuid());
        _repoMock.Setup(r => r.GetByStudentAndCourseAsync(request.StudentId, request.CourseId))
                 .ReturnsAsync((Certificate)null!);

        // Act
        var result = await _service.IssueCertificateAsync(request, "System");

        // Assert
        result.Should().NotBeNull();
        result.CertificateNumber.Should().StartWith("CERT-");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Certificate>()), Times.Once);
    }

    [Test]
    public async Task IssueCertificateAsync_ShouldReturnExistingCertificate_WhenAlreadyExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingCert = new Certificate { StudentId = studentId, CourseId = courseId, CertificateNumber = "EXISTING" };
        var request = new IssueCertificateRequest(studentId, courseId);
        _repoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, courseId))
                 .ReturnsAsync(existingCert);

        // Act
        var result = await _service.IssueCertificateAsync(request, "System");

        // Assert
        result.CertificateNumber.Should().Be("EXISTING");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Certificate>()), Times.Never);
    }

    [Test]
    public async Task VerifyCertificateAsync_ShouldReturnValid_WhenCertificateExistsAndNotRevoked()
    {
        // Arrange
        var certNum = "CERT-123";
        var cert = new Certificate { CertificateNumber = certNum, IsRevoked = false };
        _repoMock.Setup(r => r.GetByNumberAsync(certNum)).ReturnsAsync(cert);

        // Act
        var result = await _service.VerifyCertificateAsync(certNum);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Certificate.Should().NotBeNull();
    }

    [Test]
    public async Task VerifyCertificateAsync_ShouldReturnInvalid_WhenCertificateIsRevoked()
    {
        // Arrange
        var certNum = "CERT-REVOKED";
        var cert = new Certificate { CertificateNumber = certNum, IsRevoked = true };
        _repoMock.Setup(r => r.GetByNumberAsync(certNum)).ReturnsAsync(cert);

        // Act
        var result = await _service.VerifyCertificateAsync(certNum);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Certificate.Should().BeNull();
    }
}
