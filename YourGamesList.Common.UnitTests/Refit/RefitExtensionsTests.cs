using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Refit;
using YourGamesList.Common.Refit;
using YourGamesList.TestsUtils;

namespace YourGamesList.Common.UnitTests.Refit;

public class RefitExtensionsTests
{
    private IFixture _fixture;
    private IHandlesRefitFailure _refitApi;
    private ILogger _logger;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _refitApi = Substitute.For<IHandlesRefitFailure>();
        _logger = Substitute.For<ILogger>();
    }

    [Test]
    public async Task TryRefit_WhenCallSucceeds_ReturnsResponse()
    {
        //ARRANGE
        var response = Substitute.For<IApiResponse<string>>();

        //ACT
        var result = await _refitApi.TryRefit(() => Task.FromResult(response));

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.SameAs(response));
    }

    [Test]
    public async Task TryRefit_WhenCallThrowsTaskCanceledException_ReturnsTimeout()
    {
        //ACT
        var destinationName = _fixture.Create<string>();
        var exception = new TaskCanceledException();
        var result = await _refitApi.TryRefit<string>(() => throw exception, _logger, destinationName);

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(RefitFailureReason.Timeout));
        _logger.ReceivedLog(LogLevel.Warning, $"Exception occurred while making HTTP request with Refit to {destinationName}, resulting in error {nameof(RefitFailureReason.Timeout)}", ex => ex == null);
    }

    [Test]
    public async Task TryRefit_WhenCallThrowsHttpRequestException_ReturnsNetworkFailure()
    {
        //ARRANGE
        var destinationName = _fixture.Create<string>();
        var exception = new HttpRequestException("Network unavailable");

        //ACT
        var result = await _refitApi.TryRefit<string>(() => throw exception, _logger, destinationName);

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(RefitFailureReason.Network));
        _logger.ReceivedLog(LogLevel.Warning, $"Exception occurred while making HTTP request with Refit to {destinationName}, resulting in error {RefitFailureReason.Network}", ex => ex == null);
    }

    [Test]
    public async Task TryRefit_WhenCallThrowsOtherException_ReturnsGeneralFailure()
    {
        //ARRANGE
        var destinationName = _fixture.Create<string>();
        var exception = new Exception("Unexpected failure");

        //ACT
        var result = await _refitApi.TryRefit<string>(() => throw exception, _logger, destinationName);

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(RefitFailureReason.General));
        _logger.ReceivedLog(LogLevel.Warning, $"Exception occurred while making HTTP request with Refit to {destinationName}, resulting in error {RefitFailureReason.General}", ex => ex == null);
    }

    [Test]
    public async Task TryRefit_WhenJsonFailsAndDebugIsEnabled_ReturnsDeserializationFailure_And_LogsException()
    {
        //ARRANGE
        var destinationName = _fixture.Create<string>();
        var jsonException = new JsonException("Invalid JSON");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://games.example.test");
        var apiException = await RefitHelper.CreateApiException(request, HttpMethod.Get, HttpStatusCode.BadRequest, jsonException);
        var response = Substitute.For<IApiResponse<string>>();
        response.Error.Returns(apiException);
        _logger.IsEnabled(LogLevel.Debug).Returns(true);

        //ACT
        await _refitApi.TryRefit(() => Task.FromResult(response), _logger, destinationName);

        //ASSERT
        _logger.ReceivedLog(
            LogLevel.Warning,
            $"Exception occurred while making HTTP request with Refit to {destinationName}, resulting in error {RefitFailureReason.Deserialization}",
            exception => exception == jsonException
        );
    }
    
    [Test]
    public async Task TryRefit_WhenJsonFailsAndDebugIsDisabled_ReturnsDeserializationFailure_And_DoesNotLogException()
    {
        //ARRANGE
        var destinationName = _fixture.Create<string>();
        var jsonException = new JsonException("Invalid JSON");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://games.example.test");
        var apiException = await RefitHelper.CreateApiException(request, HttpMethod.Get, HttpStatusCode.BadRequest, jsonException);
        var response = Substitute.For<IApiResponse<string>>();
        response.Error.Returns(apiException);
        _logger.IsEnabled(LogLevel.Debug).Returns(false);

        //ACT
        await _refitApi.TryRefit(() => Task.FromResult(response), _logger, destinationName);

        //ASSERT
        _logger.ReceivedLog(
            LogLevel.Warning,
            $"Exception occurred while making HTTP request with Refit to {destinationName}, resulting in error {RefitFailureReason.Deserialization}",
            exception => exception != jsonException && exception == null
        );
    }
}
