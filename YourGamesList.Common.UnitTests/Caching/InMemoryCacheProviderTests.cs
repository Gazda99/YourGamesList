using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using YourGamesList.Common.Caching;

namespace YourGamesList.Common.UnitTests.Caching;

public class InMemoryCacheProviderTests
{
    private IFixture _fixture;
    private IMemoryCache _cache;
    private InMemoryCacheProvider _cacheProvider;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheProvider = new InMemoryCacheProvider(_cache);
    }

    [TearDown]
    public void TearDown()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task Get_WhenValueExists_ReturnsSuccess()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        await _cacheProvider.Set(key, value);

        //ACT
        var result = await _cacheProvider.Get<string>(key);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(value));
    }

    [Test]
    public async Task Get_WhenValueDoesNotExist_ReturnsNotFoundFailure()
    {
        //ACT
        var missingKey = _fixture.Create<string>();
        var result = await _cacheProvider.Get<string>(missingKey);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error, Is.EqualTo(CacheProviderError.NotFound));
    }

    [Test]
    public async Task Set_WhenValueIsNull_RemovesExistingValue()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();
        await _cacheProvider.Set(key, value);

        //ACT
        await _cacheProvider.Set<string>(key, null!);
        var result = await _cacheProvider.Get<string>(key);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(CacheProviderError.NotFound));
    }

    [Test]
    public async Task Set_WhenOptionsAreProvided_StoresEntryWithExpiration()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        await _cacheProvider.Set(key, value, new CacheOptions
        {
            AbsoluteExpiration = TimeSpan.FromMilliseconds(50)
        });

        //ACT
        var resultBeforeExpiration = await _cacheProvider.Get<string>(key);
        await Task.Delay(150);
        var resultAfterExpiration = await _cacheProvider.Get<string>(key);

        //ASSERT
        Assert.That(resultBeforeExpiration.IsSuccess, Is.True);
        Assert.That(resultBeforeExpiration.Value, Is.EqualTo(value));

        Assert.That(resultAfterExpiration.IsSuccess, Is.False);
        Assert.That(resultAfterExpiration.IsFailure, Is.True);
        Assert.That(resultAfterExpiration.Error, Is.EqualTo(CacheProviderError.NotFound));
    }

    [Test]
    public async Task Remove_WhenKeyExists_RemovesValueFromCache()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();
        await _cacheProvider.Set(key, value);

        //ACT
        await _cacheProvider.Remove(key);
        var result = await _cacheProvider.Get<string>(key);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(CacheProviderError.NotFound));
    }
}