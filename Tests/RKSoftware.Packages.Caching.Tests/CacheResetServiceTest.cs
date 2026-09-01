using System.Threading.Tasks;
using System.Linq;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Tests.Models;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class CacheResetServiceTest
    {
        #region fields

        private readonly ICacheService _cacheService;
        #endregion

        #region methods
        public CacheResetServiceTest()
        {
            using var scope = Initialization.CreateScope();
            _cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        }

        [TestMethod]
        public async Task TestResetCacheAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source);

            await _cacheService.ResetAsync(key);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TestResetCacheGlobalAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source, true);

            await _cacheService.ResetAsync(key, true);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TestResetCacheBulkAsync()
        {
            var source = CacheTestModel.TestModel;
            var key1 = CacheTestModel.TestKey;
            var key2 = CacheTestModel.TestKey + "_2";

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key1, source);
            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key2, source);

            await _cacheService.ResetBulkAsync(new string[] { key1, key2 });

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key1);
            Assert.IsNull(result);
            result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key2);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TestResetCacheBulkGlobalAsync()
        {
            var source = CacheTestModel.TestModel;
            var key1 = CacheTestModel.TestKey;
            var key2 = CacheTestModel.TestKey + "_2";

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key1, source, true);
            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key2, source, true);

            await _cacheService.ResetBulkAsync(new string[] { key1, key2 }, true);


            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key1, true);
            Assert.IsNull(result);

            result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key2, true);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TestResetCacheBulkPartOfKeyAsync()
        {
            using var scope = Initialization.CreateScope(RedisTopologyKind.Standalone, out var servers);
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            var source = CacheTestModel.TestModel;
            const string partOfKey = "standalone.reset.";
            var key1 = partOfKey + "1";
            var key2 = partOfKey + "1" + FakeRedisTopology.ClusterShardBSuffix;

            await cacheService.SetCachedObjectAsync<CacheTestModel>(key1, source);
            await cacheService.SetCachedObjectAsync<CacheTestModel>(key2, source);

            await cacheService.ResetBulkAsync(partOfKey);

            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(key1));
            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(key2));

            // a single master holds the whole keyspace, replicas and Sentinel nodes are never scanned
            foreach (var server in servers.Where(server => server.Name != "master"))
            {
                Assert.AreEqual(0, server.ScanCount, $"{server.Name} was scanned for keys");
            }

            Assert.AreEqual(
                1,
                servers.Single(server => server.Name == "master").ScanCount,
                "the connected master is the node that has to be scanned");
        }

        [TestMethod]
        public async Task TestResetCacheBulkPartOfKeyClusterAsync()
        {
            using var scope = Initialization.CreateScope(RedisTopologyKind.Cluster, out var servers);
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            var source = CacheTestModel.TestModel;
            const string partOfKey = "cluster.reset.";
            var shardAKey = partOfKey + "1";
            var shardBKey = partOfKey + "1" + FakeRedisTopology.ClusterShardBSuffix;

            await cacheService.SetCachedObjectAsync<CacheTestModel>(shardAKey, source);
            await cacheService.SetCachedObjectAsync<CacheTestModel>(shardBKey, source);

            await cacheService.ResetBulkAsync(partOfKey);

            // keys of every shard have to be removed, not only the ones of the first endpoint
            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(shardAKey));
            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(shardBKey));

            Assert.AreEqual(1, servers.Single(server => server.Name == "shard-a").ScanCount);
            Assert.AreEqual(1, servers.Single(server => server.Name == "shard-b").ScanCount);
            Assert.AreEqual(0, servers.Single(server => server.Name == "shard-a-replica").ScanCount);
        }

        [TestMethod]
        public async Task TestResetCacheBulkPartOfKeyNoMasterAsync()
        {
            using var scope = Initialization.CreateScope(RedisTopologyKind.NoMaster, out var servers);
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            const string partOfKey = "nomaster.reset.";
            await cacheService.SetCachedObjectAsync<CacheTestModel>(partOfKey + "1", CacheTestModel.TestModel);

            // reporting success while deleting nothing would leave the caller with a cache it believes is clear
            await Assert.ThrowsExactlyAsync<RedisConnectionException>(
                () => cacheService.ResetBulkAsync(partOfKey));

            foreach (var server in servers)
            {
                Assert.AreEqual(0, server.ScanCount, $"{server.Name} was scanned for keys");
            }
        }

        [TestMethod]
        public async Task TestResetCacheBulkPartOfKeyClusterSlotBatchesAsync()
        {
            using var scope = Initialization.CreateScope(RedisTopologyKind.Cluster, out _);
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            var source = CacheTestModel.TestModel;
            const string partOfKey = "cluster.slot.";
            // both keys are held by shard-a, neither carries the shard-b suffix
            var key1 = partOfKey + "alpha";
            var key2 = partOfKey + "omega";

            AssertKeysUseDifferentSlots(key1, key2);

            await cacheService.SetCachedObjectAsync<CacheTestModel>(key1, source);
            await cacheService.SetCachedObjectAsync<CacheTestModel>(key2, source);

            await cacheService.ResetBulkAsync(partOfKey);

            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(key1));
            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(key2));

            AssertDeletesAreSlotSafe();
        }

        [TestMethod]
        public async Task TestResetCacheBulkClusterSlotBatchesAsync()
        {
            using var scope = Initialization.CreateScope(RedisTopologyKind.Cluster, out _);
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            var source = CacheTestModel.TestModel;
            var key1 = "cluster.exact.alpha";
            var key2 = "cluster.exact.omega";

            AssertKeysUseDifferentSlots(key1, key2);

            await cacheService.SetCachedObjectAsync<CacheTestModel>(key1, source);
            await cacheService.SetCachedObjectAsync<CacheTestModel>(key2, source);

            await cacheService.ResetBulkAsync(new string[] { key1, key2 });

            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(key1));
            Assert.IsNull(await cacheService.GetCachedObjectAsync<CacheTestModel>(key2));

            AssertDeletesAreSlotSafe();
        }
        #endregion

        #region helpers

        /// <summary>
        /// Guard the slot batching assertions against passing for the wrong reason:
        /// keys that share a slot would be batched together even by an unsafe implementation
        /// </summary>
        private static void AssertKeysUseDifferentSlots(string key1, string key2)
        {
            var prefix = Initialization.ProjectName + ".";
            Assert.AreNotEqual(
                FakeRedisTopology.SlotOf(prefix + key1),
                FakeRedisTopology.SlotOf(prefix + key2),
                "the test keys have to land in different slots for this test to mean anything");
        }

        /// <summary>
        /// A Redis Cluster rejects a multi key DEL whose keys do not all hash to the same slot,
        /// so every batch that was sent has to be non empty and confined to a single slot
        /// </summary>
        private static void AssertDeletesAreSlotSafe()
        {
            Assert.IsTrue(Initialization.DeleteBatches.Count > 0, "no delete was issued at all");

            foreach (var batch in Initialization.DeleteBatches)
            {
                Assert.IsTrue(batch.Length > 0, "an empty DEL was issued");

                var slots = batch.Select(FakeRedisTopology.SlotOf).Distinct().ToArray();
                Assert.AreEqual(
                    1,
                    slots.Length,
                    $"a DEL spanned {slots.Length} slots: {string.Join(", ", batch)}");
            }
        }
        #endregion
    }
}
