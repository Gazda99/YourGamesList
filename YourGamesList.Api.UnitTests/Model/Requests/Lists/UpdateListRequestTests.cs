using System;
using System.Text.Json;
using AutoFixture;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class UpdateListRequestTests
{
    private IFixture _fixture;
    private ILogger<UpdateListRequestValidator> _logger = null!;
    private IOptions<RequestValidatorOptions> _options = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UpdateListRequestValidator>>();
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
    public void Validate_IdOnlyRequest_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody());

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NullDescription_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody
        {
            Description = Optional<string?>.Some(null)
        });

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NameExceedsMaximumLength_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody
        {
            Name = Optional<string?>.Some("This list name is too long")
        });

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name)
            .WithErrorCode(ErrorCodes.Lists.ListNameTooLong);
        var normalizedName = "This list name is too long";
        _logger.ReceivedLog(
            LogLevel.Information,
            $"List name {normalizedName} is too long. {normalizedName.Length} > 20");
    }

    [Test]
    public void Validate_WhitespaceName_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody
        {
            Name = Optional<string?>.Some("   ")
        });

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name)
            .WithErrorCode(ErrorCodes.Lists.InvalidListName);
        _logger.ReceivedLog(LogLevel.Information, "List name  is too short. 0 < 3");
    }

    [Test]
    public void Validate_NameShorterThanMinimumLengthAfterTrimming_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody
        {
            Name = Optional<string?>.Some(" ab ")
        });

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name)
            .WithErrorCode(ErrorCodes.Lists.InvalidListName);
        _logger.ReceivedLog(LogLevel.Information, "List name ab is too short. 2 < 3");
    }

    [Test]
    public void Validate_NameAtMinimumLengthAfterTrimming_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody
        {
            Name = Optional<string?>.Some(" abc ")
        });

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NullName_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody
        {
            Name = Optional<string?>.Some(null)
        });

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Count.EqualTo(1));
        result.ShouldHaveValidationErrorFor(x => x.Body.Name)
            .WithErrorCode(ErrorCodes.Lists.InvalidListName);
    }

    [Test]
    public void Validate_NullPrivacy_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateListRequestBody
        {
            IsPrivate = Optional<bool?>.Some(null)
        });

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.IsPrivate)
            .WithErrorCode(ErrorCodes.Lists.InvalidListPrivacy);
    }

    [Test]
    public void ToParams_TracksOmittedAndExplicitNullDescription()
    {
        //ARRANGE
        var omittedDescriptionRequest = CreateRequest(new UpdateListRequestBody());
        var nullDescriptionRequest = CreateRequest(new UpdateListRequestBody
        {
            Description = Optional<string?>.Some(null)
        });

        //ACT
        var omittedDescription = omittedDescriptionRequest.ToParams();
        var nullDescription = nullDescriptionRequest.ToParams();

        //ASSERT
        Assert.That(omittedDescription.HasDescription, Is.False);
        Assert.That(nullDescription.HasDescription, Is.True);
        Assert.That(nullDescription.Description, Is.Null);
    }

    [Test]
    public void Deserialize_TracksOmittedAndExplicitNullDescription()
    {
        //ARRANGE
        var omittedDescriptionJson = """{}""";
        var nullDescriptionJson = """{"description":null}""";
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        //ACT
        var omittedDescription = JsonSerializer.Deserialize<UpdateListRequestBody>(omittedDescriptionJson, options);
        var nullDescription = JsonSerializer.Deserialize<UpdateListRequestBody>(nullDescriptionJson, options);

        //ASSERT
        Assert.That(omittedDescription, Is.Not.Null);
        Assert.That(omittedDescription?.Description.HasValue, Is.False);
        Assert.That(nullDescription, Is.Not.Null);
        Assert.That(nullDescription?.Description.HasValue, Is.True);
        Assert.That(nullDescription?.Description.Value, Is.Null);
    }

    private UpdateListRequestValidator CreateValidator() => new(_logger, _options);

    private static UpdateListRequest CreateRequest(UpdateListRequestBody body) => new()
    {
        UserInformation = new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = Guid.NewGuid()
        },
        ListId = Guid.NewGuid(),
        Body = body
    };

}
