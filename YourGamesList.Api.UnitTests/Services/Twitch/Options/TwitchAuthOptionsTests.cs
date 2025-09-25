using System.Linq;
using AutoFixture;
using YourGamesList.Api.Services.Twitch.Options;

namespace YourGamesList.Api.UnitTests.Services.Twitch.Options;

public class TwitchAuthOptionsTests
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
        var options = _fixture.Build<TwitchAuthOptions>()
            .WithAutoProperties()
            .Create();

        var validator = new TwitchAuthOptionsValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidClientId_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<TwitchAuthOptions>()
            .With(x => x.ClientId, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new TwitchAuthOptionsValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(TwitchAuthOptions.ClientId)));
    }

    [Test]
    public void Validate_InvalidClientSecret_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<TwitchAuthOptions>()
            .With(x => x.ClientSecret, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new TwitchAuthOptionsValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(TwitchAuthOptions.ClientSecret)));
    }
}