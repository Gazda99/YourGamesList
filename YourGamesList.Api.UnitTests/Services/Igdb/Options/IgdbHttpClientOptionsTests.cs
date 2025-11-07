using System.Linq;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.Igdb.Options;

namespace YourGamesList.Api.UnitTests.Services.Igdb.Options;

public class IgdbHttpClientOptionsTests
{
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void Validate_ValidOptions_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<IgdbHttpClientOptions>()
            .With(x => x.BaseAddress, "https://api.igdb.com")
            .WithAutoProperties()
            .Create();

        var validator = new IgdbHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }


    [Test]
    [TestCase("ebebe")]
    [TestCase(".com")]
    [TestCase("")]
    public void Validate_InvalidBaseAddress_ReturnsFalse(string invalidBaseAddress)
    {
        //ARRANGE
        var options = _fixture.Build<IgdbHttpClientOptions>()
            .With(x => x.BaseAddress, invalidBaseAddress)
            .WithAutoProperties()
            .Create();

        var validator = new IgdbHttpClientOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.BaseAddress);
    }
}