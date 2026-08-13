using System.Threading;
using System.Threading.Tasks;

namespace YourGamesList.Common.Caching;

public interface ICacheProvider
{
    Task<CombinedResult<T, CacheProviderError>> Get<T>(string key, CancellationToken cancellationToken = default);

    Task Set<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default);

    Task Remove(string key, CancellationToken cancellationToken = default);
}