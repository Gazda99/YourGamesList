namespace YourGamesList.Contracts.Requests.Lists;

public class CreateListRequestBody
{
    public required string ListName { get; init; } = string.Empty;
    public string? Description { get; init; } = string.Empty;
}