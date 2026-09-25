using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth.Options;

public class VerificationEmailSenderOptionsTests
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
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.YglUrl, "http://127.0.0.1")
            .WithAutoProperties()
            .Create();

        var validator = new VerificationEmailSenderOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }
    
    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("htt:wrong/address")]
    public void Validate_InvalidYglUrl_ReturnsFalse(string url)
    {
        //ARRANGE
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.YglUrl, url)
            .WithAutoProperties()
            .Create();

        var validator = new VerificationEmailSenderOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.YglUrl);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidAddress_ReturnsFalse(string address)
    {
        //ARRANGE
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.Address, address)
            .WithAutoProperties()
            .Create();

        var validator = new VerificationEmailSenderOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidUsername_ReturnsFalse(string username)
    {
        //ARRANGE
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.Username, username)
            .WithAutoProperties()
            .Create();

        var validator = new VerificationEmailSenderOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidPassword_ReturnsFalse(string password)
    {
        //ARRANGE
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.Password, password)
            .WithAutoProperties()
            .Create();

        var validator = new VerificationEmailSenderOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_InvalidSmtpHost_ReturnsFalse(string host)
    {
        //ARRANGE
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.SmtpHost, host)
            .WithAutoProperties()
            .Create();

        var validator = new VerificationEmailSenderOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.SmtpHost);
    }
}