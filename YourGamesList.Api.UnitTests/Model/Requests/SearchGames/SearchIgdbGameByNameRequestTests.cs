using System.Linq;
using AutoFixture;
using YourGamesList.Api.Model.Requests.SearchIgdbGames;

namespace YourGamesList.Api.UnitTests.Model.Requests.SearchGames;

public class SearchIgdbGameByNameRequestTests
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
        var options = _fixture.Build<SearchIgdbGameByNameRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new SearchGameByNameRequestValidator();

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
        var options = _fixture.Build<SearchIgdbGameByNameRequest>()
            .With(x => x.GameName, invalidGameName)
            .WithAutoProperties()
            .Create();

        var validator = new SearchGameByNameRequestValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(SearchIgdbGameByNameRequest.GameName)));
    }
}