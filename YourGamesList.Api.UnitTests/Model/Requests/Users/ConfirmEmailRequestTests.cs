using FluentValidation.TestHelper;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Users;

public class ConfirmEmailRequestTests
{
    [Test]
    public void Validate_ValidToken_ReturnsValid()
    {
        //ARRANGE
        var validator = new ConfirmEmailRequestValidator();
        var request = new ConfirmEmailRequest { Body = new AuthConfirmEmailRequestBody("some_token") };

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_BodyMissing_ReturnsValidationError()
    {
        //ARRANGE
        var validator = new ConfirmEmailRequestValidator();
        var request = new ConfirmEmailRequest { Body = null! };

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Validate_EmptyToken_ReturnsValidationError(string? token)
    {
        //ARRANGE
        var validator = new ConfirmEmailRequestValidator();
        var request = new ConfirmEmailRequest { Body = new AuthConfirmEmailRequestBody(token!) };

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor("Body.Token")
            .WithErrorCode(ErrorCodes.UserAuth.EmailVerificationInvalid);
    }
}
