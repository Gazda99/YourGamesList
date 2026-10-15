using FluentValidation.TestHelper;
using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.UnitTests.Model.Requests.Users;

public class ConfirmEmailRequestTests
{
    [Test]
    public void Validate_EmptyToken_ReturnsValid()
    {
        //ARRANGE
        var validator = new ConfirmEmailRequestValidator();
        var request = new ConfirmEmailRequest { Token = string.Empty };

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }
}
