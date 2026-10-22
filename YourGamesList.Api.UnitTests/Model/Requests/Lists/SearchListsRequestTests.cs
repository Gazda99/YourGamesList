using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class SearchListsRequestTests
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
                MinimumListNameLength = 3
            },
            Username = new UsernameValidatorOptions
            {
                MinimumUsernameLength = 4
            }
        });
    }

    [Test]
    public void Validate_WithOwnerAndListName_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(ownerName: "owner", listName: "games");

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithEitherSearchTerm_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(listName: "games");

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_ListNameShorterThanMinimumLength_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(ownerName: "owner", listName: " ab ");

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.ListName)
            .WithErrorCode(ErrorCodes.Lists.InvalidListSearchCriteria);
    }

    [Test]
    public void Validate_ListNameAtMinimumLengthAfterTrimming_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(listName: " abc ");

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_OwnerOnlySearch_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(ownerName: "owner");

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_OwnerNameShorterThanConfiguredMinimum_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(ownerName: " abc ", listName: "games");

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.OwnerName)
            .WithErrorCode(ErrorCodes.Lists.InvalidListSearchCriteria);
    }

    [Test]
    public void Validate_OwnerNameAtConfiguredMinimumAfterTrimming_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(ownerName: " abcd ");

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithoutSearchTerms_ReturnsValidationError()
    {
        //ARRANGE
        var request = CreateRequest(ownerName: " ", listName: null);

        //ACT
        var result = CreateValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode(ErrorCodes.Lists.InvalidListSearchCriteria);
    }

    private static SearchListsRequest CreateRequest(string? ownerName = null, string? listName = null) => new()
    {
        UserInformation = new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = Guid.NewGuid()
        },
        OwnerName = ownerName,
        ListName = listName
    };

    private SearchListsRequestValidator CreateValidator() => new(_options);
}
