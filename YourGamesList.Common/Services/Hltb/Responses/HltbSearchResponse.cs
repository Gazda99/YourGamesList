using Newtonsoft.Json.Serialization;

namespace YourGamesList.Common.Services.Hltb.Responses;

public class HltbSearchResponse
{
    public string Color { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public int PageCurrent { get; set; }
    public int PageTotal { get; set; }
    public int PageSize { get; set; }
    public HltbData[] Data { get; set; } = Array.Empty<HltbData>();
    public HltbData[] UserData { get; set; } = Array.Empty<HltbData>();
    public string? DisplayModifier { get; set; }
}

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class HltbData
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int GameNameDate { get; set; }
    public string GameAlias { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public string GameImage { get; set; } = string.Empty;
    public int CompLvlCombine { get; set; }
    public int CompLvlSp { get; set; }
    public int CompLvlCo { get; set; }
    public int CompLvlMp { get; set; }
    public int CompLvlSpd { get; set; }
    public int CompMain { get; set; }
    public int CompPlus { get; set; }
    public int Comp100 { get; set; }
    public int CompAll { get; set; }
    public int CompMainCount { get; set; }
    public int CompPlusCount { get; set; }
    public int Comp100Count { get; set; }
    public int CompAllCount { get; set; }
    public int InvestedCo { get; set; }
    public int InvestedMp { get; set; }
    public int InvestedCoCount { get; set; }
    public int InvestedMpCount { get; set; }
    public int CountComp { get; set; }
    public int CountSpeedrun { get; set; }
    public int CountBacklog { get; set; }
    public int CountReview { get; set; }
    public int ReviewScore { get; set; }
    public int CountPlaying { get; set; }
    public int CountRetired { get; set; }
    public string ProfileDev { get; set; } = string.Empty;
    public int ProfilePopular { get; set; }
    public int ProfileSteam { get; set; }
    public string ProfilePlatform { get; set; } = string.Empty;
    public int ReleaseWorld { get; set; }
}