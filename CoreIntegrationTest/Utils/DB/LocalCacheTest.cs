using System;
using System.Threading.Tasks;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoreIntegrationTest.Utils.DB;

public class LocalCacheTest
{
    [Fact]
    public async Task GetStringAsync_WhenCalled_ReturnsString()
    {
        // Arrange
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var logger = new Mock<ILogger<LocalCache>>();
        var cacheConfig = new Mock<IOptions<CacheConfig>>();
        var vault = new Mock<IVault>();
        vault.Setup(x => x.RevealSecret(It.IsAny<CacheConfig>())).Returns(new CacheConfig { DurationMinutes = 1 });
        var localCache = new LocalCache(memoryCache, cacheConfig.Object, vault.Object, logger.Object);
        var key = "key";
        var value = "value";
        
        memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });

        // Act
        var result = await localCache.GetStringAsync(key);

        // Assert
        Assert.Equal(value, result);
    } 

    [Fact]
    public async Task SetStringAsync_WhenCalled_ReturnsVoid()
    {
        // Arrange
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var logger = new Mock<ILogger<LocalCache>>();
        var cacheConfig = new Mock<IOptions<CacheConfig>>();
        var vault = new Mock<IVault>();
        vault.Setup(x => x.RevealSecret(It.IsAny<CacheConfig>())).Returns(new CacheConfig { DurationMinutes = 1 });
        var localCache = new LocalCache(memoryCache, cacheConfig.Object, vault.Object, logger.Object);
        var key = "key";
        var value = "value";

        // Act
        await localCache.SetStringAsync(key, value);

        // Assert
        Assert.Equal(value, memoryCache.Get<string>(key));
    }

    [Fact]
    public async Task RemoveAsync_WhenCalled_ReturnsVoid()
    {
        // Arrange
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var logger = new Mock<ILogger<LocalCache>>();
        var cacheConfig = new Mock<IOptions<CacheConfig>>();
        var vault = new Mock<IVault>();
        vault.Setup(x => x.RevealSecret(It.IsAny<CacheConfig>())).Returns(new CacheConfig { DurationMinutes = 1 });
        var localCache = new LocalCache(memoryCache, cacheConfig.Object, vault.Object, logger.Object);
        var key = "key";
        var value = "value";
      
        memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });

        // Act
        await localCache.RemoveAsync(key);

        // Assert
        Assert.Null(memoryCache.Get<string>(key));
    }

    [Fact]
    public void Remove_WhenCalled_ReturnsVoid()
    {
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var logger = new Mock<ILogger<LocalCache>>();
        var cacheConfig = new Mock<IOptions<CacheConfig>>();
        var vault = new Mock<IVault>();
        vault.Setup(x => x.RevealSecret(It.IsAny<CacheConfig>())).Returns(new CacheConfig { DurationMinutes = 1 });
        var localCache = new LocalCache(memoryCache, cacheConfig.Object, vault.Object, logger.Object);
        var key = "key";
        var value = "value";
        
        memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });
        localCache.Remove(key);
        
        Assert.Null(memoryCache.Get<string>(key));
    }
}