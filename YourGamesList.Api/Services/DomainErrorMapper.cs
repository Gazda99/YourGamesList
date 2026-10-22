using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Api.Services.YourGamesList.Model;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Services;

public interface IDomainErrorMapper
{
    /// <summary>
    /// Maps an authentication error to an API error.
    /// </summary>
    Error Map(UserAuthError userAuthError);

    /// <summary>
    /// Maps a list error to an API error.
    /// </summary>
    Error Map(ListsError listsError);

    /// <summary>
    /// Maps a user error to an API error.
    /// </summary>
    Error Map(UsersError usersError);
}

public class DomainErrorMapper : IDomainErrorMapper
{
    private readonly ILogger<DomainErrorMapper> _logger;

    public DomainErrorMapper(ILogger<DomainErrorMapper> logger)
    {
        _logger = logger;
    }

    public Error Map(UserAuthError userAuthError)
    {
        switch (userAuthError)
        {
            case UserAuthError.InternalServerError:
                return CreateInternalServerError();

            case UserAuthError.UserNotActiveEmailActivation:
                return Create(ErrorCodes.UserAuth.UserNotActiveEmailActivation, "Verify your email address before logging in.");

            case UserAuthError.InvalidEmail:
                return Create(ErrorCodes.UserAuth.InvalidEmail, "Invalid email address.");

            case UserAuthError.EmailAlreadyTaken:
                return Create(ErrorCodes.UserAuth.EmailAlreadyTaken, "Email address is already registered.");

            case UserAuthError.EmailVerificationInvalid:
                return Create(ErrorCodes.UserAuth.EmailVerificationInvalid, "Email verification link is invalid.");

            case UserAuthError.EmailVerificationExpired:
                return Create(ErrorCodes.UserAuth.EmailVerificationExpired, "Email verification link has expired.");

            case UserAuthError.UsernameAlreadyTaken:
                return Create(ErrorCodes.UserAuth.UsernameAlreadyTaken, "Username is already taken.");

            default:
                _logger.LogInformation("Domain level error {DomainError} was not mapped. Defaulting to {ErrorCode}", userAuthError.ToString(),
                    ErrorCodes.General.InternalServerError);
                return CreateInternalServerError();
        }
    }

    public Error Map(ListsError listsError)
    {
        switch (listsError)
        {
            case ListsError.InternalServerError:
                return CreateInternalServerError();
            case ListsError.UserNotFound:
                return Create(ErrorCodes.Lists.UserNotFound, "User was not found.");
            case ListsError.ListAlreadyExists:
                return Create(ErrorCodes.Lists.ListAlreadyExists, "A list with this name already exists.");
            case ListsError.ListNotFound:
                return Create(ErrorCodes.Lists.ListNotFound, "List was not found.");
            case ListsError.ListCannotBeDeleted:
                return Create(ErrorCodes.Lists.ListCannotBeDeleted, "This list cannot be deleted.");
            case ListsError.ListCannotBeEdited:
                return Create(ErrorCodes.Lists.ListCannotBeEdited, "This list cannot be edited.");
            case ListsError.GameListEntryAlreadyExists:
                return Create(ErrorCodes.Lists.GameListEntryAlreadyExists, "This game is already in the list.");
            case ListsError.GameListEntryNotFound:
                return Create(ErrorCodes.Lists.GameListEntryNotFound, "Game list entry was not found.");
            default:
                _logger.LogInformation("Domain level error {DomainError} was not mapped. Defaulting to {ErrorCode}", listsError.ToString(),
                    ErrorCodes.General.InternalServerError);
                return CreateInternalServerError();
        }
    }

    public Error Map(UsersError usersError)
    {
        switch (usersError)
        {
            case UsersError.UserNotFound:
                return Create(ErrorCodes.Users.UserNotFound, "User was not found.");
            default:
                _logger.LogInformation("Domain level error {DomainError} was not mapped. Defaulting to {ErrorCode}", usersError.ToString(),
                    ErrorCodes.General.InternalServerError);
                return CreateInternalServerError();
        }
    }

    private static Error Create(string code, string description)
    {
        return new Error()
        {
            Code = code,
            Description = description
        };
    }

    private static Error CreateInternalServerError()
    {
        return Create(ErrorCodes.General.InternalServerError, "An unexpected error occurred.");
    }
}