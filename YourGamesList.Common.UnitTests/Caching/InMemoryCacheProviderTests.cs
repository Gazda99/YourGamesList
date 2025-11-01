using System.Text.Json;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using YourGamesList.Common.Caching;

namespace YourGamesList.Common.UnitTests.Caching;

public class InMemoryCacheProviderTests
{
    private IFixture _fixture;
    private IMemoryCache _cache;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _cache = Substitute.For<IMemoryCache>();
    }

    [Test]
    public void TryGet_WhenEntryFound_ReturnsTrueAndEntry()
    {
        //ARRANGE
        var testEntry = _fixture.Create<TestEntry>();
        var serializedValue = JsonSerializer.Serialize(testEntry);
        var key = _fixture.Create<string>();
        _cache.TryGetValue(key, out Arg.Any<object?>()).Returns(x =>
        {
            x[1] = serializedValue;
            return true;
        });

        var inMemoryCacheProvider = new InMemoryCacheProvider(_cache);

        //ACT
        var res = inMemoryCacheProvider.TryGet<TestEntry>(key, out var cacheEntry);

        //ASSERT
        Assert.That(res, Is.True);
        Assert.That(cacheEntry, Is.Not.Null);
        Assert.That(cacheEntry.X, Is.EqualTo(testEntry.X));
        _cache.Received(1).TryGetValue(key, out Arg.Any<object?>());
    }

    [Test]
    public void TryGet_WhenEntryNotFound_ReturnsFalse()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        _cache.TryGetValue(key, out Arg.Any<object?>()).Returns(x =>
        {
            x[1] = null;
            return false;
        });

        var inMemoryCacheProvider = new InMemoryCacheProvider(_cache);

        //ACT
        var res = inMemoryCacheProvider.TryGet<TestEntry>(key, out _);

        //ASSERT
        Assert.That(res, Is.False);
        _cache.Received(1).TryGetValue(key, out Arg.Any<object?>());
    }

    [Test]
    public void Set_SetsCacheEntry()
    {
        //ARRANGE

        var key = _fixture.Create<string>();
        var testEntry = _fixture.Create<TestEntry>();
        var cacheEntry = Substitute.For<ICacheEntry>();
        _cache.CreateEntry(key).Returns(cacheEntry);
        var inMemoryCacheProvider = new InMemoryCacheProvider(_cache);

        //ACT
        inMemoryCacheProvider.Set(key, testEntry);

        //ASSERT
        _cache.Received(1).CreateEntry(key);
        Assert.That(cacheEntry.Value, Is.EquivalentTo(JsonSerializer.Serialize(testEntry)));
    }

    [Test]
    public void Remove_CallsUnderlyingRemove()
    {
        //ARRANGE
        var key = _fixture.Create<string>();
        var inMemoryCacheProvider = new InMemoryCacheProvider(_cache);

        //ACT
        inMemoryCacheProvider.Remove(key);

        //ASSERT
        _cache.Received(1).Remove(key);
    }

    private class TestEntry
    {
        public string X { get; set; }
    }
}