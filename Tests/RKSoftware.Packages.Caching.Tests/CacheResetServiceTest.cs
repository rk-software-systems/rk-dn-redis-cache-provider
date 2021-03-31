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
        #endregion
    }
}
