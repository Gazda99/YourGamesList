using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Common.Refit;
using YourGamesList.TestsUtils;

namespace YourGamesList.Common.UnitTests.Refit;

[TestFixture]
public class RefitExtensionsTests
{
    [Test]
    [TestCaseSource(nameof(ExceptionTestCases))]
    public async Task TryRefit_OnException_ReturnsFailureResult(Exception ex, HttpFailureReason expectedFailureReason)
    {
        //ARRANGE
        var logger = Substitute.For<ILogger>();
        var refitApi = Substitute.For<IHandlesHttpRefitException>();
        Func<Task<object>> refitCall = async () => throw ex;

        //ACT
        var res = await refitApi.TryRefit(refitCall, logger);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(expectedFailureReason));

        logger.ReceivedLog(LogLevel.Warning, "Http related exception occured while making http request with refit");
    }

    public static readonly TestCaseData[] ExceptionTestCases =
    [
        new TestCaseData(new TaskCanceledException(), HttpFailureReason.Timeout),
        new TestCaseData(new HttpRequestException(), HttpFailureReason.Network),
        new TestCaseData(new Exception(), HttpFailureReason.General)
    ];
}