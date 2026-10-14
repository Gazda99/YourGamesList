using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Auth.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserAuthError
{
    InternalServerError,
    NoUserFound,
    UnauthorizedUser,
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