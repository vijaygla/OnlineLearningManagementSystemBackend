using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace SearchService.Tests;

[TestFixture]
public class SearchServiceTests
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
