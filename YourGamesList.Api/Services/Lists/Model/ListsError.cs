using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Lists.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ListsError
{
    InternalServerError,
    UserNotFound,
    ListAlreadyExists,
    ListNotFound,
    ListCannotBeDeleted
}