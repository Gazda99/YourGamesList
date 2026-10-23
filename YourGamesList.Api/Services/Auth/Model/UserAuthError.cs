namespace YourGamesList.Api.Services.Auth.Model;

public enum UserAuthError
{
    InternalServerError,
    NoUserFound,
    UnauthorizedUser,
    ForbiddenUser,
    UserNotActiveEmailActivation,

    #region Email

    InvalidEmail,
    EmailAlreadyTaken,

    EmailVerificationInvalid,
    EmailVerificationExpired,

    #endregion

    #region UserName

    UsernameAlreadyTaken,

    #endregion
}