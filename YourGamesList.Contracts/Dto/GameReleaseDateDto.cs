namespace YourGamesList.Contracts.Dto;

public class GameReleaseDateDto
{
    public required long Date { get; init; }
    public required string PlatformName { get; init; }
}