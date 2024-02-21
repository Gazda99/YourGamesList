using YourGamesList.Common.Services.Hltb.Responses;

namespace YourGamesList.Common.Services.Hltb.Model;

public class HltbSimple
{
    public string GameName { get; init; }
    public string GameAlias { get; init; }
    public int MainStoryTime { get; init; }
    public int MainStoryCount { get; init; }
    public int MainAndSidesTime { get; init; }
    public int MainAndSidesCount { get; init; }
    public int CompletionistTime { get; init; }
    public int CompletionistCount { get; init; }

    public int AllStylesTime => (MainStoryTime + MainAndSidesTime + CompletionistTime) / 3;
    public int AllStylesCount => MainStoryCount + MainAndSidesCount + CompletionistCount;

    public HltbSimple(HltbData hltbData)
    {
        GameName = hltbData.GameName;
        GameAlias = hltbData.GameAlias;
        MainStoryTime = hltbData.CompMain;
        MainStoryCount = hltbData.CompMainCount;
        MainAndSidesTime = hltbData.CompPlus;
        MainAndSidesCount = hltbData.CompPlusCount;
        CompletionistTime = hltbData.CompAll;
        CompletionistCount = hltbData.CompAll;
    }
}