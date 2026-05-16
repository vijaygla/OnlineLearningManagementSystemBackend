using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace EnrollmentService.Tests;

[TestFixture]
public class EnrollmentServiceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void PlaceholderTest_ShouldPass()
    {
        true.Should().BeTrue();
    }
}
