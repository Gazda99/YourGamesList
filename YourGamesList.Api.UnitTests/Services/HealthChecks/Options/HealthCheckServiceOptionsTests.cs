using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.HealthChecks.Options;

namespace YourGamesList.Api.UnitTests.Services.HealthChecks.Options;

public class HealthCheckServiceOptionsTests
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
        var options = _fixture.Build<HealthCheckServiceOptions>()
            .With(x => x.CacheDurationInSeconds, 100)
            .With(x => x.TimeoutInSeconds, 10)
            .WithAutoProperties()
            .Create();

        var validator = new HealthCheckServiceOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidTimeoutInSeconds_ReturnsFalse(int timeout)
    {
        //ARRANGE
        var options = _fixture.Build<HealthCheckServiceOptions>()
            .With(x => x.CacheDurationInSeconds, 1000)
            .With(x => x.TimeoutInSeconds, timeout)
            .WithAutoProperties()
            .Create();

        var validator = new HealthCheckServiceOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.TimeoutInSeconds);
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_CacheDurationInSeconds_GreaterThanZeroCheck_ReturnsFalse(int cacheDuration)
    {
        //ARRANGE
        var options = _fixture.Build<HealthCheckServiceOptions>()
            .With(x => x.CacheDurationInSeconds, cacheDuration)
            .With(x => x.TimeoutInSeconds, 1000)
            .WithAutoProperties()
            .Create();

        var validator = new HealthCheckServiceOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.CacheDurationInSeconds);
    }
    
    [Test]
    public void Validate_CacheDurationInSeconds_GreaterThanTimeout_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<HealthCheckServiceOptions>()
            .With(x => x.CacheDurationInSeconds, 10)
            .With(x => x.TimeoutInSeconds, 100)
            .WithAutoProperties()
            .Create();

        var validator = new HealthCheckServiceOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.CacheDurationInSeconds);
    }
}