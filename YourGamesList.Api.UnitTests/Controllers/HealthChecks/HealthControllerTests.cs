using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers.HealthChecks;
using YourGamesList.Api.Services.HealthChecks;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Responses.HealthChecks;

namespace YourGamesList.Api.UnitTests.Controllers.HealthChecks;

public class HealthControllerTests
{
    private IFixture _fixture;
    private ILogger<HealthController> _logger;
    private IHealthCheckService _healthCheckService;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<HealthController>>();
        _healthCheckService = Substitute.For<IHealthCheckService>();
    }

    #region CheckHealth

    [Test]
    [TestCaseSource(nameof(CheckHealthHealthCheckStatusDtoExamples))]
    public async Task CheckHealth_SuccessScenario(HealthCheckStatusDto healthStatus, int httpStatusReturnCode)
    {
        //ARRANGE
        var healthCheckResponse = _fixture.Build<HealthCheckResponse>()
            .With(x => x.Status, healthStatus)
            .WithAutoProperties()
            .Create();
        var combinedRes = CombinedResult<HealthCheckResponse, HealthCheckError>.Success(healthCheckResponse);
        _healthCheckService.CheckHealth().Returns(combinedRes);

        var controller = new HealthController(_logger, _healthCheckService);

        //ACT
        var res = await controller.CheckHealth();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(httpStatusReturnCode));
        await _healthCheckService.Received(1).CheckHealth();
    }


    [Test]
    public async Task CheckHealth_AlreadyInProgress()
    {
        //ARRANGE
        var combinedRes = CombinedResult<HealthCheckResponse, HealthCheckError>.Failure(HealthCheckError.AlreadyInProgress);
        _healthCheckService.CheckHealth().Returns(combinedRes);

        var controller = new HealthController(_logger, _healthCheckService);

        //ACT
        var res = await controller.CheckHealth();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status429TooManyRequests));
        await _healthCheckService.Received(1).CheckHealth();
    }

    #endregion

    public static readonly TestCaseData[] CheckHealthHealthCheckStatusDtoExamples =
    [
        new TestCaseData(HealthCheckStatusDto.Healthy, StatusCodes.Status200OK).SetName("Healthy"),
        new TestCaseData(HealthCheckStatusDto.Degraded, StatusCodes.Status200OK).SetName("Degraded"),
        new TestCaseData(HealthCheckStatusDto.Unhealthy, StatusCodes.Status503ServiceUnavailable).SetName("Unhealthy")
    ];
}