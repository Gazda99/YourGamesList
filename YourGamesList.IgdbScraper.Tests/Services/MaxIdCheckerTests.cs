using System;
using System.Threading.Tasks;
using FluentAssertions;
using Igdb.Model.Custom;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TddXt.AnyRoot.Numbers;
using TddXt.AnyRoot.Strings;
using YourGamesList.IgdbScraper.Services.IgdbClient;
using YourGamesList.IgdbScraper.Services.IgdbClient.Exceptions;
using YourGamesList.IgdbScraper.Services.MaxIdChecker;
using YourGamesList.IgdbScraper.Tests.TestHelpers;
using static TddXt.AnyRoot.Root;

namespace YourGamesList.IgdbScraper.Tests.Services;

public class MaxIdCheckerTests
{
    private const string MaxIdQueryBody = "fields id; sort id desc; limit 1;";

    #region MaxId

    [Test]
    public async Task GetMaxId_Should_ReturnMaxId()
    {
        var endpoint = EndpointsHelper.RandomEndpoint();
        TestContext.WriteLine($"Testing for endpoint \'{endpoint}\'");
        //GIVEN
        var igdbClient = Substitute.For<IIgdbClient>();
        var expectedId = (long) Any.UnsignedLong();
        var maxIdResponse = new IdAndName[]
        {
            new IdAndName()
            {
                Name = Any.String(),
                Id = expectedId
            }
        };
        igdbClient.FetchData<IdAndName>(MaxIdQueryBody, endpoint).Returns(maxIdResponse);
        var logger = Substitute.For<ILogger<MaxIdChecker>>();

        var maxIdChecker = new MaxIdChecker(logger, igdbClient);

        //WHEN
        var maxId = await maxIdChecker.GetMaxId(endpoint);

        //THEN
        maxId.Should().Be(expectedId);
        logger.Received(1).LogInformation($"Trying to find max id using endpoint: {endpoint}");
        logger.Received(1).LogInformation($"For endpoint {endpoint}, found max id: {expectedId}");
    }

    [Test]
    public async Task GetMaxId_Should_CatchIgdbClientFetchDataException()
    {
        var endpoint = EndpointsHelper.RandomEndpoint();
        TestContext.WriteLine($"Testing for endpoint \'{endpoint}\'");
        //GIVEN
        var igdbClient = Substitute.For<IIgdbClient>();
        var ex = Any.Instance<IgdbClientException>();
        igdbClient.FetchData<IdAndName>(MaxIdQueryBody, endpoint).Throws(ex);
        var logger = Substitute.For<ILogger<MaxIdChecker>>();

        var maxIdChecker = new MaxIdChecker(logger, igdbClient);

        //WHEN
        var maxId = await maxIdChecker.GetMaxId(endpoint);

        //THEN
        maxId.Should().Be(-1);
        logger.Received(1).LogError(ex, $"{CouldNotObtainMaxIdLog(endpoint)} Reason: {ex.Reason}.");
    }

    [Test]
    public async Task GetMaxId_Should_CatchProblemResponseEmpty()
    {
        var endpoint = EndpointsHelper.RandomEndpoint();
        TestContext.WriteLine($"Testing for endpoint \'{endpoint}\'");
        //GIVEN
        var igdbClient = Substitute.For<IIgdbClient>();
        var maxIdResponse = Array.Empty<IdAndName>();
        igdbClient.FetchData<IdAndName>(MaxIdQueryBody, endpoint).Returns(maxIdResponse);
        var logger = Substitute.For<ILogger<MaxIdChecker>>();

        var maxIdChecker = new MaxIdChecker(logger, igdbClient);

        //WHEN
        var maxId = await maxIdChecker.GetMaxId(endpoint);

        //THEN
        maxId.Should().Be(-1);
        logger.Received(1).LogError($"{CouldNotObtainMaxIdLog(endpoint)} Reason: list is empty.");
    }

    [Test]
    public async Task GetMaxId_Should_CatchProblemEntryIdIsNull()
    {
        var endpoint = EndpointsHelper.RandomEndpoint();
        TestContext.WriteLine($"Testing for endpoint \'{endpoint}\'");
        //GIVEN
        var igdbClient = Substitute.For<IIgdbClient>();
        var maxIdResponse = new IdAndName[]
        {
            new IdAndName()
            {
                Id = null,
                Name = Any.String()
            }
        };
        igdbClient.FetchData<IdAndName>(MaxIdQueryBody, endpoint).Returns(maxIdResponse);
        var logger = Substitute.For<ILogger<MaxIdChecker>>();

        var maxIdChecker = new MaxIdChecker(logger, igdbClient);

        //WHEN
        var maxId = await maxIdChecker.GetMaxId(endpoint);

        //THEN
        maxId.Should().Be(-1);
        logger.Received(1).LogError($"{CouldNotObtainMaxIdLog(endpoint)} Reason: first item.Id is null.");
    }

    private static string CouldNotObtainMaxIdLog(string endpoint)
    {
        return $"Could not obtain max id using endpoint: {endpoint}.";
    }

    #endregion

    #region GetCount

    [Test]
    public async Task GetCount_Should_ReturnCount()
    {
        var endpoint = EndpointsHelper.RandomEndpoint();
        TestContext.WriteLine($"Testing for endpoint \'{endpoint}\'");
        //GIVEN
        var igdbClient = Substitute.For<IIgdbClient>();
        var expectedCount = (long) Any.UnsignedLong();
        var countResponse = new MultiQueryCount[]
        {
            new MultiQueryCount()
            {
                Name = Any.String(),
                Count = expectedCount
            }
        };
        igdbClient.FetchData<MultiQueryCount>(CountQueryBody(endpoint), EndpointsHelper.MultiQuery)
            .Returns(countResponse);
        var logger = Substitute.For<ILogger<MaxIdChecker>>();

        var maxIdChecker = new MaxIdChecker(logger, igdbClient);

        //WHEN
        var count = await maxIdChecker.GetCount(endpoint);

        //THEN
        count.Should().Be(expectedCount);
        logger.Received(1).LogInformation($"Trying to find count using endpoint: {endpoint}");
        logger.Received(1).LogInformation($"For endpoint {endpoint}, found count: {expectedCount}");
    }

    [Test]
    public async Task GetCount_Should_CatchIgdbClientFetchDataException()
    {
        var endpoint = EndpointsHelper.RandomEndpoint();
        TestContext.WriteLine($"Testing for endpoint \'{endpoint}\'");
        //GIVEN
        var igdbClient = Substitute.For<IIgdbClient>();
        var ex = Any.Instance<IgdbClientException>();
        igdbClient.FetchData<MultiQueryCount>(CountQueryBody(endpoint), EndpointsHelper.MultiQuery)
            .Throws(ex);
        var logger = Substitute.For<ILogger<MaxIdChecker>>();

        var maxIdChecker = new MaxIdChecker(logger, igdbClient);

        //WHEN
        var count = await maxIdChecker.GetCount(endpoint);

        //THEN
        count.Should().Be(-1);
        logger.Received(1).LogError(ex, $"{CouldNotObtainCountLog(endpoint)} Reason: {ex.Reason}.");
    }

    [Test]
    public async Task GetCount_Should_CatchProblemResponseEmpty()
    {
        var endpoint = EndpointsHelper.RandomEndpoint();
        TestContext.WriteLine($"Testing for endpoint \'{endpoint}\'");
        //GIVEN
        var igdbClient = Substitute.For<IIgdbClient>();
        var countResponse = Array.Empty<MultiQueryCount>();
        igdbClient.FetchData<MultiQueryCount>(CountQueryBody(endpoint), EndpointsHelper.MultiQuery)
            .Returns(countResponse);
        var logger = Substitute.For<ILogger<MaxIdChecker>>();

        var maxIdChecker = new MaxIdChecker(logger, igdbClient);

        //WHEN
        var count = await maxIdChecker.GetCount(endpoint);

        //THEN
        count.Should().Be(-1);
        logger.Received(1).LogError($"{CouldNotObtainCountLog(endpoint)} Reason: list is empty.");
    }

    private static string CountQueryBody(string endpoint)
    {
        return $"query {endpoint}/count \"Count of {endpoint}\" {{}};";
    }


    private static string CouldNotObtainCountLog(string endpoint)
    {
        return $"Could not obtain count using endpoint: {endpoint}.";
    }

    #endregion
}