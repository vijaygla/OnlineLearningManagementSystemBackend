using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace MediaService.Tests;

[TestFixture]
public class MediaServiceTests
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
