using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Services.Auth.Filters;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Common.Http;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth.Filters;

public class ApiKeyAuthFilterAttributeTests
{
    private IFixture _fixture;
    private string _requiredApiKeyName;
    private ILogger<ApiKeyAuthFilterAttribute> _logger;
    private IOptions<ApiKeysOptions> _apiKeyOptions;


    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _requiredApiKeyName = _fixture.Create<string>();
        _logger = Substitute.For<ILogger<ApiKeyAuthFilterAttribute>>();
        _apiKeyOptions = Substitute.For<IOptions<ApiKeysOptions>>();
    }

    [Test]
    public async Task OnActionExecutionAsync_OnCorrectApiKey_Passes()
    {
        //ARRANGE
        var apiKey = _fixture.Create<string>();
        var options = new ApiKeysOptions()
        {
            Keys = new Dictionary<string, string>()
            {
                { _requiredApiKeyName, apiKey }
            }
        };
        _apiKeyOptions.Value.Returns(options);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[HttpHeaders.ApiKeyHeader] = apiKey;
        var (context, next) = GetContextAndNext(httpContext);

        var filterAttribute = new ApiKeyAuthFilterAttribute(_requiredApiKeyName, _logger, _apiKeyOptions);

        //ACT
        await filterAttribute.OnActionExecutionAsync(context, next);

        //ASSERT
        _logger.ReceivedLog(LogLevel.Debug, "Api key correct");
    }

    [Test]
    public async Task OnActionExecutionAsync_OnEmptyHeader_Returns401()
    {
        //ARRANGE
        var apiKey = _fixture.Create<string>();
        var options = new ApiKeysOptions()
        {
            Keys = new Dictionary<string, string>()
            {
                { _requiredApiKeyName, apiKey }
            }
        };
        _apiKeyOptions.Value.Returns(options);

        // We are not settings api key header this time!
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var (context, next) = GetContextAndNext(httpContext);

        var filterAttribute = new ApiKeyAuthFilterAttribute(_requiredApiKeyName, _logger, _apiKeyOptions);

        //ACT
        await filterAttribute.OnActionExecutionAsync(context, next);

        //ASSERT
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(401));
        var body = await ReadResponseBody(httpContext.Response.Body);
        Assert.That(body, Is.EquivalentTo("Missing Api Key"));
        _logger.ReceivedLog(LogLevel.Warning, "Missing Api Key");
    }

    [Test]
    public async Task OnActionExecutionAsync_OnInvalidApiKey_Returns401()
    {
        //ARRANGE
        var apiKey = _fixture.Create<string>();
        var wrongApiKey = _fixture.Create<string>();
        var options = new ApiKeysOptions()
        {
            Keys = new Dictionary<string, string>()
            {
                { _requiredApiKeyName, apiKey }
            }
        };
        _apiKeyOptions.Value.Returns(options);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Headers[HttpHeaders.ApiKeyHeader] = wrongApiKey;
        var (context, next) = GetContextAndNext(httpContext);

        var filterAttribute = new ApiKeyAuthFilterAttribute(_requiredApiKeyName, _logger, _apiKeyOptions);

        //ACT
        await filterAttribute.OnActionExecutionAsync(context, next);

        //ASSERT
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(401));
        var body = await ReadResponseBody(httpContext.Response.Body);
        Assert.That(body, Is.EquivalentTo("Wrong Api Key"));
        _logger.ReceivedLog(LogLevel.Warning, "Wrong Api Key");
    }

    private static (ActionExecutingContext context, ActionExecutionDelegate next) GetContextAndNext(HttpContext httpContext)
    {
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object() // Controller instance
        );

        ActionExecutionDelegate next = () =>
        {
            var ctx = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object());
            return Task.FromResult(ctx);
        };

        return (context, next);
    }

    private static async Task<string> ReadResponseBody(Stream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(body, Encoding.UTF8);
        var actualResponseBody = await reader.ReadToEndAsync();
        return actualResponseBody;
    }
}