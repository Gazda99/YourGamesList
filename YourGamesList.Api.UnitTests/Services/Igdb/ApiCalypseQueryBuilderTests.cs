using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using YourGamesList.Api.Services.Igdb;

namespace YourGamesList.Api.UnitTests.Services.Igdb;

public class ApiCalypseQueryBuilderTests
{
    private IFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void Build_ReturnsNewApiCalypseQueryBuilderInstance()
    {
        //ACT
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ASSERT
        Assert.That(apiCalypseQueryBuilder, Is.Not.Null);
        Assert.That(apiCalypseQueryBuilder, Is.TypeOf<ApiCalypseQueryBuilder>());
    }

    [Test]
    public void WithFields_SingleString_AddsFieldsParameter()
    {
        //ARRANGE
        var fields = _fixture.Create<string>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithFields(fields);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("fields"));
        Assert.That(res, Contains.Substring(fields));
    }

    [Test]
    public void WithFields_CollectionOfStrings_AddsFieldsParameter()
    {
        //ARRANGE
        var fields = _fixture.Create<List<string>>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithFields(fields);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("fields"));
        Assert.That(res, Contains.Substring(string.Join(",", fields.Select(x => x.Trim()))));
    }

    [Test]
    public void WithExcludedFields_SingleString_AddsExcludeParameter()
    {
        //ARRANGE
        var excluded = _fixture.Create<string>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithExcludedFields(excluded);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("exclude"));
        Assert.That(res, Contains.Substring(excluded));
    }

    [Test]
    public void WithExcludedFields_CollectionOfStrings_AddsExcludeParameter()
    {
        //ARRANGE
        var excluded = _fixture.Create<List<string>>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithExcludedFields(excluded);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("exclude"));
        Assert.That(res, Contains.Substring(string.Join(",", excluded.Select(x => x.Trim()))));
    }

    [Test]
    public void WithWhere_AddsWhereParameter()
    {
        //ARRANGE
        var where = _fixture.Create<string>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithWhere(where);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("where"));
        Assert.That(res, Contains.Substring(where));
    }

    [Test]
    public void WithLimit_AddsLimitParameter()
    {
        //ARRANGE
        var limit = _fixture.Create<int>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithLimit(limit);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("limit"));
        Assert.That(res, Contains.Substring(limit.ToString()));
    }

    [Test]
    public void WithOffset_AddsLimitParameter()
    {
        //ARRANGE
        var offset = _fixture.Create<int>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithOffset(offset);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("offset"));
        Assert.That(res, Contains.Substring(offset.ToString()));
    }

    [Test]
    public void WithSort_AddsSortParameter()
    {
        //ARRANGE
        var sort = _fixture.Create<string>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithSort(sort);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("sort"));
        Assert.That(res, Contains.Substring(sort));
    }

    [Test]
    public void WithCustomQuery_AddsQueryParameter()
    {
        //ARRANGE
        var query = _fixture.Create<string>();
        var apiCalypseQueryBuilder = ApiCalypseQueryBuilder.Build();

        //ACT
        apiCalypseQueryBuilder.WithCustomQuery(query);
        var res = apiCalypseQueryBuilder.CreateQuery();

        //ASSERT
        Assert.That(res, Contains.Substring("query"));
        Assert.That(res, Contains.Substring(query));
    }
}