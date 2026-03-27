using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YourGamesList.Api.Services.Igdb;

public class ApiCalypseQueryBuilder
{
    private const string FieldsParamName = "fields";
    private const string ExcludeParamName = "exclude";
    private const string WhereParamName = "where";
    private const string LimitParamName = "limit";
    private const string OffsetParamName = "offset";
    private const string SortParamName = "sort";
    private const string QueryParamName = "query";

    private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

    private ApiCalypseQueryBuilder()
    {
    }

    public static ApiCalypseQueryBuilder Build()
    {
        return new ApiCalypseQueryBuilder();
    }

    /// <summary>
    /// Adds "fields" parameter to ApiCalypse query
    /// </summary>
    /// <param name="fields">Fields e.g. 'name,release_dates,genres.name,rating'</param>
    public ApiCalypseQueryBuilder WithFields(string fields)
    {
        return WithString(FieldsParamName, fields);
    }

    /// <summary>
    /// Adds "fields" parameter to ApiCalypse query
    /// </summary>
    /// <param name="fields">Fields e.g. [name, release_dates, genres.name, rating]</param>
    public ApiCalypseQueryBuilder WithFields(IEnumerable<string> fields)
    {
        return WithStringSplit(FieldsParamName, fields);
    }

    /// <summary>
    /// Adds "exclude" parameter to ApiCalypse query
    /// </summary>
    /// <param name="excludedFields">Fields e.g. 'name,release_dates,genres.name,rating'</param>
    public ApiCalypseQueryBuilder WithExcludedFields(string excludedFields)
    {
        return WithString(ExcludeParamName, excludedFields);
    }

    /// <summary>
    /// Adds "exclude" parameter to ApiCalypse query
    /// </summary>
    ///<param name="excludedFields">Fields e.g. [name, release_dates, genres.name, rating]</param>
    public ApiCalypseQueryBuilder WithExcludedFields(IEnumerable<string> excludedFields)
    {
        return WithStringSplit(ExcludeParamName, excludedFields);
    }

    /// <summary>
    /// Adds "where" parameter to ApiCalypse query
    /// </summary>
    ///<param name="where">Filter e.g. 'genres = 4'</param>
    public ApiCalypseQueryBuilder WithWhere(string where)
    {
        return WithString(WhereParamName, where);
    }

    /// <summary>
    /// Adds "limit" parameter to ApiCalypse query
    /// </summary>
    ///<param name="limit">Limit e.g. '50'</param>
    public ApiCalypseQueryBuilder WithLimit(int limit)
    {
        return WithString(LimitParamName, limit.ToString());
    }

    /// <summary>
    /// Adds "limit" parameter to ApiCalypse query
    /// </summary>
    ///<param name="offset">Limit e.g. '50'</param>
    public ApiCalypseQueryBuilder WithOffset(int offset)
    {
        return WithString(OffsetParamName, offset.ToString());
    }

    /// <summary>
    /// Adds "sort" parameter to ApiCalypse query
    /// </summary>
    ///<param name="sort">Sort e.g. 'rating asc'</param>
    public ApiCalypseQueryBuilder WithSort(string sort)
    {
        return WithString(SortParamName, sort);
    }

    /// <summary>
    /// Adds "query" parameter to ApiCalypse query
    /// </summary>
    ///<param name="query">Sort e.g. 'games/count "Count of games" {}'</param>
    public ApiCalypseQueryBuilder WithCustomQuery(string query)
    {
        return WithString(QueryParamName, query);
    }

    /// <summary>
    /// Parses all provided parameters to final query
    /// </summary>
    public string CreateQuery()
    {
        var sb = new StringBuilder();

        foreach (var (key, value) in _parameters)
        {
            sb.Append(key);
            sb.Append(' ');
            sb.Append(value);
            if (!value.EndsWith(';'))
            {
                sb.Append(';');
            }
        }

        return sb.ToString();
    }

    private ApiCalypseQueryBuilder WithString(string paramName, string paramValue)
    {
        _parameters[paramName] = paramValue;
        return this;
    }

    private ApiCalypseQueryBuilder WithStringSplit(string paramName, IEnumerable<string> paramValues, string separator = ",")
    {
        var paramValue = string.Join(separator, paramValues.Select(x => x.Trim()));
        return WithString(paramName, paramValue);
    }
}