using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class AddGameListEntryRequestTests
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
    public void Validate_ValidBody_IsValid()
    {
        //ARRANGE
        var request = CreateRequest();

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_UnknownCompletionStatus_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest((CompletionStatusDto)999);

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
        var request = CreateRequest(description: new string('a', 11));

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Description)
            .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryDescription);
    }

    [Test]
    public void Validate_NonPositiveIgdbId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(igdbId: 0);

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.IgdbId)
            .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryIgdbId);
    }

    [TestCase((byte)0)]
    [TestCase((byte)5)]
    public void Validate_RatingWithinRange_IsValid(byte rating)
    {
        //ARRANGE
        var request = CreateRequest(rating: rating);

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
        var request = CreateRequest(rating: rating);

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Rating)
            .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryRating);
    }

    private static AddGameListEntryRequest CreateRequest(
        CompletionStatusDto completionStatus = CompletionStatusDto.Played,
        long igdbId = 123,
        string? description = null,
        byte? rating = null) => new()
    {
        UserInformation = new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = Guid.NewGuid()
        },
        ListId = Guid.NewGuid(),
        Body = new AddGameListEntryRequestBody
        {
            CompletionStatus = completionStatus,
            IgdbId = igdbId,
            Description = description,
            Rating = rating
        }
    };

    private AddGameListEntryRequestValidator CreateValidator() => new(_options);
}
