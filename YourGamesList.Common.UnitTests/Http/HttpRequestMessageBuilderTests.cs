using TddXt.AnyRoot.Numbers;
using TddXt.AnyRoot.Strings;
using YourGamesList.Common.Http;

namespace YourGamesList.Common.UnitTests.Http;

public class HttpRequestMessageBuilderTests
{
    private HttpRequestMessageBuilder _builder;

    [SetUp]
    public void SetUp()
    {
        _builder = new HttpRequestMessageBuilder();
    }

    [Test]
    public void WithMethod_Should_CreateRequestMessageWithCorrectMethod()
    {
        var method = Any.Instance<HttpMethod>();

        //WHEN
        _builder.WithMethod(method);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Method.Method.Should().BeEquivalentTo(method.Method);
    }


    [Test]
    public void WithUri_Should_CreateRequestMessageWithCorrectUri()
    {
        const string uri = "http://127.0.0.1/";

        //WHEN
        _builder.WithUri(uri);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.RequestUri.AbsoluteUri.Should().BeEquivalentTo(uri);
    }

    [Test]
    public void WithUri_Should_CreateRequestMessageWithCorrectUriWhenAdditionalPathArgumentsProvided()
    {
        const string uri = "http://127.0.0.1/";
        var anyInt = Any.Integer().ToString();
        var queryParams = new Dictionary<string, string>()
        {
            { "a", "b" },
            { "c", anyInt }
        };
        var expecteduri = $"http://127.0.0.1/?a=b&c={anyInt}";

        //WHEN
        _builder.WithUri(uri, queryParams);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.RequestUri.AbsoluteUri.Should().BeEquivalentTo(expecteduri);
    }

    [Test]
    public void WithHeader_Should_CreateRequestMessageWithCorrectHeader()
    {
        var key = Any.String();
        var val = Any.String();

        //WHEN
        _builder.WithHeader(key, val);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Headers.Should().ContainKey(key);
        requestMessage.Headers.GetValues(key).Should().HaveCount(1);
        requestMessage.Headers.GetValues(key).Should().Contain(val);
    }

    [Test]
    public void WithHeader_Should_CreateRequestMessageWithCorrectHeader_2()
    {
        var key = Any.String();
        var val = Any.String();
        //WHEN
        _builder.WithHeader((key, val));
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Headers.Should().ContainKey(key);
        requestMessage.Headers.GetValues(key).Should().HaveCount(1);
        requestMessage.Headers.GetValues(key).Should().Contain(val);
    }

    [Test]
    public void WithHeaders_Should_CreateRequestMessageWithCorrectHeader()
    {
        var key1 = Any.String();
        var val1 = Any.String();
        var key2 = Any.String();
        var val2 = Any.String();
        //WHEN
        var headers = new Dictionary<string, string>()
        {
            { key1, val1 },
            { key2, val2 },
        };

        _builder.WithHeaders(headers);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Headers.Should().ContainKey(key1);
        requestMessage.Headers.Should().ContainKey(key2);

        requestMessage.Headers.GetValues(key1).Should().HaveCount(1);
        requestMessage.Headers.GetValues(key1).Should().Contain(val1);

        requestMessage.Headers.GetValues(key2).Should().HaveCount(1);
        requestMessage.Headers.GetValues(key2).Should().Contain(val2);
    }

    [Test]
    public void WithBearerToken_Should_CreateRequestMessageWithBearerTokenHeader()
    {
        const string authorizationTokenHeaderName = "Authorization";
        var token = Any.String();

        //WHEN
        _builder.WithBearerToken(token);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Headers.Should().ContainKey(authorizationTokenHeaderName);
        requestMessage.Headers.GetValues(authorizationTokenHeaderName).Should().HaveCount(1);
        requestMessage.Headers.GetValues(authorizationTokenHeaderName).Should().Contain($"Bearer {token}");
    }


    [Test]
    public void WithStringContent_Should_CreateRequestMessageWithCorrectStringContent()
    {
        var content = Any.String();

        //WHEN
        _builder.WithStringContent(content);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Content.ReadAsStringAsync().Result.Should().BeEquivalentTo(content);
    }


    [TestCase("application/json")]
    [TestCase("text/plain")]
    public void WithStringContent_Should_CreateRequestMessageWithCorrectStringContentAndMediaType(string mediaType)
    {
        var content = Any.String();

        //WHEN
        _builder.WithStringContent(content, mediaType);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Content.ReadAsStringAsync().Result.Should().BeEquivalentTo(content);
        requestMessage.Content.Headers.ContentType.MediaType.Should().BeEquivalentTo(mediaType);
    }

    [Test]
    public void WithFormUrlEncodedContent_Should_CreateRequestMessageWithCorrectFormUrlEncodedContent()
    {
        var content = Any.Instance<Dictionary<string, string>>();
        var expectedContent = content
            .Select(x => $"{x.Key}={x.Value}")
            .Aggregate((a, b) => $"{a}&{b}");

        //WHEN
        _builder.WithFormUrlEncodedContent(content);
        var requestMessage = _builder.Build();

        //THEN
        requestMessage.Content.ReadAsStringAsync().Result.Should().BeEquivalentTo(expectedContent);
    }
}