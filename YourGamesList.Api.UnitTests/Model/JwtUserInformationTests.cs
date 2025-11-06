using System;
using System.Linq;
using AutoFixture;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.UnitTests.Model;

public class JwtUserInformationTests
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
        var options = _fixture.Build<JwtUserInformation>()
            .WithAutoProperties()
            .Create();

        var validator = new JwtUserInformationValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }


    [Test]
    public void Validate_InvalidUsername_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<JwtUserInformation>()
            .With(x => x.Username, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new JwtUserInformationValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(JwtUserInformation.Username)));
    }

    [Test]
    public void Validate_InvalidUserId_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<JwtUserInformation>()
            .With(x => x.UserId, Guid.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new JwtUserInformationValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(JwtUserInformation.UserId)));
    }
}