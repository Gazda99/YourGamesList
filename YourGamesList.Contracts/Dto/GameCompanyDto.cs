namespace YourGamesList.Contracts.Dto;

public class GameCompanyDto
{
    public required string CompanyName { get; init; }
    public required bool Developer { get; init; }
    public required bool Publisher { get; init; }
}