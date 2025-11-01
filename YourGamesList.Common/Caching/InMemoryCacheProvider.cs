using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace YourGamesList.Common.Caching;

//TODO: unit tests
public class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet<T>(string key, out T? value, JsonSerializerOptions? options = null)
    {
        value = default;
        if (_cache.TryGetValue(key, out string? serializedValue) && serializedValue is not null)
        {
            value = JsonSerializer.Deserialize<T>(serializedValue, options);
            return value is not null;
        }

        return false;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null, JsonSerializerOptions? options = null)
    {
        var serializedValue = JsonSerializer.Serialize(value, options);
        var cacheOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            cacheOptions.SetAbsoluteExpiration(expiration.Value);
        }

        _cache.Set(key, serializedValue, cacheOptions);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}