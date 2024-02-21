namespace YourGamesList.Common.Services.Hltb.Requests;

public class HltbSearchRequest
{
    public string SearchTypes { get; set; } = "games";
    public string[] SearchTerms { get; set; } = Array.Empty<string>();
    public int SearchPage { get; set; } = 1;
    public int Size { get; set; } = 20;
}