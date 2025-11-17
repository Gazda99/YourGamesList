using System.Collections.Generic;

namespace YourGamesList.Web.Page.Services.Ygl.Model.Responses;

public class AvailableSearchQueryArgumentsResponse
{
    public List<string> GameTypes { get; init; } = [];
    public List<string> Genres { get; init; }= [];
    public List<string> Themes { get; init; }= [];
}