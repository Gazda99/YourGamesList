using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model.Requests.Auth;
using YourGamesList.Contracts.Requests.Users;

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
            .With(x => x.Body, _fixture.Build<AuthUserDeleteRequestBody>()
                .With(x => x.Username, string.Empty)
                .WithAutoProperties()
                .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Body.Username);
    }

    [Test]
    public void Validate_InvalidPassword_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserDeleteRequest>()
            .With(x => x.Body, _fixture.Build<AuthUserDeleteRequestBody>()
                .With(x => x.Password, string.Empty)
                .WithAutoProperties()
                .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Body.Password);
    }
}