using System.Threading.Tasks;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoreIntegrationTest.Utils.DB;

[TestSubject(typeof(RedisMemory))]
public class RedisMemoryTest
{
  private readonly RedisMemory _redisMemory;
  public RedisMemoryTest()
  {
    var vault = new Mock<IVault>();
    vault.Setup(x => x.RevealSecret(It.IsAny<CacheConfig>())).Returns(new CacheConfig
    {
      Database = "4",
      DurationMinutes = 5,
      Port = "6379",
      Provider = "redis",
      Server = "localhost"
    });
    var options = new Mock<IOptions<CacheConfig>>();

    var logger = new Mock<ILogger<RedisMemory>>();
    _redisMemory = new RedisMemory(options.Object, vault.Object, logger.Object);
  }

  [Fact]
  public void SetDataCache()
  {
    _redisMemory.SetString("message", "hallo");
    var data = _redisMemory.GetString("message");
      
    Assert.Equal("hallo", data);
  }
    
  [Fact]
  public async Task SetGetDataAsyncCache()
  {
    await _redisMemory.SetStringAsync("message", "hallo");
    var data = await _redisMemory.GetStringAsync("message");
      
    Assert.Equal("hallo", data);
  }
}