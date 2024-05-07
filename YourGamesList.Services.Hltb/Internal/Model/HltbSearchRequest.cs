namespace YourGamesList.Services.Hltb.Internal.Model;

internal class HltbSearchRequest
{
    public string SearchTypes { get; set; } = "games";
    public string[] SearchTerms { get; set; } = Array.Empty<string>();
    public int SearchPage { get; set; } = 1;
    public int Size { get; set; } = 20;
}