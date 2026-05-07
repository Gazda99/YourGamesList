using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Telemetry;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;

namespace YourGamesList.Api.Services.HealthChecks;

public class YourGamesListDatabaseHealthCheck : IHealthCheck
{
    private readonly ILogger<YourGamesListDatabaseHealthCheck> _logger;
    private readonly YglDbContext _yglDbContext;

    public string ServiceName => "Your Games List Database";

    public YourGamesListDatabaseHealthCheck(
        ILogger<YourGamesListDatabaseHealthCheck> logger,
        IDbContextFactory<YglDbContext> yglDbContext
    )
    {
        _logger = logger;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task<ServiceStatusDto> CheckHealth(CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.HealthCheck.Start("Your Games List Database");

        HealthCheckStatusDto status;

        try
        {
            var check = await _yglDbContext.Database.CanConnectAsync(cancellationToken);


            if (check)
            {
                _logger.LogInformation($"{ServiceName} is healthy.");

                status = HealthCheckStatusDto.Healthy;
                activity.SetHealthCheckStatus(status);
                return new ServiceStatusDto()
                {
                    Name = ServiceName,
                    Status = status
                };
            }

            _logger.LogWarning($"{ServiceName} is unhealthy. Reason: Cannot connect to the database.");

            status = HealthCheckStatusDto.Unhealthy;
            activity.SetHealthCheckStatus(status);
            return new ServiceStatusDto()
            {
                Name = ServiceName,
                Status = status
            };
        }
        //Cancellation token cancelled
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, $"{ServiceName} is degraded. Reason: Operation cancelled while checking database connectivity.");
            status = HealthCheckStatusDto.Degraded;
            activity.SetHealthCheckStatus(status);
            return new ServiceStatusDto()
            {
                Name = ServiceName,
                Status = HealthCheckStatusDto.Degraded
            };
        }
    }
}