using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Auth.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserAuthError
{
    InternalServerError,
    NoUserFound,
    UnauthorizedUser,

    #region Email

    InvalidEmail,
    EmailIsTooLong,
    EmailAlreadyTaken,

    #endregion

    #region UserName

    InvalidUsername,
    UsernameIsTooShort,
    UsernameIsTooLong,
    UsernameAlreadyTaken,

    #endregion

    #region Password

    WrongPassword,
    PasswordIsTooShort,
    PasswordIsTooLong,

    #endregion

    #region DateOfBirth

    InvalidDateOfBirth,
    UnderMinimumAge,
    ExceedsMaximumAge

    #endregion
}