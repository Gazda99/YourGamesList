using System.Linq;
using AutoFixture;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth.Options;

public class PasswordValidatorOptionsTests
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
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MinimumPasswordLength, 10)
            .With(x => x.MaximumPasswordLength, 100)
            .WithAutoProperties()
            .Create();

        var validator = new PasswordValidatorOptionsValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

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
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(PasswordValidatorOptions.MinimumPasswordLength)));
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
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.PropertyName), Contains.Item(nameof(PasswordValidatorOptions.MaximumPasswordLength)));
    }

    [Test]
    public void Validate_MinimumBiggerThanMaximum_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MinimumPasswordLength, 100)
            .With(x => x.MaximumPasswordLength, 10)
            .WithAutoProperties()
            .Create();

        var validator = new PasswordValidatorOptionsValidator();

        //ACT
        var res = validator.Validate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Minimum password length must be less or  equal than maximum password length."));
    }
}