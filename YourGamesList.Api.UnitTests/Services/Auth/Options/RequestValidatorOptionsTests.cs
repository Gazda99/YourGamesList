using System.Linq;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth.Options;

public class RequestValidatorOptionsTests
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
        var options = _fixture.Build<RequestValidatorOptions>()
            .With(x => x.Email, _fixture.Build<EmailValidatorOptions>()
                .With(x => x.MaximumEmailLength, 100)
                .Create())
            .With(x => x.Lists, _fixture.Build<ListsValidatorOptions>()
                .With(x => x.MaximumListNameLength, 100)
                .Create())
            .With(x => x.Username, _fixture.Build<UsernameValidatorOptions>()
                .With(x => x.MinimumUsernameLength, 1)
                .With(x => x.MaximumUsernameLength, 100)
                .Create())
            .With(x => x.Password, _fixture.Build<PasswordValidatorOptions>()
                .With(x => x.MinimumPasswordLength, 1)
                .With(x => x.MaximumPasswordLength, 100)
                .Create())
            .With(x => x.DateOfBirth, _fixture.Build<DateOfBirthValidatorOptions>()
                .With(x => x.MinimumAge, 18)
                .Create())
            .Create();

        var validator = new UserValuesValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    #region Email

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMaximumEmailLength_ReturnsFalse(int maximumEmailLength)
    {
        //ARRANGE
        var options = _fixture.Build<EmailValidatorOptions>()
            .With(x => x.MaximumEmailLength, maximumEmailLength)
            .WithAutoProperties()
            .Create();

        var validator = new EmailValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MaximumEmailLength);
    }

    #endregion

    #region Lists

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMaximumListNameLength_ReturnsFalse(int maximumListNameLength)
    {
        //ARRANGE
        var options = _fixture.Build<ListsValidatorOptions>()
            .With(x => x.MaximumListNameLength, maximumListNameLength)
            .Create();

        var validator = new ListsValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MaximumListNameLength);
    }

    #endregion


    #region Username

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMinimumUsernameLength_ReturnsFalse(int minimumUsernameLength)
    {
        //ARRANGE
        var options = _fixture.Build<UsernameValidatorOptions>()
            .With(x => x.MinimumUsernameLength, minimumUsernameLength)
            .WithAutoProperties()
            .Create();

        var validator = new UsernameValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MinimumUsernameLength);
    }


    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMaximumUsernameLength_ReturnsFalse(int maximumUsernameLength)
    {
        //ARRANGE
        var options = _fixture.Build<UsernameValidatorOptions>()
            .With(x => x.MaximumUsernameLength, maximumUsernameLength)
            .WithAutoProperties()
            .Create();

        var validator = new UsernameValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MaximumUsernameLength);
    }

    [Test]
    public void Validate_UsernameMinimumBiggerThanMaximum_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UsernameValidatorOptions>()
            .With(x => x.MinimumUsernameLength, 100)
            .With(x => x.MaximumUsernameLength, 10)
            .WithAutoProperties()
            .Create();

        var validator = new UsernameValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Minimum username length must be less or equal than maximum username length."));
    }

    #endregion


    #region Password

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMinimumPasswordLength_ReturnsFalse(int minimumPasswordLength)
    {
        //ARRANGE
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MinimumPasswordLength, minimumPasswordLength)
            .WithAutoProperties()
            .Create();

        var validator = new PasswordValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MinimumPasswordLength);
    }


    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMaximumPasswordLength_ReturnsFalse(int maximumPasswordLength)
    {
        //ARRANGE
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MaximumPasswordLength, maximumPasswordLength)
            .WithAutoProperties()
            .Create();

        var validator = new PasswordValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MaximumPasswordLength);
    }

    [Test]
    public void Validate_PasswordMinimumBiggerThanMaximum_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MinimumPasswordLength, 100)
            .With(x => x.MaximumPasswordLength, 10)
            .WithAutoProperties()
            .Create();

        var validator = new PasswordValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Minimum password length must be less or equal than maximum password length."));
    }

    #endregion

    #region DateOfBirth

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMinimumAge_ReturnsFalse(int minimumAge)
    {
        //ARRANGE
        var options = _fixture.Build<DateOfBirthValidatorOptions>()
            .With(x => x.MinimumAge, minimumAge)
            .WithAutoProperties()
            .Create();

        var validator = new DateOfBirthValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MinimumAge);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-10)]
    public void Validate_InvalidMaximum_ReturnsFalse(int maximumAge)
    {
        //ARRANGE
        var options = _fixture.Build<DateOfBirthValidatorOptions>()
            .With(x => x.MaximumAge, maximumAge)
            .WithAutoProperties()
            .Create();

        var validator = new DateOfBirthValidatorOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MaximumAge);
    }

    #endregion
}