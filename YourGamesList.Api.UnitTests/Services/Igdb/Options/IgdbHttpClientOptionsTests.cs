using AutoFixture;
using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Http.Resilience;
using YourGamesList.Common.Http;
using YourGamesList.Api.Services.Igdb.Options;

namespace YourGamesList.Api.UnitTests.Services.Igdb.Options;

public class IgdbHttpClientOptionsTests
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
        var options = _fixture.Build<IgdbHttpClientOptions>()
            .With(x => x.BaseAddress, "https://api.igdb.com")
            .With(x => x.Resilience, new HttpResilienceOptions())
            .WithAutoProperties()
            .Create();

        var validator = new IgdbHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

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
        var options = _fixture.Build<IgdbHttpClientOptions>()
            .With(x => x.BaseAddress, invalidBaseAddress)
            .With(x => x.Resilience, new HttpResilienceOptions())
            .WithAutoProperties()
            .Create();

        var validator = new IgdbHttpClientOptionsValidator();

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
        var options = new IgdbHttpClientOptions
        {
            BaseAddress = "https://api.igdb.com",
            Resilience = new HttpResilienceOptions
            {
                TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.Zero }
            }
        };

        var validator = new IgdbHttpClientOptionsValidator();

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
        var options = new IgdbHttpClientOptions
        {
            BaseAddress = "https://api.igdb.com",
            Resilience = null
        };

        var validator = new IgdbHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        res.ShouldNotHaveAnyValidationErrors();
    }
}