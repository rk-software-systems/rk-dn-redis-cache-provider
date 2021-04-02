using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.System.Text.Json.Converter;
using System.IO;
using RKSoftware.Packages.Caching.Implementation;
using Microsoft.Extensions.Logging;
using Moq;
using RKSoftware.Packages.Caching.Contract;
using StackExchange.Redis;
using System;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace RKSoftware.Packages.Caching.Tests
{
    internal class Initialization
    {
        private static string _projectName => typeof(Initialization).Namespace;
        private static MemoryCache _cache;

        internal static string ProjectName => _projectName;

        internal static IServiceScope CreateScope()
        {
            var services = new ServiceCollection();

            AddLoggers(services);

            var configuration = LoadConfiguration();
            services.UseRKSoftwareCache(configuration, _projectName)
                .UseSystemTextJsonTextConverter();

            AddConnectionProvider(services);            

            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            return scope;
        }

        private static IConfigurationSection LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            return configuration.GetSection(nameof(RedisCacheSettings));
        }

        private static void AddLoggers(ServiceCollection services)
        {
            var loggerMoq = new Mock<ILogger<CacheService>>();
            services.AddScoped<ILogger<CacheService>>(x => loggerMoq.Object);
            var loggerConnectionMoq = new Mock<ILogger<RedisConnectionProvider>>();
            services.AddScoped<ILogger<RedisConnectionProvider>>(x => loggerConnectionMoq.Object);
        }

        private static void AddConnectionProvider(ServiceCollection services)
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {                
                SizeLimit = 1024
            });

            var databaseMoq = new Mock<IDatabase>();
            databaseMoq
                .Setup(x => x.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, RedisValue value, TimeSpan sec, When when, CommandFlags flags) =>
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(sec);

                    _cache.Set<string>(key, value, cacheEntryOptions);
                    return true;
                });
            databaseMoq
               .Setup(x => x.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.TryGetValue<string>(key, out string value);
                   return value;
               });
            databaseMoq
                .Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, RedisValue value, TimeSpan sec, When when, CommandFlags flags) =>
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(sec);

                    _cache.Set<string>(key, value, cacheEntryOptions);
                    return Task.FromResult(true);
                });
            databaseMoq
               .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.TryGetValue<string>(key, out string value);
                   var result = new RedisValue(value);
                   return Task.FromResult(result);
               });
            databaseMoq
               .Setup(x => x.KeyDelete(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.Remove(key);
                   return true;
               });
            databaseMoq
               .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.Remove(key);
                   return Task.FromResult(true);
               });
            databaseMoq
               .Setup(x => x.KeyDelete(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey[] keys, CommandFlags flags) =>
               {
                   foreach(var key in keys)
                   {
                       _cache.Remove(key);
                   }                  
                   return keys.Length;
               });
            databaseMoq
               .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey[] keys, CommandFlags flags) =>
               {
                   foreach (var key in keys)
                   {
                       _cache.Remove(key);
                   }
                   return Task.FromResult((long)keys.Length);
               });

            var connectionMultiplexerMoq = new Mock<IConnectionMultiplexer>();
            connectionMultiplexerMoq
                .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(databaseMoq.Object);

            var connectionProviderMoq = new Mock<IConnectionProvider>();
            connectionProviderMoq
                .Setup(x => x.GetConnection())
                .Returns(connectionMultiplexerMoq.Object);
            services.AddScoped<IConnectionProvider>(x => connectionProviderMoq.Object);
        }
    }
}
