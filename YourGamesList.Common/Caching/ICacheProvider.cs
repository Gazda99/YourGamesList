using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace YourGamesList.Common.Caching;

public interface ICacheProvider
{
    [Obsolete]
    bool TryGet<T>(string key, [NotNullWhen(true)] out T? value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
    Task<CombinedResult<T, CacheProviderError>> Get<T>(string key, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
    Task Set<T>(string key, T value, TimeSpan? expiration = null, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
    Task Remove(string key, CancellationToken cancellationToken = default);
}