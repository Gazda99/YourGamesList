using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Common.Telemetry.Options;

namespace YourGamesList.Common.UnitTests.Telemetry.Options;

public class TelemetryConfigurationOptionsTests
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
        var options = _fixture.Build<TelemetryConfigurationOptions>()
            .WithAutoProperties()
            .Create();

        var validator = new TelemetryConfigurationOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        res.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [TestCase((string?) null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidApiSecret_ReturnsFalse(string apiSecret)
    {
        //ARRANGE
        var options = _fixture.Build<TelemetryConfigurationOptions>()
            .With(x => x.ApiSecret, apiSecret)
            .WithAutoProperties()
            .Create();

        var validator = new TelemetryConfigurationOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.ApiSecret);
    }


    [Test]
    [TestCase((string?) null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidOtlpExporterTracesEndpoint_ReturnsFalse(string endpoint)
    {
        //ARRANGE
        var options = _fixture.Build<TelemetryConfigurationOptions>()
            .With(x => x.OtlpExporterTracesEndpoint, endpoint)
            .WithAutoProperties()
            .Create();

        var validator = new TelemetryConfigurationOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.OtlpExporterTracesEndpoint);
    }

    [Test]
    [TestCase((string?) null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidOtlpExporterMetricsEndpoint_ReturnsFalse(string endpoint)
    {
        //ARRANGE
        var options = _fixture.Build<TelemetryConfigurationOptions>()
            .With(x => x.OtlpExporterMetricsEndpoint, endpoint)
            .WithAutoProperties()
            .Create();

        var validator = new TelemetryConfigurationOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.OtlpExporterMetricsEndpoint);
    }


    [Test]
    [TestCase((string?) null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidServiceName_ReturnsFalse(string serviceName)
    {
        //ARRANGE
        var options = _fixture.Build<TelemetryConfigurationOptions>()
            .With(x => x.ServiceName, serviceName)
            .WithAutoProperties()
            .Create();

        var validator = new TelemetryConfigurationOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.ServiceName);
    }


    [Test]
    [TestCase((string?) null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidTracesStreamName_ReturnsFalse(string tracesStreamName)
    {
        //ARRANGE
        var options = _fixture.Build<TelemetryConfigurationOptions>()
            .With(x => x.TracesStreamName, tracesStreamName)
            .WithAutoProperties()
            .Create();

        var validator = new TelemetryConfigurationOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.TracesStreamName);
    }
}