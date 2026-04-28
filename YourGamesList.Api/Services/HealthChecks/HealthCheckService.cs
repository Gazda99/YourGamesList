using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.HealthChecks.Options;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Responses.HealthChecks;

namespace YourGamesList.Api.Services.HealthChecks;

public interface IHealthCheckService
{
    Task<CombinedResult<HealthCheckResponse, HealthCheckError>> CheckHealth();
}

public class HealthCheckService : IHealthCheckService
{
    private const string HealthCheckCacheKey = "ygl-health-check";

    private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

    private readonly ILogger<HealthCheckService> _logger;
    private readonly IOptions<HealthCheckServiceOptions> _options;
    private readonly ICacheProvider _cacheProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IEnumerable<IHealthCheck> _healthChecks;

    public HealthCheckService(
        ILogger<HealthCheckService> logger,
        IOptions<HealthCheckServiceOptions> options,
        [FromKeyedServices(CacheProviders.InMemory)]
        ICacheProvider cacheProvider,
        TimeProvider timeProvider,
        IEnumerable<IHealthCheck> healthChecks)
    {
        _logger = logger;
        _options = options;
        _cacheProvider = cacheProvider;
        _healthChecks = healthChecks;
        _timeProvider = timeProvider;
    }

    public async Task<CombinedResult<HealthCheckResponse, HealthCheckError>> CheckHealth()
    {
        var readFromCache = await _cacheProvider.Get<HealthCheckResponse>(HealthCheckCacheKey);
        if (readFromCache.IsSuccess)
        {
            _logger.LogInformation("Returning cached value for health check.");
            return CombinedResult<HealthCheckResponse, HealthCheckError>.Success(readFromCache.Value);
        }

        if (SemaphoreSlim.CurrentCount <= 0)
        {
            _logger.LogInformation("Health check already in progress.");
            return CombinedResult<HealthCheckResponse, HealthCheckError>.Failure(HealthCheckError.AlreadyInProgress);
        }

        await SemaphoreSlim.WaitAsync();

        try
        {
            HealthCheckResponse response;

            if (!_healthChecks.Any())
            {
                _logger.LogInformation("No health checks configured. Returning empty list with healthy status.");
                response = new HealthCheckResponse()
                {
                    Services = [],
                    Status = HealthCheckStatusDto.Healthy,
                    CheckedAt = _timeProvider.GetUtcNow()
                };
            }
            else
            {
                var cts = new CancellationTokenSource();
                var timeout = TimeSpan.FromSeconds(_options.Value.TimeoutInSeconds);
                cts.CancelAfter(timeout);
                var token = cts.Token;

                var tasks = _healthChecks.Select(x => x.CheckHealth(token)).ToArray();
                var taskNames = string.Join(", ", _healthChecks.Select(x => $"'{x.ServiceName}'").ToArray());

                _logger.LogInformation($"Starting health check with a timeout of {timeout.TotalSeconds} seconds for {tasks.Length} tasks: {taskNames}.");

                var serviceStatuses = await Task.WhenAll(tasks);

                var aggregatedStatus = AggregateStatus(serviceStatuses);

                _logger.LogInformation($"Health check complete. Status: {aggregatedStatus}");

                response = new HealthCheckResponse()
                {
                    Services = serviceStatuses,
                    Status = aggregatedStatus,
                    CheckedAt = _timeProvider.GetUtcNow()
                };
            }

            await _cacheProvider.Set(HealthCheckCacheKey, response, TimeSpan.FromSeconds(_options.Value.CacheDurationInSeconds));
            _logger.LogInformation($"Health check status saved into cache for {_options.Value.CacheDurationInSeconds} seconds.");

            return CombinedResult<HealthCheckResponse, HealthCheckError>.Success(response);
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    /// <summary>
    /// <para>Unhealthy -> ANY service is unhealthy, or ALL services are degraded</para>
    /// <para>Degraded -> ANY service is degraded</para>
    /// <para>Healthy -> ALL services are healthy</para>
    /// </summary>
    private static HealthCheckStatusDto AggregateStatus(ServiceStatusDto[] serviceStatuses)
    {
        if (serviceStatuses.Any(x => x.Status == HealthCheckStatusDto.Unhealthy) || serviceStatuses.All(x => x.Status == HealthCheckStatusDto.Degraded))
        {
            return HealthCheckStatusDto.Unhealthy;
        }

        if (serviceStatuses.Any(x => x.Status == HealthCheckStatusDto.Degraded))
        {
            return HealthCheckStatusDto.Degraded;
        }

        return HealthCheckStatusDto.Healthy;
    }
}