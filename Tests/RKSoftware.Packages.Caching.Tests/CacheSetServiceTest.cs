using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.ErrorHandling;
using RKSoftware.Packages.Caching.Tests.Models;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class CacheSetServiceTest
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
        public void TestSetCache()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            _cacheService.SetCachedObject<CacheTestModel>(key, source);

            var result = _cacheService.GetCachedObject<CacheTestModel>(key);

            Assert.IsTrue(source.Equals(result));
        }
                
        [TestMethod]
        public void TestSetCacheGlobal()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            _cacheService.SetCachedObject<CacheTestModel>(key, source, true);

            var result = _cacheService.GetCachedObject<CacheTestModel>(key, true);

            Assert.IsTrue(source.Equals(result));
        }

        [TestMethod]
        public void TestSetCacheGlobalDuration()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            _cacheService.SetCachedObject<CacheTestModel>(key, source, 1, true);

            var result = _cacheService.GetCachedObject<CacheTestModel>(key, true);
            Assert.IsTrue(source.Equals(result));

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Assert.ThrowsException<CacheMissException>(() =>
            {
                result = _cacheService.GetCachedObject<CacheTestModel>(key, true);
            });
        }

        [TestMethod]
        public async Task TestSetCacheAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key);

            Assert.IsTrue(source.Equals(result));
        }

        [TestMethod]
        public async Task TestSetCacheGlobalAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source, true);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);

            Assert.IsTrue(source.Equals(result));
        }

        [TestMethod]
        public async Task TestSetCacheGlobalDurationAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source, 1, true);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            Assert.IsTrue(source.Equals(result));

            Thread.Sleep(TimeSpan.FromSeconds(2));

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            });
        }
        #endregion
    }
}
