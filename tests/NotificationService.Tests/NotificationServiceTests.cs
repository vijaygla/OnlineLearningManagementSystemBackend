using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace NotificationService.Tests;

[TestFixture]
public class NotificationServiceTests
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
