using System.Threading;
using System.Threading.Tasks;

namespace YourGamesList.Common.Caching;

public interface ICacheProvider
{
    /// <summary>
    /// Gets a cached value.
    /// </summary>
    Task<CombinedResult<T, CacheProviderError>> Get<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value in the cache.
    /// </summary>
    Task Set<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task Remove(string key, CancellationToken cancellationToken = default);
}