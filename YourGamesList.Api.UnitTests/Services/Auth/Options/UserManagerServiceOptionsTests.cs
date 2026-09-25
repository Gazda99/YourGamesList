using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth.Options;

public class UserManagerServiceOptionsTests
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
        var options = _fixture.Build<UserManagerServiceOptions>()
            .Create();

        var validator = new UserManagerServiceOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidDefaultEmailVerificationExpiryInMinutes_ReturnsFalse(int expiryMinutes)
    {
        //ARRANGE
        var options = _fixture.Build<UserManagerServiceOptions>()
            .With(x => x.DefaultEmailVerificationExpiryInMinutes, expiryMinutes)
            .WithAutoProperties()
            .Create();

        var validator = new UserManagerServiceOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.DefaultEmailVerificationExpiryInMinutes);
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidDefaultPendingUserExpiryInMinutes_ReturnsFalse(int expiryMinutes)
    {
        //ARRANGE
        var options = _fixture.Build<UserManagerServiceOptions>()
            .With(x => x.DefaultPendingUserExpiryInMinutes, expiryMinutes)
            .WithAutoProperties()
            .Create();

        var validator = new UserManagerServiceOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.DefaultPendingUserExpiryInMinutes);
    }
}