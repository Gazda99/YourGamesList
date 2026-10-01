using System;
using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class UserRegisterParams
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required DateOnly DateOfBirth { get; set; }
}

public static class UserRegisterParamsMappingExtensions
{
    public static UserRegisterParams ToParams(this UserRegisterRequest request)
    {
        return new UserRegisterParams
        {
            Email = request.Body.Email,
            Username = request.Body.Username,
            Password = request.Body.Password,
            DateOfBirth = request.Body.DateOfBirth
        };
    }
}