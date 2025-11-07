using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace YourGamesList.Common.Caching;

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

        using var entry = _cache.CreateEntry(key);

        if (expiration.HasValue)
        {
            var cacheOptions = new MemoryCacheEntryOptions();
            cacheOptions.SetAbsoluteExpiration(expiration.Value);
            entry.SetOptions(cacheOptions);
        }

        entry.Value = serializedValue;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}