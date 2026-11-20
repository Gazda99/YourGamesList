using AutoFixture;
using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Http.Resilience;
using YourGamesList.Common.Http;
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
            .With(x => x.Resilience, new HttpResilienceOptions())
            .WithAutoProperties()
            .Create();

        var validator = new TwitchAuthHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        res.ShouldNotHaveAnyValidationErrors();
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
            .With(x => x.Resilience, new HttpResilienceOptions())
            .WithAutoProperties()
            .Create();

        var validator = new TwitchAuthHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.BaseAddress);
    }

    [Test]
    public void Validate_InvalidResilienceTimeout_ReturnsFalse()
    {
        //ARRANGE
        var options = new TwitchAuthHttpClientOptions
        {
            BaseAddress = "https://api.twitch.tv",
            Resilience = new HttpResilienceOptions
            {
                TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.Zero }
            }
        };

        var validator = new TwitchAuthHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrorFor("Resilience.TotalRequestTimeout.Timeout");
    }

    [Test]
    public void Validate_ResilienceNotConfigured_ReturnsTrue()
    {
        //ARRANGE
        var options = new TwitchAuthHttpClientOptions
        {
            BaseAddress = "https://api.twitch.tv",
            Resilience = null
        };

        var validator = new TwitchAuthHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        res.ShouldNotHaveAnyValidationErrors();
    }
}