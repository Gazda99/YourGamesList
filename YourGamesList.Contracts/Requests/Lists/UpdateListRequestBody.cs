using System;
using YourGamesList.Contracts;

namespace YourGamesList.Contracts.Requests.Lists;

public record UpdateListRequestBody
{
    public Guid Id { get; init; }
    public Optional<string?> Name { get; init; }
    public Optional<string?> Description { get; init; }
    public Optional<bool?> IsPrivate { get; init; }
}
