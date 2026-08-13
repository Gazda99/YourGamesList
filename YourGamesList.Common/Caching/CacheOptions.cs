using System;

namespace YourGamesList.Common.Caching;

public record CacheOptions
{
    /// <summary>
    /// Absolute time until the entry expires.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; init; }

    /// <summary>
    /// Time period after which the entry expires if not accessed.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; init; }
}