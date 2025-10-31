using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Scraper.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScraperError
{
    General,
    ScrapeAlreadyInProgress
}