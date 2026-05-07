using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services.HealthChecks;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;
using YourGamesList.Database.TestUtils;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.HealthChecks;

public class YourGamesListDatabaseHealthCheckTests
{
    private IFixture _fixture;
    private ILogger<YourGamesListDatabaseHealthCheck> _logger;

    private TestYglDbContextBuilder _yglDbContextBuilder;
    private IDbContextFactory<YglDbContext> _dbContextFactory;
    private YglDbContext _yglDbContext;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<YourGamesListDatabaseHealthCheck>>();

        _yglDbContextBuilder = TestYglDbContextBuilder.Build();
        _yglDbContext = _yglDbContextBuilder.Get();
        _dbContextFactory = Substitute.For<IDbContextFactory<YglDbContext>>();
        _dbContextFactory.CreateDbContext().Returns(_yglDbContext);
    }

    [Test]
    public async Task CheckHealth_Success_ReturnsHealthy()
    {
        //ARRANGE
        var healthCheck = new YourGamesListDatabaseHealthCheck(_logger, _dbContextFactory);

        //ACT
        var res = await healthCheck.CheckHealth(CancellationToken.None);

        //ASSERT
        Assert.That(res.Status, Is.EqualTo(HealthCheckStatusDto.Healthy));
        _logger.ReceivedLog(LogLevel.Information, $"{healthCheck.ServiceName} is healthy.");
    }

    [Test]
    public async Task CheckHealth_TokenExpires_ReturnsDegraded()
    {
        //ARRANGE
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        await cts.CancelAsync();

        var healthCheck = new YourGamesListDatabaseHealthCheck(_logger, _dbContextFactory);

        //ACT
        var res = await healthCheck.CheckHealth(token);

        //ASSERT
        Assert.That(res.Status, Is.EqualTo(HealthCheckStatusDto.Degraded));
        _logger.ReceivedLog(LogLevel.Warning, $"{healthCheck.ServiceName} is degraded. Reason: Operation cancelled while checking database connectivity.");
    }

    //TODO mock CanConnect return false and test that scenario

    // [Test]
    // public async Task CheckHealth_OnCanConnectReturnsFalse_ReturnsUnhealthy()
    // {
    //     //ARRANGE
    //     var healthCheck = new YourGamesListDatabaseHealthCheck(_logger, _dbContextFactory);
    //
    //     //ACT
    //     var res = await healthCheck.CheckHealth(CancellationToken.None);
    //
    //     //ASSERT
    //     Assert.That(res.Status, Is.EqualTo(HealthCheckStatusDto.Degraded));
    //    _logger.ReceivedLog(LogLevel.Warning, $"{healthCheck.ServiceName} is unhealthy. Reason: Cannot connect to the database.");
    // }
}