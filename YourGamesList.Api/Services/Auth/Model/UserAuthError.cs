using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Auth.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserAuthError
{
    NoUserFound,

    #region Email

    InvalidEmail,
    EmailIsTooLong,

    #endregion

    #region UserName

    InvalidUsername,
    UsernameIsTooShort,
    UsernameIsTooLong,

    #endregion

    #region Register

    RegisterNameAlreadyTaken,

    #endregion

    #region Password

    WrongPassword,
    PasswordIsTooShort,
    PasswordIsTooLong,

    #endregion
}