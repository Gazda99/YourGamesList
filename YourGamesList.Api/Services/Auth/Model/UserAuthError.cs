using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Auth.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserAuthError
{
    InvalidUsername,
    InvalidEmail,
    WrongPassword,

    NoUserFound,

    #region Register

    RegisterNameAlreadyTaken,

    #endregion

    #region Password

    PasswordIsTooShort,
    PasswordIsTooLong,

    #endregion
}