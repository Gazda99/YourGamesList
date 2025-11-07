using System.Linq;
using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class SearchListsRequestTests
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
        var request = _fixture.Build<SearchListsRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new SearchListsRequestValidator(_jwtUserInformationValidator);

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
        var request = _fixture.Build<SearchListsRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new SearchListsRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
    }

    [Test]
    public void Validate_ListNameAndUserNameBothEmpty_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<SearchListsRequest>()
            .With(x => x.Body,
                _fixture.Build<SearchListsRequestBody>()
                    .With(x => x.UserName, string.Empty)
                    .With(x => x.ListName, string.Empty)
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new SearchListsRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("You must provide user name or list name."));
    }

    [Test]
    public void Validate_ListNameShorterThan3_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<SearchListsRequest>()
            .With(x => x.Body,
                _fixture.Build<SearchListsRequestBody>()
                    .With(x => x.UserName, string.Empty)
                    .With(x => x.ListName, new string(_fixture.CreateMany<char>(2).ToArray()))
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new SearchListsRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("List name must be at least 3 characters long."));
    }

    [Test]
    public void Validate_InvalidTake_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<SearchListsRequest>()
            .With(x => x.Body,
                _fixture.Build<SearchListsRequestBody>()
                    .With(x => x.Take, 0)
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new SearchListsRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Take must be greater than 0."));
    }


    [Test]
    public void Validate_InvalidSkip_ReturnsFalse()
    {
        //ARRANGE
        _jwtUserInformationValidator.RuleFor(x => x).Must(_ => true);
        var request = _fixture.Build<SearchListsRequest>()
            .With(x => x.Body,
                _fixture.Build<SearchListsRequestBody>()
                    .With(x => x.Skip, -1)
                    .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new SearchListsRequestValidator(_jwtUserInformationValidator);

        //ACT
        var res = validator.TestValidate(request);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        res.ShouldHaveValidationErrors();
        Assert.That(res.Errors.Select(x => x.ErrorMessage), Contains.Item("Skip must be greater or equal to 0."));
    }
}