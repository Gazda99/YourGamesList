using System.Linq;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.Scraper.Options;

namespace YourGamesList.Api.UnitTests.Services.Scraper.Options;

public class ScraperOptionsTests
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
        var options = _fixture.Build<ScraperOptions>()
            .WithAutoProperties()
            .Create();

        var validator = new ScraperOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidBatchSize_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<ScraperOptions>()
            .With(x => x.BatchSize, -100)
            .WithAutoProperties()
            .Create();

        var validator = new ScraperOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.BatchSize);
    }

    [Test]
    public void Validate_InvalidConcurrencyLevel_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<ScraperOptions>()
            .With(x => x.ConcurrencyLevel, -100)
            .WithAutoProperties()
            .Create();

        var validator = new ScraperOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.ConcurrencyLevel);
    }

    [Test]
    public void Validate_InvalidRateLimitTimeFrameInMilliseconds_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<ScraperOptions>()
            .With(x => x.RateLimitTimeFrameInMilliseconds, -100)
            .WithAutoProperties()
            .Create();

        var validator = new ScraperOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.RateLimitTimeFrameInMilliseconds);
    }

    [Test]
    public void Validate_InvalidMaxConcurrentCallsWithinTimeFrame_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<ScraperOptions>()
            .With(x => x.MaxConcurrentCallsWithinTimeFrame, -100)
            .WithAutoProperties()
            .Create();

        var validator = new ScraperOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MaxConcurrentCallsWithinTimeFrame);
    }
}