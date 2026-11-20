using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Http.Resilience;
using YourGamesList.Common.Http;

namespace YourGamesList.Common.UnitTests.Http;

public class HttpResilienceOptionsValidatorTests
{
    [Test]
    public void Validate_DefaultOptions_ReturnsValid()
    {
        //ARRANGE
        var validator = new HttpResilienceOptionsValidator();

        //ACT
        var result = validator.TestValidate(new HttpResilienceOptions());

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void DefaultTimeoutStrategyOptions_TimeoutIs30Seconds()
    {
        //ACT - used by the resilience pipeline when TotalRequestTimeout is not configured
        var options = new HttpTimeoutStrategyOptions();

        //ASSERT
        Assert.That(options.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
    }

    [TestCase("00:00:00.010")]
    [TestCase("00:00:20")]
    [TestCase("1.00:00:00")]
    public void Validate_TimeoutWithinRange_ReturnsValid(string timeout)
    {
        //ARRANGE
        var validator = new HttpResilienceOptionsValidator();
        var options = CreateOptions(TimeSpan.Parse(timeout));

        //ACT
        var result = validator.TestValidate(options);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("00:00:00")]
    [TestCase("00:00:00.009")]
    [TestCase("1.00:00:01")]
    public void Validate_TimeoutOutOfRange_ReturnsInvalid(string timeout)
    {
        //ARRANGE
        var validator = new HttpResilienceOptionsValidator();
        var options = CreateOptions(TimeSpan.Parse(timeout));

        //ACT
        var result = validator.TestValidate(options);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor("TotalRequestTimeout.Timeout");
    }

    [Test]
    public void Validate_TotalRequestTimeoutNotConfigured_ReturnsValid()
    {
        //ARRANGE
        var validator = new HttpResilienceOptionsValidator();
        var options = new HttpResilienceOptions { TotalRequestTimeout = null };

        //ACT
        var result = validator.TestValidate(options);

        //ASSERT
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static HttpResilienceOptions CreateOptions(TimeSpan timeout) => new()
    {
        TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = timeout }
    };
}
