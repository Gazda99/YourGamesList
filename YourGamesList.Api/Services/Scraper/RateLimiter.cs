using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Api.Services.Scraper;

//TODO: unit tests
public class RateLimiter
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly ConcurrentQueue<DateTimeOffset> _callTimestamps = new ConcurrentQueue<DateTimeOffset>();

    private readonly ILogger<RateLimiter> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _timeFrame;
    private readonly int _maxConcurrentCallsWithinTimeFrame;

    public RateLimiter(
        ILogger<RateLimiter> logger,
        TimeProvider timeProvider,
        int timeFrameInMilliseconds,
        int maxConcurrentCallsWithinTimeFrame
    )
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _timeFrame = TimeSpan.FromMilliseconds(timeFrameInMilliseconds);
        _maxConcurrentCallsWithinTimeFrame = maxConcurrentCallsWithinTimeFrame;
    }

    public async Task WaitAsync()
    {
        while (true)
        {
            await _lock.WaitAsync();

            var cutoff = _timeProvider.GetUtcNow() - _timeFrame;

            while (_callTimestamps.TryPeek(out var timestamp) && timestamp < cutoff)
            {
                _callTimestamps.TryDequeue(out _);
            }

            if (_callTimestamps.Count < _maxConcurrentCallsWithinTimeFrame)
            {
                // We have a slot! Enqueue the current time and exit.
                _callTimestamps.Enqueue(_timeProvider.GetUtcNow());
                _lock.Release();
                return;
            }

            if (!_callTimestamps.TryPeek(out var oldestCall))
            {
                // Should not happen if count > 0, but as a safeguard
                continue;
            }

            var timeToWait = oldestCall + _timeFrame - _timeProvider.GetUtcNow();
            //Just to be safe
            if (timeToWait <= TimeSpan.Zero)
            {
                timeToWait = TimeSpan.FromMilliseconds(50);
            }
            else
            {
                _logger.LogDebug($"Exceeded rate limit, waiting for '{timeToWait.TotalMilliseconds}' ms...");
            }

            // Release lock before waiting inside this thread, so others can check the queue while this one is waiting
            _lock.Release();

            await Task.Delay(timeToWait, _timeProvider);
        }
    }
}