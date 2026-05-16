using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace ContentService.Tests;

[TestFixture]
public class ContentServiceTests
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
