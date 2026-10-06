namespace YourGamesList.Contracts.Dto;

public class GameCompanyDto
{
    public required string CompanyName { get; set; }
    public required bool Developer { get; set; }
    public required bool Publisher { get; set; }
}