using System.Security.Cryptography;
using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth.Options;

public class LibSodiumPasswordHashingOptionsTests
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
        var options = _fixture.Build<LibSodiumPasswordHashingOptions>()
            .With(x => x.OpsLimit, (ulong) 2)
            .With(x => x.MemoryLimit, (nuint) (32 * 1024 * 1024))
            .WithAutoProperties()
            .Create();

        var validator = new LibSodiumPasswordHashingOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_InvalidOpsLimit_ReturnsFalse()
    {
        //ARRANGE
        var invalidOpsLimit = RandomNumberGenerator.GetInt32(1);
        var options = _fixture.Build<LibSodiumPasswordHashingOptions>()
            .With(x => x.OpsLimit, (ulong) invalidOpsLimit)
            .WithAutoProperties()
            .Create();

        var validator = new LibSodiumPasswordHashingOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.OpsLimit);
    }

    [Test]
    public void Validate_InvalidMemoryLimit_ReturnsFalse()
    {
        //ARRANGE
        var invalidMemoryLimit = RandomNumberGenerator.GetInt32(8192);
        var options = _fixture.Build<LibSodiumPasswordHashingOptions>()
            .With(x => x.MemoryLimit, (nuint) invalidMemoryLimit)
            .WithAutoProperties()
            .Create();

        var validator = new LibSodiumPasswordHashingOptionsValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.MemoryLimit);
    }
}