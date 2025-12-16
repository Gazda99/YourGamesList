using System;
using System.Linq;
using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class UpdateListEntriesRequestTests
{
    private IFixture _fixture;
    private InlineValidator<JwtUserInformation> _jwtUserInformationValidator;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _jwtUserInformationValidator = new InlineValidator<JwtUserInformation>();
    }

    [Test]
    public void Validate_ValidOptions_ReturnsTrue()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<UpdateListEntriesRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new UpdateListEntriesRequestValidator(_jwtUserInformationValidator);

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
        var request = _fixture.Build<UpdateListEntriesRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new UpdateListEntriesRequestValidator(_jwtUserInformationValidator);

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
        var request = _fixture.Build<UpdateListEntriesRequest>()
            .With(x => x.Body,
                _fixture.Build<UpdateListEntriesRequestBody>()
                    .With(x => x.ListId, Guid.Empty)
                    .WithAutoProperties()
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UpdateListEntriesRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("List Id is required."));
    }

    [Test]
    public void Validate_InvalidGameIdInEntryToAddRequestPart_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<UpdateListEntriesRequest>()
            .With(x => x.Body,
                _fixture.Build<UpdateListEntriesRequestBody>()
                    .With(x => x.EntriesToUpdate,
                        [
                            //Add one valid and one invalid
                            _fixture.Build<EntryToUpdateRequestPart>()
                                .With(x => x.EntryId, Guid.NewGuid())
                                .WithAutoProperties()
                                .Create(),
                            _fixture.Build<EntryToUpdateRequestPart>()
                                .With(x => x.EntryId, Guid.Empty)
                                .WithAutoProperties()
                                .Create()
                        ]
                    )
                    .WithAutoProperties()
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UpdateListEntriesRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Entry Id is required."));
    }
}