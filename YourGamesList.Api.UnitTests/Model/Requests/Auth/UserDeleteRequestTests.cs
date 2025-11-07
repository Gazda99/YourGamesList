using System.Linq;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model.Requests.Auth;

namespace YourGamesList.Api.UnitTests.Model.Requests.Auth;

public class UserDeleteRequestTests
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
        var options = _fixture.Build<UserDeleteRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidUsername_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserDeleteRequest>()
            .With(x => x.Username, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Validate_InvalidPassword_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserDeleteRequest>()
            .With(x => x.Password, string.Empty)
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Password);
    }
}