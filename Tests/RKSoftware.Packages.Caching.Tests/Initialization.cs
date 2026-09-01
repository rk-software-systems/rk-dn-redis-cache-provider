using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RKSoftware.Packages.Caching.Infrastructure;
using System.IO;
using RKSoftware.Packages.Caching.Implementation;
using Microsoft.Extensions.Logging;
using Moq;
using RKSoftware.Packages.Caching.Contract;
using StackExchange.Redis;
using System;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RKSoftware.Packages.Caching.Converter.Mock;

namespace RKSoftware.Packages.Caching.Tests
{
    internal static class Initialization
    {
        private static string _projectName => typeof(Initialization).Namespace!;
        private static MemoryCache? _cache;
        private static readonly List<RedisKey[]> _deleteBatches = [];

        internal static string ProjectName => _projectName;

        /// <summary>
        /// Keys currently held by the cache storage behind the <see cref="IDatabase"/> mock
        /// </summary>
        internal static IEnumerable<string> CacheKeys =>
            _cache == null ? [] : _cache.Keys.Select(key => key.ToString()!).ToArray();

        /// <summary>
        /// Multi key deletes that were issued against the <see cref="IDatabase"/> mock, one entry
        /// per DEL command. Lets a test assert how the keys were batched, which is what decides
        /// whether a delete is safe on a Redis Cluster.
        /// </summary>
        internal static IReadOnlyList<RedisKey[]> DeleteBatches => _deleteBatches;

        internal static IServiceScope CreateScope() =>
            CreateScope(RedisTopologyKind.Standalone, out _);

        internal static IServiceScope CreateScope(RedisTopologyKind topology,
            out IReadOnlyList<FakeRedisServer> servers)
        {
            var services = new ServiceCollection();

            AddLoggers(services);

            var configuration = LoadConfiguration();
            services.UseRKSoftwareCache(configuration, _projectName)
                .UseMockJsonTextConverter();

            servers = AddConnectionProvider(services, topology);

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

        private static IReadOnlyList<FakeRedisServer> AddConnectionProvider(ServiceCollection services,
            RedisTopologyKind topology)
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024
            });
            _deleteBatches.Clear();

            var databaseMoq = new Mock<IDatabase>();
            databaseMoq
                .Setup(x => x.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, RedisValue value, TimeSpan sec, bool keepTtl, When when, CommandFlags flags) =>
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(sec);

                    _cache.Set<string>(key.ToString(), value!, cacheEntryOptions);
                    return true;
                });
            databaseMoq
                .Setup(x => x.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, RedisValue value, TimeSpan sec, When when, CommandFlags flags) =>
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(sec);

                    _cache.Set<string>(key.ToString(), value!, cacheEntryOptions);
                    return true;
                });
            databaseMoq
               .Setup(x => x.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.TryGetValue<string>(key.ToString(), out string? value);
                   return value;
               });
            databaseMoq
                .Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, RedisValue value, TimeSpan sec, bool keepTtl, When when, CommandFlags flags) =>
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(sec);

                    _cache.Set<string>(key.ToString(), value!, cacheEntryOptions);
                    return Task.FromResult(true);
                });
            databaseMoq
                .Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, RedisValue value, TimeSpan sec, When when, CommandFlags flags) =>
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(sec);

                    _cache.Set<string>(key.ToString(), value!, cacheEntryOptions);
                    return Task.FromResult(true);
                });
            databaseMoq
               .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.TryGetValue<string>(key.ToString(), out string? value);
                   var result = new RedisValue(value!);
                   return Task.FromResult(result);
               });
            databaseMoq
               .Setup(x => x.KeyDelete(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.Remove(key.ToString());
                   return true;
               });
            databaseMoq
               .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey key, CommandFlags flags) =>
               {
                   _cache.Remove(key.ToString());
                   return Task.FromResult(true);
               });
            databaseMoq
               .Setup(x => x.KeyDelete(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey[] keys, CommandFlags flags) =>
               {
                   foreach (var key in keys)
                   {
                       _cache.Remove(key.ToString());
                   }
                   return keys.Length;
               });
            databaseMoq
               .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
               .Returns((RedisKey[] keys, CommandFlags flags) =>
               {
                   _deleteBatches.Add(keys);
                   foreach (var key in keys)
                   {
                       _cache.Remove(key.ToString());
                   }
                   return Task.FromResult((long)keys.Length);
               });

            var servers = FakeRedisTopology.Create(topology, () => CacheKeys);

            var connectionMultiplexerMoq = new Mock<IConnectionMultiplexer>();
            connectionMultiplexerMoq
                .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(databaseMoq.Object);
            connectionMultiplexerMoq
                .Setup(x => x.GetServers())
                .Returns(servers.Select(server => server.Server).ToArray());
            connectionMultiplexerMoq
                .Setup(x => x.GetEndPoints(It.IsAny<bool>()))
                .Returns(servers.Select(server => server.EndPoint).ToArray());
            connectionMultiplexerMoq
                .Setup(x => x.GetServer(It.IsAny<EndPoint>(), It.IsAny<object>()))
                .Returns((EndPoint endPoint, object asyncState) =>
                    servers.First(server => server.EndPoint.Equals(endPoint)).Server);
            connectionMultiplexerMoq
                .Setup(x => x.GetHashSlot(It.IsAny<RedisKey>()))
                .Returns((RedisKey key) => FakeRedisTopology.SlotOf(key));

            var connectionProviderMoq = new Mock<IConnectionProvider>();
            connectionProviderMoq
                .Setup(x => x.GetConnection())
                .Returns(connectionMultiplexerMoq.Object);
            services.AddScoped<IConnectionProvider>(x => connectionProviderMoq.Object);

            return servers;
        }
    }
}
