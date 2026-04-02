using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Services.HealthChecks;
using YourGamesList.Api.Services.HealthChecks.Options;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Responses.HealthChecks;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.HealthChecks;

public class HealthCheckServiceTests
{
    private const string HealthCheckCacheKey = "ygl-health-check";

    private IFixture _fixture;
    private ILogger<HealthCheckService> _logger;
    private IOptions<HealthCheckServiceOptions> _options;
    private ICacheProvider _cacheProvider;
    private TimeProvider _timeProvider;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<HealthCheckService>>();

        _cacheProvider = Substitute.For<ICacheProvider>();
        _options = Substitute.For<IOptions<HealthCheckServiceOptions>>();
        _timeProvider = Substitute.For<TimeProvider>();
    }

    [Test]
    public async Task CheckHealth_CachedValue_ReturnsCachedValue()
    {
        //ARRANGE
        var expectedResponse = _fixture.Create<HealthCheckResponse>();
        _cacheProvider.Get<HealthCheckResponse>(Arg.Any<string>()).Returns(CombinedResult<HealthCheckResponse, CacheProviderError>.Success(expectedResponse));
        var healthCheckService = new HealthCheckService(_logger, _options, _cacheProvider, _timeProvider, []);

        //ACT
        var res = await healthCheckService.CheckHealth();

        //ASSERT
        Assert.That(res, Is.EqualTo(expectedResponse));
        _logger.ReceivedLog(LogLevel.Information, "Returning cached value for health check.");
    }

    [Test]
    public async Task CheckHealth_NoHealthChecksConfigured_ReturnsHealthyWithEmptyServices()
    {
        //ARRANGE
        var cacheError = _fixture.Create<CacheProviderError>();
        var options = new HealthCheckServiceOptions()
        {
            TimeoutInSeconds = 100,
            CacheDurationInSeconds = 200
        };
        _options.Value.Returns(options);
        _cacheProvider.Get<HealthCheckResponse>(Arg.Any<string>()).Returns(CombinedResult<HealthCheckResponse, CacheProviderError>.Failure(cacheError));
        var now = DateTimeOffset.UtcNow;
        _timeProvider.GetUtcNow().Returns(now);
        IEnumerable<IHealthCheck> emptyHealthChecks = [];
        var healthCheckService = new HealthCheckService(_logger, _options, _cacheProvider, _timeProvider, emptyHealthChecks);

        //ACT
        var res = await healthCheckService.CheckHealth();

        //ASSERT
        Assert.That(res.Status, Is.EqualTo(HealthCheckStatusDto.Healthy));
        Assert.That(res.CheckedAt, Is.EqualTo(now));
        _logger.NotReceivedLog(LogLevel.Information, "Returning cached value for health check.");
        _logger.ReceivedLog(LogLevel.Information, "No health checks configured. Returning empty list with healthy status.");
        _logger.ReceivedLog(LogLevel.Information, $"Health check status saved into cache for {_options.Value.CacheDurationInSeconds} seconds.");
        await _cacheProvider.Received(1).Set(HealthCheckCacheKey,
            Arg.Is<HealthCheckResponse>(x => x.Status == HealthCheckStatusDto.Healthy && x.CheckedAt == now),
            Arg.Is<TimeSpan>(x => x.TotalSeconds == options.CacheDurationInSeconds));
    }

    [Test]
    [TestCaseSource(nameof(RunChecksTestCases))]
    public async Task CheckHealth_RunChecks_ReturnsHealthCheckResponse(List<IHealthCheck> healthChecks, HealthCheckStatusDto aggregatedStatus)
    {
        //ARRANGE
        var n = healthChecks.Count;
        var taskNames = string.Join(", ", healthChecks.Select(x =>  $"'{x.ServiceName}'").ToArray());
        var cacheError = _fixture.Create<CacheProviderError>();
        var options = new HealthCheckServiceOptions()
        {
            TimeoutInSeconds = 100,
            CacheDurationInSeconds = 200
        };
        _options.Value.Returns(options);
        _cacheProvider.Get<HealthCheckResponse>(Arg.Any<string>()).Returns(CombinedResult<HealthCheckResponse, CacheProviderError>.Failure(cacheError));
        var now = DateTimeOffset.UtcNow;
        _timeProvider.GetUtcNow().Returns(now);
        var healthCheckService = new HealthCheckService(_logger, _options, _cacheProvider, _timeProvider, healthChecks);

        //ACT
        var res = await healthCheckService.CheckHealth();

        //ASSERT
        Assert.That(res.Status, Is.EqualTo(aggregatedStatus));
        Assert.That(res.CheckedAt, Is.EqualTo(now));
        _logger.NotReceivedLog(LogLevel.Information, "Returning cached value for health check.");
        _logger.ReceivedLog(LogLevel.Information, $"Starting health check with a timeout of {options.TimeoutInSeconds} seconds for {n} tasks: {taskNames}.");
        _logger.ReceivedLog(LogLevel.Information, $"Health check complete. Status: {aggregatedStatus}");
        _logger.ReceivedLog(LogLevel.Information, $"Health check status saved into cache for {_options.Value.CacheDurationInSeconds} seconds.");
        await _cacheProvider.Received(1).Set(HealthCheckCacheKey, Arg.Is<HealthCheckResponse>(x => x.Status == aggregatedStatus && x.CheckedAt == now),
            Arg.Is<TimeSpan>(x => x.TotalSeconds == options.CacheDurationInSeconds));
    }

    private static IEnumerable<TestCaseData> RunChecksTestCases()
    {
        var c1 = Substitute.For<IHealthCheck>();
        var c2 = Substitute.For<IHealthCheck>();
        c1.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c1", Status = HealthCheckStatusDto.Healthy });
        c2.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c2", Status = HealthCheckStatusDto.Healthy });

        yield return new TestCaseData(new List<IHealthCheck>() { c1, c2 }, HealthCheckStatusDto.Healthy)
            .SetName("All healthy");

        var c3 = Substitute.For<IHealthCheck>();
        var c4 = Substitute.For<IHealthCheck>();
        c3.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c3", Status = HealthCheckStatusDto.Healthy });
        c4.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c4", Status = HealthCheckStatusDto.Degraded });

        yield return new TestCaseData(new List<IHealthCheck>() { c3, c4 }, HealthCheckStatusDto.Degraded)
            .SetName("At least one degraded");

        var c5 = Substitute.For<IHealthCheck>();
        var c6 = Substitute.For<IHealthCheck>();
        var c7 = Substitute.For<IHealthCheck>();
        c5.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c5", Status = HealthCheckStatusDto.Healthy });
        c6.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c6", Status = HealthCheckStatusDto.Degraded });
        c7.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c7", Status = HealthCheckStatusDto.Unhealthy });

        yield return new TestCaseData(new List<IHealthCheck>() { c5, c6, c7 }, HealthCheckStatusDto.Unhealthy)
            .SetName("At least one unhealthy");

        var c8 = Substitute.For<IHealthCheck>();
        var c9 = Substitute.For<IHealthCheck>();
        c8.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c8", Status = HealthCheckStatusDto.Degraded });
        c9.CheckHealth(Arg.Any<CancellationToken>()).Returns(new ServiceStatusDto() { Name = "c9", Status = HealthCheckStatusDto.Degraded });

        yield return new TestCaseData(new List<IHealthCheck>() { c8, c9 }, HealthCheckStatusDto.Unhealthy)
            .SetName("All degraded");
    }
}