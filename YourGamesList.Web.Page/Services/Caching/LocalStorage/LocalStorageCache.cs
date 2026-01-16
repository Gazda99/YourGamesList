using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Web.Page.Services.Caching.LocalStorage.Model;

namespace YourGamesList.Web.Page.Services.Caching.LocalStorage;

//TODO: unit tests
public class LocalStorageCache : ICacheProvider
{
    private readonly ILogger<LocalStorageCache> _logger;
    private readonly IJSRuntime _jsRuntime;
    private readonly TimeProvider _timeProvider;

    public LocalStorageCache(
        ILogger<LocalStorageCache> logger,
        IJSRuntime jsRuntime,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
        _timeProvider = timeProvider;
    }

    public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        var getResult = Get<T>(key, options, cancellationToken).Result;
        if (getResult.IsSuccess)
        {
            value = getResult.Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public async Task<CombinedResult<T, CacheProviderError>> Get<T>(string key, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
            if (item == null)
            {
                _logger.LogInformation("No local storage item found for '{LocalStorageKey}'.", key);
                return CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.NotFound);
            }
            else
            {
                _logger.LogInformation("Got local storage item '{LocalStorageKey}'.", key);
                var deserializedItem = JsonSerializer.Deserialize<LocalStorageItem<T>>(item, options);

                if (deserializedItem == null)
                {
                    _logger.LogInformation("No local storage item found for '{LocalStorageKey}'.", key);
                    return CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.NotFound);
                }

                var now = _timeProvider.GetUtcNow();
                if (now > deserializedItem.LastModified.UtcDateTime + deserializedItem.Ttl)
                {
                    _logger.LogInformation("Local storage item '{LocalStorageKey}' has expired.", key);
                    return CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.Expired);
                }

                _logger.LogDebug("Local storage item '{LocalStorageKey}' successfully obtained.", key);
                return CombinedResult<T, CacheProviderError>.Success(deserializedItem.Item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get local storage item '{LocalStorageKey}'.", key);
            return CombinedResult<T, CacheProviderError>.Failure(CacheProviderError.Other);
        }
    }


    public async Task Set<T>(string key, T value, TimeSpan? expiration = null, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var localStorageItem = new LocalStorageItem<T>
            {
                Item = value,
                LastModified = _timeProvider.GetUtcNow(),
                Ttl = expiration ?? TimeSpan.FromDays(1)
            };
            var serializedItem = JsonSerializer.Serialize(localStorageItem, options);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, serializedItem);
            _logger.LogInformation("Set local storage item '{LocalStorageKey}'.", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set local storage item '{LocalStorageKey}'.", key);
        }
    }

    public async Task Remove(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing local storage item '{LocalStorageKey}'.", key);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, key);
    }
}