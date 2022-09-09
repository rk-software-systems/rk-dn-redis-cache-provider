using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RKSoftware.Packages.Caching.Implementation;
using RKSoftware.Packages.Caching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class ConnectionProviderTest
    {
        private RedisConnectionProvider GetConnectionProvider()
        {
            var loggerMockProvider = new Mock<ILogger<RedisConnectionProvider>>();

            var settingsMock = new Mock<IOptions<RedisCacheSettings>>();
            settingsMock.Setup(x => x.Value).Returns(new RedisCacheSettings
            {
                SyncTimeout = 5000,
                ConnectionMultiplexerPoolSize = 3,
                DefaultCacheDuration = 3600,
                GlobalCacheKey = "RK.Redis",
                RedisUrl = "sentinel.master:26379,serviceName=rk_redis_master",
                Password = "1234567"
            });

            return new RedisConnectionProvider(settingsMock.Object,
                loggerMockProvider.Object);
        }

        [TestMethod]
        public void TestDisposingWithNoMultiplexers()
        {
            var provider = GetConnectionProvider();
            provider.Dispose();
        }
    }
}
