namespace YourGamesList.Contracts.Requests.Lists;

public record UpdateListRequestBody
{
    public Optional<string?> Name { get; init; }
    public Optional<string?> Description { get; init; }
    public Optional<bool?> IsPrivate { get; init; }
}