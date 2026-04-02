using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        try
        {
            var check = await _yglDbContext.Database.CanConnectAsync(cancellationToken);

            if (check)
            {
                _logger.LogInformation($"{ServiceName} is healthy.");
                return new ServiceStatusDto()
                {
                    Name = ServiceName,
                    Status = HealthCheckStatusDto.Healthy
                };
            }

            _logger.LogWarning($"{ServiceName} is unhealthy. Reason: Cannot connect to the database.");
            return new ServiceStatusDto()
            {
                Name = ServiceName,
                Status = HealthCheckStatusDto.Unhealthy
            };
        }
        //Cancellation token cancelled
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, $"{ServiceName} is degraded. Reason: Operation cancelled while checking database connectivity.");
            return new ServiceStatusDto()
            {
                Name = ServiceName,
                Status = HealthCheckStatusDto.Degraded
            };
        }
    }
}