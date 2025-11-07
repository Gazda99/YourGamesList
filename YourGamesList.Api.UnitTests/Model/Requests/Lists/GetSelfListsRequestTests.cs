using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class GetSelfListsRequestTests
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
        var options = _fixture.Build<GetSelfListsRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new GetSelfListsRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_WrongJwtUserInformation_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x.UserId).Must(_ => false);
        var options = _fixture.Build<GetSelfListsRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new GetSelfListsRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
    }
}