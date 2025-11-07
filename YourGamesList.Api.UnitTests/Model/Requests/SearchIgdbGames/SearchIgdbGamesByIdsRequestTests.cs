using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model.Requests.SearchIgdbGames;

namespace YourGamesList.Api.UnitTests.Model.Requests.SearchIgdbGames;

public class SearchIgdbGamesByIdsRequestTests
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
        var options = _fixture.Build<SearchIgdbGamesByIdsRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new SearchGamesByIdsRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    [TestCase(new[] { 1, -2 })]
    [TestCase(new int[0])]
    public void Validate_InvalidGameIds_ReturnsFalse(int[] invalidGameIds)
    {
        //ARRANGE
        var options = _fixture.Build<SearchIgdbGamesByIdsRequest>()
            .With(x => x.GameIds, invalidGameIds)
            .WithAutoProperties()
            .Create();

        var validator = new SearchGamesByIdsRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.GameIds);
    }
}