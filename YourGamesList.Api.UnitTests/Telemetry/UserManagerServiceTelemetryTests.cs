using System.Diagnostics.Metrics;
using AutoFixture;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using NSubstitute;
using YourGamesList.Api.Telemetry;

namespace YourGamesList.Api.UnitTests.Telemetry;

public class UserManagerServiceTelemetryTests
{
    private const string MeterName = "YourGamesList.Api.UserManagerService";
    private const string LoggingAttemptCounterName = "user_login_attempts";

    private IFixture _fixture;
    private IMeterFactory _meterFactory;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _meterFactory = Substitute.For<IMeterFactory>();
    }

    [Test]
    public void TrackSuccessfulLogin_TrackAll()
    {
        //ARRANGE
        using var meter = new Meter(MeterName);
        _meterFactory.Create(Arg.Any<MeterOptions>()).Returns(meter);
        var collector = new MetricCollector<long>(meter, LoggingAttemptCounterName);

        var telemetry = new UserManagerServiceTelemetry(_meterFactory);


        //ACT
        telemetry.TrackSuccessfulLogin();

        //ASSERT
        var measurements = collector.GetMeasurementSnapshot();
        Assert.That(measurements, Has.Count.EqualTo(1));

        var snapshot = measurements[0];
        Assert.That(snapshot.Value, Is.EqualTo(1));
        Assert.That(snapshot.Tags["success"], Is.True);

        _meterFactory.Received(1).Create(Arg.Is<MeterOptions>(x => x.Name == MeterName));
    }

    [Test]
    public void TrackFailedLogin_TrackAll()
    {
        //ARRANGE
        var expectedReason = _fixture.Create<string>();
        using var meter = new Meter(MeterName);
        _meterFactory.Create(Arg.Any<MeterOptions>()).Returns(meter);
        var collector = new MetricCollector<long>(meter, LoggingAttemptCounterName);

        var telemetry = new UserManagerServiceTelemetry(_meterFactory);


        //ACT
        telemetry.TrackFailedLogin(expectedReason);

        //ASSERT
        var measurements = collector.GetMeasurementSnapshot();
        Assert.That(measurements, Has.Count.EqualTo(1));

        var snapshot = measurements[0];
        Assert.That(snapshot.Value, Is.EqualTo(1));
        Assert.That(snapshot.Tags["success"], Is.False);
        Assert.That(snapshot.Tags["reason"], Is.EquivalentTo(expectedReason));

        _meterFactory.Received(1).Create(Arg.Is<MeterOptions>(x => x.Name == MeterName));
    }
}