using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RKSoftware.Packages.Caching.Implementation;
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.Newtonsoft.Json.Converter;
using System.IO;
using System.Threading.Tasks;

namespace TestCluster
{
    /// <summary>
    /// This console application is used to TEST Cache provider performance against Actual Redis cache storage
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            var loggerMockProvider = new Mock<ILogger<RedisConnectionProvider>>();
            var loggerMockService = new Mock<ILogger<CacheService>>();

            var settingsMock = new Mock<IOptions<RedisCacheSettings>>();
            settingsMock.Setup(x => x.Value).Returns(new RedisCacheSettings
            {
                SyncTimeout = 5000,
                ConnectionMultiplexerPoolSize = 3,
                DefaultCacheDuration = 3600,
                GlobalCacheKey = "RK.Redis",
                RedisUrl = "sentinel.master:26379,serviceName=rk_redis_master"
                //RedisUrl = "redis.standalone:6379"
            });
            var converter = new NewtonsoftJsonTextConverter();
            var provider = new RedisConnectionProvider(settingsMock.Object,
                loggerMockProvider.Object);
            var service = new CacheService(settingsMock.Object,
                loggerMockService.Object,
                provider,
                converter,
                "RK.Test");
            var str = LoadFile();
            string key = "test";
            await service.SetCachedObjectAsync(key, str);
            var result = await service.GetCachedObjectAsync<string>(key);
        }

        private static string LoadFile()
        {
            return File.ReadAllText(@"files/sample.json");
        }
    }
}
