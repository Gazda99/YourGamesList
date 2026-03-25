using System;
using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class DeleteListEntriesRequestTests : BaseTest
{
    private InlineValidator<UserInformationToken> _jwtUserInformationValidator;

    [SetUp]
    public void Setup()
    {
        _jwtUserInformationValidator = new InlineValidator<UserInformationToken>();
    }


    [Test]
    public void Validate_ValidOptions_ReturnsTrue()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<DeleteListEntriesRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new DeleteListEntriesRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_WrongJwtUserInformation_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x.UserId).Must(_ => false);
        var request = _fixture.Build<DeleteListEntriesRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new DeleteListEntriesRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
    }

    [Test]
    public void Validate_InvalidListId_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<DeleteListEntriesRequest>()
            .With(x => x.Body,
                _fixture.Build<DeleteListEntriesRequestBody>()
                    .With(x => x.ListId, Guid.Empty)
                    .WithAutoProperties()
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new DeleteListEntriesRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
    }
}