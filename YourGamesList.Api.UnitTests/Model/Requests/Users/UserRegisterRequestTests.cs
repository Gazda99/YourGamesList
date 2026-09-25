using System;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.UnitTests.Model.Requests.Users;

public class UserRegisterRequestTests
{
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory(() => DateOnly.FromDateTime(_fixture.Create<DateTime>())));
    }

    [Test]
    public void Validate_ValidOptions_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<UserRegisterRequest>()
            .With(x => x.Body,
                _fixture.Build<AuthUserRegisterRequestBody>()
                    .With(x => x.Email, "some@email.com")
                    .WithAutoProperties()
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UserRegisterRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidUsername_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserRegisterRequest>()
            .With(x => x.Body, _fixture.Build<AuthUserRegisterRequestBody>()
                .With(x => x.Username, string.Empty)
                .WithAutoProperties()
                .Create()
            )
            .WithAutoProperties()
            .Create();
        var validator = new UserRegisterRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Body.Username);
    }


    [Test]
    [TestCase("")]
    [TestCase("not_email")]
    public void Validate_InvalidEmail_ReturnsFalse(string invalidEmail)
    {
        //ARRANGE
        var options = _fixture.Build<UserRegisterRequest>()
            .With(x => x.Body, _fixture.Build<AuthUserRegisterRequestBody>()
                .With(x => x.Email, invalidEmail)
                .WithAutoProperties()
                .Create()
            )
            .WithAutoProperties()
            .Create();
        var validator = new UserRegisterRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Body.Email);
    }

    [Test]
    public void Validate_InvalidPassword_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserRegisterRequest>()
            .With(x => x.Body, _fixture.Build<AuthUserRegisterRequestBody>()
                .With(x => x.Password, string.Empty)
                .WithAutoProperties()
                .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UserRegisterRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Body.Password);
    }
}