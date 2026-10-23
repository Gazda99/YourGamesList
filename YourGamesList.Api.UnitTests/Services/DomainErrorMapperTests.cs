using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Api.Services.YourGamesList.Model;
using YourGamesList.Contracts.Responses;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services;

public class DomainErrorMapperTests
{
    private ILogger<DomainErrorMapper> _logger = null!;
    private DomainErrorMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<DomainErrorMapper>>();
        _mapper = new DomainErrorMapper(_logger);
    }

    [TestCase(UserAuthError.InternalServerError, ErrorCodes.General.InternalServerError, "An unexpected error occurred.")]
    [TestCase(UserAuthError.ForbiddenUser, ErrorCodes.UserAuth.ForbiddenUser, "You can only perform this action on your own account.")]
    [TestCase(UserAuthError.UserNotActiveEmailActivation, ErrorCodes.UserAuth.UserNotActiveEmailActivation, "Verify your email address before logging in.")]
    [TestCase(UserAuthError.InvalidEmail, ErrorCodes.UserAuth.InvalidEmail, "Invalid email address.")]
    [TestCase(UserAuthError.EmailAlreadyTaken, ErrorCodes.UserAuth.EmailAlreadyTaken, "Email address is already registered.")]
    [TestCase(UserAuthError.EmailVerificationInvalid, ErrorCodes.UserAuth.EmailVerificationInvalid, "Email verification link is invalid.")]
    [TestCase(UserAuthError.EmailVerificationExpired, ErrorCodes.UserAuth.EmailVerificationExpired, "Email verification link has expired.")]
    [TestCase(UserAuthError.UsernameAlreadyTaken, ErrorCodes.UserAuth.UsernameAlreadyTaken, "Username is already taken.")]
    public void Map_UserAuthError_ReturnsMappedError(UserAuthError authError, string expectedCode, string expectedDescription)
    {
        //ARRANGE

        //ACT
        var result = _mapper.Map(authError);

        //ASSERT
        Assert.That(result.Code, Is.EqualTo(expectedCode));
        Assert.That(result.Description, Is.EqualTo(expectedDescription));
    }

    [TestCase(UserAuthError.NoUserFound)]
    [TestCase(UserAuthError.UnauthorizedUser)]
    public void Map_UnmappedUserAuthError_LogsAndReturnsInternalServerError(UserAuthError authError)
    {
        //ARRANGE
        const string expectedCode = ErrorCodes.General.InternalServerError;
        const string expectedDescription = "An unexpected error occurred.";

        //ACT
        var result = _mapper.Map(authError);

        //ASSERT
        Assert.That(result.Code, Is.EqualTo(expectedCode));
        Assert.That(result.Description, Is.EqualTo(expectedDescription));
        _logger.ReceivedLog(
            LogLevel.Information,
            $"Domain level error {authError} was not mapped. Defaulting to {ErrorCodes.General.InternalServerError}");
    }

    [TestCase(ListsError.InternalServerError, ErrorCodes.General.InternalServerError, "An unexpected error occurred.")]
    [TestCase(ListsError.UserNotFound, ErrorCodes.Lists.UserNotFound, "User was not found.")]
    [TestCase(ListsError.ListAlreadyExists, ErrorCodes.Lists.ListAlreadyExists, "A list with this name already exists.")]
    [TestCase(ListsError.ListNotFound, ErrorCodes.Lists.ListNotFound, "List was not found.")]
    [TestCase(ListsError.ListCannotBeDeleted, ErrorCodes.Lists.ListCannotBeDeleted, "This list cannot be deleted.")]
    [TestCase(ListsError.ListCannotBeEdited, ErrorCodes.Lists.ListCannotBeEdited, "This list cannot be edited.")]
    [TestCase(ListsError.GameListEntryAlreadyExists, ErrorCodes.Lists.GameListEntryAlreadyExists, "This game is already in the list.")]
    [TestCase(ListsError.GameListEntryNotFound, ErrorCodes.Lists.GameListEntryNotFound, "Game list entry was not found.")]
    [TestCase(ListsError.OwnershipInfoNotFound, ErrorCodes.Lists.OwnershipInfoNotFound, "Ownership info was not found.")]
    [TestCase(ListsError.InvalidOwnershipInfoEmulation, ErrorCodes.Lists.InvalidOwnershipInfoEmulation, "EmulatedOn can only be set when WasEmulated is true.")]
    public void Map_ListsError_ReturnsMappedError(ListsError listsError, string expectedCode, string expectedDescription)
    {
        //ARRANGE

        //ACT
        var result = _mapper.Map(listsError);

        //ASSERT
        Assert.That(result.Code, Is.EqualTo(expectedCode));
        Assert.That(result.Description, Is.EqualTo(expectedDescription));
    }

    [Test]
    public void Map_UnmappedListsError_LogsAndReturnsInternalServerError()
    {
        //ARRANGE
        const ListsError listsError = (ListsError) 999;

        //ACT
        var result = _mapper.Map(listsError);

        //ASSERT
        Assert.That(result.Code, Is.EqualTo(ErrorCodes.General.InternalServerError));
        Assert.That(result.Description, Is.EqualTo("An unexpected error occurred."));
        _logger.ReceivedLog(
            LogLevel.Information,
            $"Domain level error {listsError} was not mapped. Defaulting to {ErrorCodes.General.InternalServerError}");
    }

    [Test]
    public void Map_UserNotFound_ReturnsMappedError()
    {
        //ARRANGE

        //ACT
        var result = _mapper.Map(UsersError.UserNotFound);

        //ASSERT
        Assert.That(result.Code, Is.EqualTo(ErrorCodes.Users.UserNotFound));
        Assert.That(result.Description, Is.EqualTo("User was not found."));
    }
}