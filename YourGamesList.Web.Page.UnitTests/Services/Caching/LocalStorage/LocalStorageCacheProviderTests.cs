using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using NSubstitute;
using YourGamesList.Common.Caching;
using YourGamesList.TestsUtils;
using YourGamesList.Web.Page.Services.Caching.LocalStorage;
using YourGamesList.Web.Page.Services.Caching.LocalStorage.Model;

namespace YourGamesList.Web.Page.UnitTests.Services.Caching.LocalStorage;

public class LocalStorageCacheProviderTests
{
    private IFixture _fixture;
    private ILogger<LocalStorageCacheProvider> _logger;
    private IJSRuntime _jsRuntime;
    private TimeProvider _timeProvider;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<LocalStorageCacheProvider>>();
        _jsRuntime = Substitute.For<IJSRuntime>();
        _timeProvider = Substitute.For<TimeProvider>();
    }

    [Test]
    public async Task Set_Successfully_SetsItemInLocalStorage()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var testEntry = _fixture.Create<TestEntry>();
        var expiration = TimeSpan.FromHours(1);
        var now = DateTimeOffset.UtcNow;
        _timeProvider.GetUtcNow().Returns(now);
        var localStorageItem = new LocalStorageItem<TestEntry>
        {
            Item = testEntry,
            LastModified = now,
            Ttl = expiration
        };
        var serializedValue = JsonSerializer.Serialize(localStorageItem);
        var localStorageCacheProvider = new LocalStorageCacheProvider(_logger, _jsRuntime, _timeProvider);

        //ACT
        await localStorageCacheProvider.Set(key, testEntry, expiration);

        //ASSERT
        await _jsRuntime.Received(1).InvokeAsync<IJSVoidResult>("localStorage.setItem", Arg.Any<CancellationToken>(),
            Arg.Is<object?[]?>(o =>
                o.Any(x => x.ToString() == key)
                && o.Any(x => x.ToString() == serializedValue)
            ));
        _timeProvider.Received(1).GetUtcNow();
        _logger.ReceivedLog(LogLevel.Information, $"Set local storage item '{key}'.");
    }

    [Test]
    public async Task Get_WhenItemFound_ReturnsItemFromLocalStorage()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var testEntry = _fixture.Create<TestEntry>();
        var expiration = TimeSpan.FromHours(1);
        var now = DateTimeOffset.UtcNow;
        _timeProvider.GetUtcNow().Returns(now);
        var localStorageItem = new LocalStorageItem<TestEntry>
        {
            Item = testEntry,
            LastModified = now,
            Ttl = expiration
        };
        var serializedValue = JsonSerializer.Serialize(localStorageItem);

        _jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            Arg.Any<CancellationToken>(),
            Arg.Is<object?[]>(args => args[0].ToString() == key)
        ).Returns(new ValueTask<string?>(serializedValue));

        var localStorageCacheProvider = new LocalStorageCacheProvider(_logger, _jsRuntime, _timeProvider);

        //ACT
        var res = await localStorageCacheProvider.Get<TestEntry>(key);

        //ASSERT
        await _jsRuntime.Received(1).InvokeAsync<string?>("localStorage.getItem", Arg.Any<CancellationToken>(),
            Arg.Is<object?[]?>(o => o.Any(x => x.ToString() == key)));
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.Not.Null);
        var itemFromCache = localStorageItem.Item;
        Assert.That(itemFromCache, Is.Not.Null);
        Assert.That(itemFromCache.X, Is.EqualTo(testEntry.X));
    }

    [Test]
    public async Task Get_WhenNoItemFound_ReturnsError()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var now = DateTimeOffset.UtcNow;
        _timeProvider.GetUtcNow().Returns(now);

        _jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            Arg.Any<CancellationToken>(),
            Arg.Is<object?[]>(args => args[0].ToString() == key)
        ).Returns(new ValueTask<string?>((string?) null));

        var localStorageCacheProvider = new LocalStorageCacheProvider(_logger, _jsRuntime, _timeProvider);

        //ACT
        var res = await localStorageCacheProvider.Get<TestEntry>(key);

        //ASSERT
        await _jsRuntime.Received(1).InvokeAsync<string?>("localStorage.getItem", Arg.Any<CancellationToken>(),
            Arg.Is<object?[]?>(o => o.Any(x => x.ToString() == key)));
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(CacheProviderError.NotFound));

        _logger.ReceivedLog(LogLevel.Information, $"No local storage item found for '{key}'.");
    }

    [Test]
    public async Task Get_WhenEntryExpired_ReturnsError()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var testEntry = _fixture.Create<TestEntry>();
        var expiration = TimeSpan.FromHours(1);
        var now = DateTimeOffset.UtcNow;
        var lastModified = now - TimeSpan.FromHours(2);
        _timeProvider.GetUtcNow().Returns(now);
        var localStorageItem = new LocalStorageItem<TestEntry>
        {
            Item = testEntry,
            LastModified = lastModified,
            Ttl = expiration
        };
        var serializedValue = JsonSerializer.Serialize(localStorageItem);

        _jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            Arg.Any<CancellationToken>(),
            Arg.Is<object?[]>(args => args[0].ToString() == key)
        ).Returns(new ValueTask<string?>(serializedValue));

        var localStorageCacheProvider = new LocalStorageCacheProvider(_logger, _jsRuntime, _timeProvider);

        //ACT
        var res = await localStorageCacheProvider.Get<TestEntry>(key);

        //ASSERT
        await _jsRuntime.Received(1).InvokeAsync<string?>("localStorage.getItem", Arg.Any<CancellationToken>(),
            Arg.Is<object?[]?>(o => o.Any(x => x.ToString() == key)));
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(CacheProviderError.Expired));

        _logger.ReceivedLog(LogLevel.Information, $"Local storage item '{key}' has expired.");
    }

    [Test]
    public async Task Remove_SuccessfullyRemovesItemFromLocalStorage()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var localStorageCacheProvider = new LocalStorageCacheProvider(_logger, _jsRuntime, _timeProvider);

        //ACT
        await localStorageCacheProvider.Remove(key);

        //ASSERT
        _logger.ReceivedLog(LogLevel.Information, $"Removing local storage item '{key}'.");
        await _jsRuntime.Received(1).InvokeAsync<IJSVoidResult>("localStorage.removeItem", Arg.Any<CancellationToken>(),
            Arg.Is<object?[]?>(o => o.Any(x => x.ToString() == key)));
    }

    private class TestEntry
    {
        public string X { get; set; }
    }
}