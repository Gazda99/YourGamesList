using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Controllers;

public class TestControllerTests
{
    private IFixture _fixture;
    private ILogger<TestController> _logger;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<TestController>>();
    }

    [Test]
    public async Task Test()
    {
        //ARRANGE
        var controller = new TestController(_logger);

        //ACT
        await controller.Get();

        //ASSERT
        _logger.ReceivedLog(LogLevel.Information, "Hello from TestController");
    }
}