using System;
using System.Text.Json;

namespace YourGamesList.Common.Caching;

public interface ICacheProvider
{
    bool TryGet<T>(string key, out T? value, JsonSerializerOptions? options = null);
    void Set<T>(string key, T value, TimeSpan? expiration = null, JsonSerializerOptions? options = null);
    void Remove(string key);
}