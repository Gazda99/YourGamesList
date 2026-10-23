using System;
using System.Text.Json;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class UpdateGameListEntryRequestTests
{
    private IOptions<RequestValidatorOptions> _options = null!;

    [SetUp]
    public void Setup()
    {
        _options = Substitute.For<IOptions<RequestValidatorOptions>>();
        _options.Value.Returns(new RequestValidatorOptions
        {
            Lists = new ListsValidatorOptions
            {
                MaximumGameListEntryDescriptionLength = 10,
                MinimumGameListEntryRating = 0,
                MaximumGameListEntryRating = 5
            }
        });
    }

    [Test]
    public void Validate_DescriptionCanBeCleared()
    {
        //ARRANGE
        var request = new UpdateGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = new UpdateGameListEntryRequestBody
            {
                Description = Optional<string?>.Some(null),
                Rating = Optional<byte?>.Some(null)
            }
        };

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_UnknownCompletionStatus_IsInvalid()
    {
        //ARRANGE
        var request = new UpdateGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = new UpdateGameListEntryRequestBody
            {
                CompletionStatus = Optional<CompletionStatusDto?>.Some((CompletionStatusDto)999)
            }
        };

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.CompletionStatus)
            .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryCompletionStatus);
    }

    [Test]
    public void Validate_DescriptionExceedsConfiguredLimit_IsInvalid()
    {
        //ARRANGE
        var request = new UpdateGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = new UpdateGameListEntryRequestBody
            {
                Description = Optional<string?>.Some(new string('a', 11))
            }
        };

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Description)
            .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryDescription);
    }

    [Test]
    public void Validate_NullCompletionStatus_IsInvalid()
    {
        //ARRANGE
        var request = new UpdateGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = new UpdateGameListEntryRequestBody
            {
                CompletionStatus = Optional<CompletionStatusDto?>.Some(null)
            }
        };

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.CompletionStatus)
            .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryCompletionStatus);
    }

    [Test]
    public void DeserializeRequest_DistinguishesOmittedAndNullDescription()
    {
        //ARRANGE
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        //ACT
        var omitted = JsonSerializer.Deserialize<UpdateGameListEntryRequestBody>(
            """{}""",
            options);
        var explicitNull = JsonSerializer.Deserialize<UpdateGameListEntryRequestBody>(
            """{"description":null}""",
            options);

        //ASSERT
        Assert.That(omitted?.Description.HasValue, Is.False);
        Assert.That(explicitNull?.Description.HasValue, Is.True);
        Assert.That(explicitNull?.Description.Value, Is.Null);
    }

    [TestCase((byte)0)]
    [TestCase((byte)5)]
    public void Validate_RatingWithinRange_IsValid(byte rating)
    {
        //ARRANGE
        var request = new UpdateGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = new UpdateGameListEntryRequestBody
            {
                Rating = Optional<byte?>.Some(rating)
            }
        };

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase((byte)6)]
    [TestCase((byte)255)]
    public void Validate_RatingAboveMaximum_IsInvalid(byte rating)
    {
        //ARRANGE
        var request = new UpdateGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = new UpdateGameListEntryRequestBody
            {
                Rating = Optional<byte?>.Some(rating)
            }
        };

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Rating)
            .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryRating);
    }

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = Guid.NewGuid()
    };

    private UpdateGameListEntryRequestValidator CreateValidator() => new(_options);
}
