using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class AddListRequestTests
{
    private ILogger<CreateListRequestValidator> _logger;
    private IOptions<RequestValidatorOptions> _options;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<CreateListRequestValidator>>();
        _options = Substitute.For<IOptions<RequestValidatorOptions>>();
        _options.Value.Returns(new RequestValidatorOptions
        {
            Lists = new ListsValidatorOptions
            {
                MinimumListNameLength = 3,
                MaximumListNameLength = 20
            }
        });
    }

    [Test]
    public void Validate_ValidRequest_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest("My games");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NameWithWhitespace_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest("  My games  ");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyName_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(string.Empty);
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name)
            .WithErrorCode(ErrorCodes.Lists.InvalidListName);
    }

    [Test]
    public void Validate_NameShorterThanMinimumLengthAfterTrimming_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(" ab ");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name)
            .WithErrorCode(ErrorCodes.Lists.InvalidListName);
    }

    [Test]
    public void Validate_NameAtMinimumLengthAfterTrimming_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest(" abc ");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NameExceedsMaximumLength_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest("This list name is too long");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name)
            .WithErrorCode(ErrorCodes.Lists.ListNameTooLong)
            .WithErrorMessage("List name must be no longer than 20 characters.");
    }

    [Test]
    public void Validate_NameAtMaximumLength_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest(new string('a', 20));
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_BodyMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest("My games");
        request = new AddListRequest
        {
            UserInformation = request.UserInformation,
            Body = null!
        };
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new AddListRequest
        {
            UserInformation = null!,
            Body = new CreateListRequestBody("My games")
        };
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }

    private CreateListRequestValidator CreateValidator()
    {
        return new CreateListRequestValidator(_logger, _options);
    }

    private static AddListRequest CreateRequest(string name)
    {
        return new AddListRequest()
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            },
            Body = new CreateListRequestBody(name)
        };
    }
}