using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Converter.Mock;
using RKSoftware.Packages.Caching.ErrorHandling;
using RKSoftware.Packages.Caching.Infrastructure;
using System;
using System.Collections.Generic;
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
            var provider = GetServiceProvider();

            var parentCacheService = provider.GetService<ICacheService>();
            var str = LoadFile();
            string key = "test";
            await parentCacheService.SetCachedObjectAsync(key, str, true);

            for (var i = 0; i < 10000; i++)
            {
                using var childScope = provider.CreateScope();
                var service = childScope.ServiceProvider.GetService<ICacheService>();

                var tasks = new List<Task>();
                await service.SetCachedObjectAsync(key + i, str, true);
                try
                {
                    service.GetCachedObject<object>(key + (i - 1), true);
                    await Task.Delay(500);
                }
                catch (CacheMissException)
                {
                }
            }
        }

        private static IServiceProvider GetServiceProvider()
        {
            return new ServiceCollection()
                .UseRKSoftwareCache("RK.Test")
                .AddSingleton<IOptions<RedisCacheSettings>>(x =>
                {
                    var settingsMock = new Mock<IOptions<RedisCacheSettings>>();
                    settingsMock.Setup(x => x.Value).Returns(new RedisCacheSettings
                    {
                        SyncTimeout = 5000,
                        ConnectionMultiplexerPoolSize = 3,
                        DefaultCacheDuration = 3600,
                        GlobalCacheKey = "RK.Redis",
                        RedisUrl = "sentinel.master:26379,serviceName=rk_redis_master",
                        Password = "koresh1234567"
                        //RedisUrl = "redis.standalone:6379"
                    });

                    return settingsMock.Object;
                })
                .UseMockJsonTextConverter()
                .AddLogging()
                .UseDefaultConnectionProvider()
                .BuildServiceProvider();
        }

        private static string LoadFile()
        {
            return File.ReadAllText(@"files/sample.json");
        }
    }
}
