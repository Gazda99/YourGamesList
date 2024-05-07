using Microsoft.AspNetCore.Http;
using YourGamesList.Common.Http;

namespace YourGamesList.Common.UnitTests.Http;

public class HeadersExtensionsTests
{
    [Test]
    public void GetCorrelationId_Should_ReturnCorrelationId()
    {
        //GIVEN
        var corId = CreateCorrelationId();
        var headers = new HeaderDictionary { { HttpHelper.HeaderCorrelationIdName, corId } };

        //WHEN
        var obtainedCorId = headers.GetCorrelationId();

        //THEN
        obtainedCorId.Should().BeEquivalentTo(corId);
    }

    [Test]
    public void AddCorrelationId_Should_AddCorrelationId()
    {
        //GIVEN
        var corId = CreateCorrelationId();
        var headers = new HeaderDictionary();

        //WHEN
        headers.AddCorrelationId(corId);

        //THEN
        headers.TryGetValue(HttpHelper.HeaderCorrelationIdName, out var obtainedCorId).Should().BeTrue();
        obtainedCorId.Should().BeEquivalentTo(corId);
    }

    private static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("D");
    }
}