using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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

    [Obsolete]
    public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        value = default;
        if (_cache.TryGetValue(key, out string? serializedValue) && serializedValue is not null)
        {
            value = JsonSerializer.Deserialize<T>(serializedValue, options);
            return value is not null;
        }

        return false;
    }

    public Task<CombinedResult<T, CacheProviderError>> Get<T>(string key, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(key, out string? serializedValue))
        {
            return Task.FromResult(CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.NotFound));
        }

        if (serializedValue == null)
        {
            return Task.FromResult(CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.NotFound));
        }

        var value = JsonSerializer.Deserialize<T>(serializedValue, options);

        if (value == null)
        {
            return Task.FromResult(CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.NotFound));
        }

        return Task.FromResult(CombinedResult<T, CacheProviderError>.Success(value));
    }

    public Task Set<T>(string key, T value, TimeSpan? expiration = null, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        var serializedValue = JsonSerializer.Serialize(value, options);

        using var entry = _cache.CreateEntry(key);

        if (expiration.HasValue)
        {
            var cacheOptions = new MemoryCacheEntryOptions();
            cacheOptions.SetAbsoluteExpiration(expiration.Value);
            entry.SetOptions(cacheOptions);
        }

        entry.Value = serializedValue;
        return Task.CompletedTask;
    }

    public Task Remove(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}