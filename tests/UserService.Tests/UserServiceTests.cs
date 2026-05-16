using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace UserService.Tests;

[TestFixture]
public class UserServiceTests
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
