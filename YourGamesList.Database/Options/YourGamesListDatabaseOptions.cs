using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace YourGamesList.Database.Options;

[ExcludeFromCodeCoverage]
public class YourGamesListDatabaseOptions
{
    public const string SectionName = "YourGamesListDatabase";
    public required string ConnectionString { get; init; }
    public string? MigrationAssembly { get; init; }
}

[ExcludeFromCodeCoverage]
public class YourGamesListDatabaseOptionsValidator : AbstractValidator<YourGamesListDatabaseOptions>
{
    public YourGamesListDatabaseOptionsValidator()
    {
        RuleFor(x => x.ConnectionString).NotEmpty();
    }
}