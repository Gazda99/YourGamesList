using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Web.Page.Services.UserLoginStateManager.Options;

namespace YourGamesList.Web.Page.UnitTests.Services.UserLoginState.Options;

public class UserLoginStateManagerOptionsTests
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
        var options = _fixture.Create<UserLoginStateManagerOptions>();

        var validator = new UserLoginStateManagerOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidTokenTtlInMinutes_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserLoginStateManagerOptions>()
            .With(x => x.TokenTtlInMinutes, -100)
            .WithAutoProperties()
            .Create();

        var validator = new UserLoginStateManagerOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.TokenTtlInMinutes);
    }
}