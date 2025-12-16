using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using YourGamesList.Api.Middlewares;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Common.Logging;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Middlewares;

public class JwtUserInformationMiddlewareTests
{
    private ILogger<JwtUserInformationMiddleware> _logger;
    private ITokenParser _tokenParser;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<JwtUserInformationMiddleware>>();
        _tokenParser = Substitute.For<ITokenParser>();
    }

    [Test]
    public async Task InvokeAsync_OnValidToken_CorrectlySetsHttpContextItemAndAddsScopeToLogging()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var userId = Guid.NewGuid();
        var (rawToken, token) =
            JwtHelper.CreateToken([new Claim(JwtRegisteredClaimNames.Sub, username), new Claim(JwtCustomClaimNames.UserId, userId.ToString())]);
        _tokenParser.CanReadToken(rawToken).Returns(true);
        _tokenParser.ReadJsonWebToken(rawToken).Returns(token);

        var endpoint = Substitute.For<IEndpointFeature>();
        RequestDelegate endpointDelegate = async (innerHttpContext) => { await Task.CompletedTask; };
        endpoint.Endpoint = new Endpoint(endpointDelegate, new EndpointMetadataCollection(new AuthorizeAttribute()), _fixture.Create<string>());

        HttpContext ctx = new DefaultHttpContext();
        ctx.Features.Set<IEndpointFeature>(endpoint);
        ctx.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {rawToken}");

        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var jwtUserInformationMiddleware = new JwtUserInformationMiddleware(next, _logger, _tokenParser);

        //ACT
        await jwtUserInformationMiddleware.InvokeAsync(ctx);

        //ASSERT
        Assert.That(ctx.Items.ContainsKey(nameof(JwtUserInformation)));
        var storedJwtUserInformationEntry = ctx.Items[nameof(JwtUserInformation)];
        Assert.That(storedJwtUserInformationEntry, Is.Not.Null);
        Assert.That(storedJwtUserInformationEntry, Is.TypeOf<JwtUserInformation>());
        var storedJwtUserInformation = (JwtUserInformation) storedJwtUserInformationEntry;
        Assert.That(storedJwtUserInformation.UserId, Is.EqualTo(userId));
        Assert.That(storedJwtUserInformation.Username, Is.EqualTo(username));
        _logger.Received(1)
            .BeginScope(Arg.Is<Dictionary<string, object>>(x => x.ContainsKey(LogProperties.UserId) && x.ContainsValue(userId.ToString())));
    }

    [Test]
    public async Task InvokeAsync_OnNoAuthorizeAttribute_SkipsExecution()
    {
        //ARRANGE
        var endpoint = Substitute.For<IEndpointFeature>();
        RequestDelegate endpointDelegate = async (innerHttpContext) => { await Task.CompletedTask; };
        endpoint.Endpoint = new Endpoint(endpointDelegate, new EndpointMetadataCollection(), _fixture.Create<string>());

        HttpContext ctx = new DefaultHttpContext();
        ctx.Features.Set<IEndpointFeature>(endpoint);

        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var jwtUserInformationMiddleware = new JwtUserInformationMiddleware(next, _logger, _tokenParser);

        //ACT
        await jwtUserInformationMiddleware.InvokeAsync(ctx);

        //ASSERT
        Assert.That(!ctx.Items.ContainsKey(nameof(JwtUserInformation)));
        _logger.Received(0).BeginScope(Arg.Is<Dictionary<string, object>>(x => x.ContainsKey(LogProperties.UserId)));
    }


    [Test]
    [TestCaseSource(nameof(WrongAuthHeaderCases))]
    public async Task InvokeAsync_OnWrongAuthHeader_ReturnsUnauthorized(string headerName, string headerValue)
    {
        //ARRANGE
        var endpoint = Substitute.For<IEndpointFeature>();
        RequestDelegate endpointDelegate = async (innerHttpContext) => { await Task.CompletedTask; };
        endpoint.Endpoint = new Endpoint(endpointDelegate, new EndpointMetadataCollection(new AuthorizeAttribute()), _fixture.Create<string>());

        HttpContext ctx = new DefaultHttpContext();
        ctx.Features.Set<IEndpointFeature>(endpoint);
        ctx.Request.Headers.Append(headerName, headerValue);

        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var jwtUserInformationMiddleware = new JwtUserInformationMiddleware(next, _logger, _tokenParser);

        //ACT
        await jwtUserInformationMiddleware.InvokeAsync(ctx);

        //ASSERT
        Assert.That(ctx.Response.StatusCode, Is.EqualTo((int) HttpStatusCode.Unauthorized));
    }

    private static readonly TestCaseData[] WrongAuthHeaderCases =
    [
        new TestCaseData(HeaderNames.Authorization, "wrong-value").SetName("Wrong value"),
        new TestCaseData("NoAuthHeaderExample", "whatever").SetName("No auth header")
    ];

    [Test]
    public async Task InvokeAsync_OnUnparsableToken_ReturnsUnauthorized()
    {
        //ARRANGE
        var unparsableToken = _fixture.Create<string>();
        _tokenParser.CanReadToken(unparsableToken).Returns(false);

        var endpoint = Substitute.For<IEndpointFeature>();
        RequestDelegate endpointDelegate = async (innerHttpContext) => { await Task.CompletedTask; };
        endpoint.Endpoint = new Endpoint(endpointDelegate, new EndpointMetadataCollection(new AuthorizeAttribute()), _fixture.Create<string>());

        HttpContext ctx = new DefaultHttpContext();
        ctx.Features.Set<IEndpointFeature>(endpoint);
        ctx.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {unparsableToken}");

        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var jwtUserInformationMiddleware = new JwtUserInformationMiddleware(next, _logger, _tokenParser);

        //ACT
        await jwtUserInformationMiddleware.InvokeAsync(ctx);

        //ASSERT
        Assert.That(ctx.Response.StatusCode, Is.EqualTo((int) HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task InvokeAsync_OnTokenWithMissingClaims_ReturnsUnauthorized()
    {
        //ARRANGE
        var (rawToken, token) = JwtHelper.CreateToken();
        _tokenParser.CanReadToken(rawToken).Returns(false);

        var endpoint = Substitute.For<IEndpointFeature>();
        RequestDelegate endpointDelegate = async (innerHttpContext) => { await Task.CompletedTask; };
        endpoint.Endpoint = new Endpoint(endpointDelegate, new EndpointMetadataCollection(new AuthorizeAttribute()), _fixture.Create<string>());

        HttpContext ctx = new DefaultHttpContext();
        ctx.Features.Set<IEndpointFeature>(endpoint);
        ctx.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {rawToken}");

        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var jwtUserInformationMiddleware = new JwtUserInformationMiddleware(next, _logger, _tokenParser);

        //ACT
        await jwtUserInformationMiddleware.InvokeAsync(ctx);

        //ASSERT
        Assert.That(ctx.Response.StatusCode, Is.EqualTo((int) HttpStatusCode.Unauthorized));
    }
}