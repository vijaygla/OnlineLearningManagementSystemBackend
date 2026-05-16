using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace CategoryService.Tests;

[TestFixture]
public class CategoryServiceTests
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
