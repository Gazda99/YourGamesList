using YourGamesList.Services.Hltb.Model;

namespace YourGamesList.Services.Hltb.Services;

public interface IHltbService
{
    Task<HltbSearchResponse?> GetHowLongToBeatDataForGame(string gameName);
}