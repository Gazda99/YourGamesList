using Microsoft.Extensions.Logging;
using TddXt.AnyRoot.Strings;
using YourGamesList.Common.Log;

namespace YourGamesList.Common.UnitTests.Log;

public class LogScopeExtensionsTests
{
    [Test]
    public void With_Should_AddScope()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<object>>();
        var propName = Any.String();
        var propValue = Any.String();

        //WHEN
        logger.With(propName, propValue);

        //THEN
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x =>
            x.ContainsKey(propName)
            && (string) x[propName] == propValue
        ));
    }

    [Test]
    public void WithCorrelationId_Should_AddCorrelationId()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<object>>();
        var corId = LogHelper.CreateCorrelationId();

        //WHEN
        logger.WithCorrelationId(corId);

        //THEN
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x =>
            x.ContainsKey(LogHelper.CorrelationIdPropertyName)
            && (string) x[LogHelper.CorrelationIdPropertyName] == corId
        ));
    }

    [Test]
    public void WithCorrelationId_Should_CreateAndAddCorrelationId()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<object>>();

        //WHEN
        logger.WithCorrelationId();

        //THEN
        Guid guid;
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x =>
            x.ContainsKey(LogHelper.CorrelationIdPropertyName)
            && !string.IsNullOrEmpty((string) x[LogHelper.CorrelationIdPropertyName])
            && Guid.TryParse((string) x[LogHelper.CorrelationIdPropertyName], out guid)
        ));
    }
}