using System.Linq;
using AutoFixture;
using YourGamesList.Api.Services.Twitch.Options;

namespace YourGamesList.Api.UnitTests.Services.Twitch.Options;

public class TwitchAuthHttpClientOptionsTests
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
        var options = _fixture.Build<TwitchAuthHttpClientOptions>()
            .With(x => x.BaseAddress, "https://api.twitch.tv")
            .WithAutoProperties()
            .Create();

        var validator = new TwitchAuthHttpClientOptionsValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }


    [Test]
    [TestCase("ebebe")]
    [TestCase(".com")]
    [TestCase("")]
    public void Validate_InvalidBaseAddress_ReturnsFalse(string invalidBaseAddress)
    {
        //ARRANGE
        var options = _fixture.Build<TwitchAuthHttpClientOptions>()
            .With(x => x.BaseAddress, invalidBaseAddress)
            .WithAutoProperties()
            .Create();

        var validator = new TwitchAuthHttpClientOptionsValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(TwitchAuthHttpClientOptions.BaseAddress)));
    }
}