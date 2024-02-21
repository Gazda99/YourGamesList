using YourGamesList.Common.Services.Hltb.Responses;

namespace YourGamesList.Common.Services.Hltb;

public interface IHltbService
{
    Task<HltbSearchResponse?> GetHowLongToBeatDataForGame(string gameName);
}