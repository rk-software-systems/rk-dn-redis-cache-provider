using System.Threading.Tasks;
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
        #endregion
    }
}
