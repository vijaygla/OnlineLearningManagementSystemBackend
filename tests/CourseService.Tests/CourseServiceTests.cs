using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace CourseService.Tests;

[TestFixture]
public class CourseServiceTests
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
