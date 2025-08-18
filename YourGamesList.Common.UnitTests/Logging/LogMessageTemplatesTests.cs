using AutoFixture;
using YourGamesList.Common.Logging;
using YourGamesList.Common.Refit;

namespace YourGamesList.Common.UnitTests.Logging;

public class LogMessageTemplatesTests
{
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void NetworkFailure_ReturnsCorrectLogMessageTemplate()
    {
        //ARRANGE
        var httpFailureReason = _fixture.Create<HttpFailureReason>();
        var destinationServiceName = _fixture.Create<string>();

        //ACT
        var msg = LogMessageTemplates.NetworkFailure(httpFailureReason, destinationServiceName);

        //ASSERT
        Assert.That(msg, Is.EquivalentTo($"Network error '{httpFailureReason.ToString()}' while making request to '{destinationServiceName}'."));
    }
}