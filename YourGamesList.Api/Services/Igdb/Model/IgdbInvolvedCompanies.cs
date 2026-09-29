using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbInvolvedCompanies
{
    [JsonPropertyName("company")] public IgdbCompany Company { get; init; } = new IgdbCompany();
    [JsonPropertyName("developer")] public bool Developer { get; init; }
    [JsonPropertyName("publisher")] public bool Publisher { get; init; }
}