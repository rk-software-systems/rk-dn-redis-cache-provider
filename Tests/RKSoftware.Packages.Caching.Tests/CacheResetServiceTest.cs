using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.ErrorHandling;
using RKSoftware.Packages.Caching.Tests.Models;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class CacheResetServiceTest
    {
        #region fields  

        private ICacheService _cacheService;
        #endregion

        #region methods

        [TestInitialize]
        public void Init()
        {
            using var scope = Initialization.CreateScope();
            _cacheService = scope.ServiceProvider.GetService<ICacheService>();
        }

        [TestMethod]
        public void TestResetCache()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            _cacheService.SetCachedObject<CacheTestModel>(key, source);

            _cacheService.Reset(key);

            Assert.ThrowsException<CacheMissException>(() =>
            {
                var result = _cacheService.GetCachedObject<CacheTestModel>(key);
            });
        }

        [TestMethod]
        public void TestResetCacheGlobal()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            _cacheService.SetCachedObject<CacheTestModel>(key, source, true);

            _cacheService.Reset(key, true);

            Assert.ThrowsException<CacheMissException>(() =>
            {
                var result = _cacheService.GetCachedObject<CacheTestModel>(key, true);
            });
        }

        [TestMethod]
        public async Task TestResetCacheAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source);

            await _cacheService.ResetAsync(key);

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key);
            });
        }

        [TestMethod]
        public async Task TestResetCacheGlobalAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source, true);

            await _cacheService.ResetAsync(key, true);

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                var result = await  _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            });
        }

        [TestMethod]
        public void TestResetCacheBulk()
        {
            var source = CacheTestModel.TestModel;
            var key1 = CacheTestModel.TestKey;
            var key2 = CacheTestModel.TestKey + "_2";

            _cacheService.SetCachedObject<CacheTestModel>(key1, source);
            _cacheService.SetCachedObject<CacheTestModel>(key2, source);

            _cacheService.ResetBulk(new string[] { key1, key2 });

            Assert.ThrowsException<CacheMissException>(() =>
            {
                var result = _cacheService.GetCachedObject<CacheTestModel>(key1);
            });

            Assert.ThrowsException<CacheMissException>(() =>
            {
                var result = _cacheService.GetCachedObject<CacheTestModel>(key2);
            });
        }

        [TestMethod]
        public void TestResetCacheBulkGlobal()
        {
            var source = CacheTestModel.TestModel;
            var key1 = CacheTestModel.TestKey;
            var key2 = CacheTestModel.TestKey + "_2";

            _cacheService.SetCachedObject<CacheTestModel>(key1, source, true);
            _cacheService.SetCachedObject<CacheTestModel>(key2, source, true);

            _cacheService.ResetBulk(new string[] { key1, key2 }, true);

            Assert.ThrowsException<CacheMissException>(() =>
            {
                var result = _cacheService.GetCachedObject<CacheTestModel>(key1, true);
            });

            Assert.ThrowsException<CacheMissException>(() =>
            {
                var result = _cacheService.GetCachedObject<CacheTestModel>(key2, true);
            });
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

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key1);
            });

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key2);
            });
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

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key1, true);
            });

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key2, true);
            });
        }
        #endregion
    }
}
