using System.Linq;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model.Requests.SearchYglGames;
using YourGamesList.Contracts.Requests.Games;

namespace YourGamesList.Api.UnitTests.Model.Requests.SearchYglGames;

public class SearchYglGamesRequestTests
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
        var options = _fixture.Build<SearchYglGamesRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new SearchYglGamesRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidGameName_ReturnsFalse()
    {
        //ARRANGE
        var request = _fixture.Build<SearchYglGamesRequest>()
            .With(x => x.Body,
                _fixture.Build<SearchYglGamesRequestBody>()
                    .With(x => x.GameName, string.Empty)
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new SearchYglGamesRequestValidator();

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Game name must be provided."));
    }

    [Test]
    public void Validate_InvalidTake_ReturnsFalse()
    {
        //ARRANGE
        var request = _fixture.Build<SearchYglGamesRequest>()
            .With(x => x.Body,
                _fixture.Build<SearchYglGamesRequestBody>()
                    .With(x => x.Take, 0)
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new SearchYglGamesRequestValidator();

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Take must be greater than 0."));
    }


    [Test]
    public void Validate_InvalidSkip_ReturnsFalse()
    {
        //ARRANGE
        var request = _fixture.Build<SearchYglGamesRequest>()
            .With(x => x.Body,
                _fixture.Build<SearchYglGamesRequestBody>()
                    .With(x => x.Skip, -1)
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new SearchYglGamesRequestValidator();

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Skip must be greater or equal to 0."));
    }
}