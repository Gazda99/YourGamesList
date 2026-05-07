using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.HealthChecks;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Responses.HealthChecks;

namespace YourGamesList.Api.Controllers.HealthChecks;

[ApiController]
[Route("health")]
public class HealthController : YourGamesListBaseController
{
    private readonly ILogger<HealthController> _logger;
    private readonly IHealthCheckService _healthCheckService;

    public HealthController(
        ILogger<HealthController> logger,
        IHealthCheckService healthCheckService
    )
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckHealth()
    {
        var res = await _healthCheckService.CheckHealth();

        if (res.IsFailure)
        {
            return Result(StatusCodes.Status429TooManyRequests);
        }

        var healthStatus = res.Value;

        if (healthStatus.Status == HealthCheckStatusDto.Unhealthy)
        {
            return Result(StatusCodes.Status503ServiceUnavailable, healthStatus);
        }
        else
        {
            return Result(StatusCodes.Status200OK, healthStatus);
        }
    }
}