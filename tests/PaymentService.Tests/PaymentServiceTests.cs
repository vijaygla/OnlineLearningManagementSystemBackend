using NUnit.Framework;
using Moq;
using FluentAssertions;

namespace PaymentService.Tests;

[TestFixture]
public class PaymentServiceTests
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
