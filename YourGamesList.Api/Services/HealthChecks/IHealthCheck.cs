using System.Threading;
using System.Threading.Tasks;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.HealthChecks;

public interface IHealthCheck
{
    public string ServiceName { get; }
    Task<ServiceStatusDto> CheckHealth(CancellationToken cancellationToken = default);
}