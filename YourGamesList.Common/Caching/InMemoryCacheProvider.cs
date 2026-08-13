using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace YourGamesList.Common.Caching;

public class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<CombinedResult<T, CacheProviderError>> Get<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? value) && value is not null)
        {
            return Task.FromResult(CombinedResult<T, CacheProviderError>.Success(value));
        }

        return Task.FromResult(CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.NotFound));
    }

    public Task Set<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (value is null)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        var entryOptions = MapCacheOptions(options);

        _cache.Set(key, value, entryOptions);
        return Task.CompletedTask;
    }

    private static MemoryCacheEntryOptions MapCacheOptions(CacheOptions? options)
    {
        var entryOptions = new MemoryCacheEntryOptions();

        if (options != null)
        {
            if (options.AbsoluteExpiration.HasValue)
            {
                entryOptions.SetAbsoluteExpiration(options.AbsoluteExpiration.Value);
            }

            if (options.SlidingExpiration.HasValue)
            {
                entryOptions.SetSlidingExpiration(options.SlidingExpiration.Value);
            }
        }

        return entryOptions;
    }

    public Task Remove(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}