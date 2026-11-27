namespace YourGamesList.Contracts.Requests.Users;

public sealed record AuthUserChangePasswordRequestBody(
    string CurrentPassword,
    string NewPassword
);
