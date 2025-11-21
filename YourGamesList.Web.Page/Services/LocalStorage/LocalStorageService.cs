using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using YourGamesList.Common;
using YourGamesList.Web.Page.Services.LocalStorage.Model;

namespace YourGamesList.Web.Page.Services.LocalStorage;

public interface ILocalStorageService
{
    Task<CombinedResult<T, LocalStorageError>> GetItem<T>(string key, CancellationToken cancellationToken = default);
    Task SetItem<T>(string key, T data, TimeSpan? ttl, CancellationToken cancellationToken = default);
    Task RemoveItem(string key, CancellationToken cancellationToken = default);
}

//TODO: unit tests
public class LocalStorageService : ILocalStorageService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ILogger<LocalStorageService> _logger;
    private readonly IJSRuntime _jsRuntime;
    private readonly TimeProvider _timeProvider;

    public LocalStorageService(
        ILogger<LocalStorageService> logger,
        IJSRuntime jsRuntime,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
        _timeProvider = timeProvider;
    }

    public async Task<CombinedResult<T, LocalStorageError>> GetItem<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
            if (item == null)
            {
                _logger.LogInformation("No local storage item found for '{LocalStorageKey}'.", key);
                return CombinedResult<T, LocalStorageError>.Failure(LocalStorageError.NotFound);
            }
            else
            {
                _logger.LogInformation("Got local storage item '{LocalStorageKey}'.", key);
                var deserializedItem = JsonSerializer.Deserialize<LocalStorageItem<T>>(item, _jsonSerializerOptions);

                if (deserializedItem == null)
                {
                    _logger.LogInformation("No local storage item found for '{LocalStorageKey}'.", key);
                    return CombinedResult<T, LocalStorageError>.Failure(LocalStorageError.NotFound);
                }

                var now = _timeProvider.GetUtcNow();
                if (now > deserializedItem.LastModified.UtcDateTime + deserializedItem.Ttl)
                {
                    _logger.LogInformation("Local storage item '{LocalStorageKey}' has expired.", key);
                    return CombinedResult<T, LocalStorageError>.Failure(LocalStorageError.Expired);
                }

                _logger.LogDebug("Local storage item '{LocalStorageKey}' successfully obtained.", key);
                return CombinedResult<T, LocalStorageError>.Success(deserializedItem.Item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get local storage item '{LocalStorageKey}'.", key);
            return CombinedResult<T, LocalStorageError>.Failure(LocalStorageError.Other);
        }
    }

    public async Task SetItem<T>(string key, T data, TimeSpan? ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var localStorageItem = new LocalStorageItem<T>
            {
                Item = data,
                LastModified = _timeProvider.GetUtcNow(),
                Ttl = ttl ?? TimeSpan.FromDays(1)
            };
            var serializedItem = JsonSerializer.Serialize(localStorageItem, _jsonSerializerOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, serializedItem);
            _logger.LogInformation("Set local storage item '{LocalStorageKey}'.", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set local storage item '{LocalStorageKey}'.", key);
        }
    }

    public async Task RemoveItem(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing local storage item '{LocalStorageKey}'.", key);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, key);
    }
}