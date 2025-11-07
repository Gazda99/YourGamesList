using System.Linq;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth.Options;

public class TokenAuthOptionsTests
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
        var options = _fixture.Build<TokenAuthOptions>()
            .WithAutoProperties()
            .Create();

        var validator = new TokenAuthOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidAudience_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<TokenAuthOptions>()
            .With(x => x.Audience, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new TokenAuthOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Audience);
    }

    [Test]
    public void Validate_InvalidIssuer_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<TokenAuthOptions>()
            .With(x => x.Issuer, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new TokenAuthOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Issuer);
    }

    [Test]
    public void Validate_InvalidJwtSecret_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<TokenAuthOptions>()
            .With(x => x.JwtSecret, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new TokenAuthOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.JwtSecret);
    }

    [Test]
    public void Validate_InvalidExpirationInMinutes_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<TokenAuthOptions>()
            .With(x => x.ExpirationInMinutes, -100)
            .WithAutoProperties()
            .Create();

        var validator = new TokenAuthOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.ExpirationInMinutes);
    }
}