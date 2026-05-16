using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace IdentityService.Tests;

[TestFixture]
public class IdentityServiceTests
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
