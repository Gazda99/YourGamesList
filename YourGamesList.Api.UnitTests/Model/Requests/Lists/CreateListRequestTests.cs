using System.Linq;
using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class CreateListRequestTests : BaseTest
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
        var request = _fixture.Build<CreateListRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new CreateListRequestValidator(_jwtUserInformationValidator);

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
        var request = _fixture.Build<CreateListRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new CreateListRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
    }

    [Test]
    public void Validate_InvalidListName_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<CreateListRequest>()
            .With(x => x.Body,
                _fixture.Build<CreateListRequestBody>()
                    .With(x => x.ListName, string.Empty)
                    .WithAutoProperties()
                    .Create())
            .WithAutoProperties()
            .Create();

        var validator = new CreateListRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrorFor(x => x.Body.ListName);
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("List name is required."));
    }
}