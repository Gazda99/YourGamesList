namespace YourGamesList.Api.Services.Lists.Model;

public enum ListsError
{
    InternalServerError,
    UserNotFound,
    ListAlreadyExists,
    ListNameTooLong,
    ListNotFound,
    ListCannotBeDeleted
}