using System.Linq;
using AutoFixture;
using YourGamesList.Api.Model.Requests.SearchGames;

namespace YourGamesList.Api.UnitTests.Model.Requests.SearchGames;

public class SearchGamesRequestTests
{
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void Validate_ValidOptions_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<SearchGamesRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new SearchGamesRequestValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    [TestCase("")]
    [TestCase("  ")]
    public void Validate_InvalidGameName_ReturnsFalse(string invalidGameName)
    {
        //ARRANGE
        var options = _fixture.Build<SearchGamesRequest>()
            .With(x => x.GameName, invalidGameName)
            .WithAutoProperties()
            .Create();

        var validator = new SearchGamesRequestValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(SearchGamesRequest.GameName)));
    }
}