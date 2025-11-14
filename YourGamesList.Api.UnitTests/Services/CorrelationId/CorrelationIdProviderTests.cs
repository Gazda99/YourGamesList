using System;
using YourGamesList.Api.Services.CorrelationId;

namespace YourGamesList.Api.UnitTests.Services.CorrelationId;

public class CorrelationIdProviderTests
{
    [Test]
    [TestCaseSource(nameof(IsValidCorrelationIdTestCases))]
    public void IsValidCorrelationId_ReturnsExpectedResult(string corId, bool expectedResult)
    {
        // ARRANGE
        var provider = new CorrelationIdProvider();

        // ACT
        var result = provider.IsValidCorrelationId(corId);

        // ASSERT
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private static readonly TestCaseData[] IsValidCorrelationIdTestCases =
    [
        new TestCaseData(Guid.NewGuid().ToString("N"), true).SetName("Valid GUID without dashes"),
        new TestCaseData(Guid.NewGuid().ToString(), false).SetName("Valid GUID with dashes"),
        new TestCaseData("invalid-guid", false).SetName("Invalid GUID string"),
        new TestCaseData(string.Empty, false).SetName("Empty string"),
        new TestCaseData(null!, false).SetName("Null string")
    ];

    [Test]
    public void GetCorrelationId_ReturnsValidCorrelationId()
    {
        // ARRANGE
        var provider = new CorrelationIdProvider();

        // ACT
        var result = provider.GetCorrelationId();

        // ASSERT
        var assertion = Guid.TryParseExact(result, "N", out _);
        Assert.That(assertion, Is.True);
    }
}