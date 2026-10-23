using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace YourGamesList.Common.Caching;

public class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

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

    public async Task<IAsyncDisposable> AcquireLock(string key, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), "Lock lease duration must be greater than zero.");
        }

        var semaphore = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            return new SemaphoreLockLease(semaphore, leaseDuration);
        }
        catch
        {
            semaphore.Release();
            throw;
        }
    }

    private sealed class SemaphoreLockLease : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Timer _expirationTimer;
        private int _isDisposed;

        public SemaphoreLockLease(SemaphoreSlim semaphore, TimeSpan leaseDuration)
        {
            _semaphore = semaphore;
            _expirationTimer = new Timer(
                static state => ((SemaphoreLockLease)state!).Dispose(),
                this,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
            _expirationTimer.Change(leaseDuration, Timeout.InfiniteTimeSpan);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        private void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            {
                return;
            }

            _expirationTimer.Dispose();
            _semaphore.Release();
        }
    }
}