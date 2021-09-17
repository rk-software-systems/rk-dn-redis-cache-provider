using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.ErrorHandling;
using RKSoftware.Packages.Caching.Tests.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class CacheGetOrSetServiceTest
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
        public void TestGetOrSetCache()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = _cacheService.GetOrSetCachedObject<CacheTestModel>(key, () => source);

            Assert.IsTrue(source.Equals(result1));

            var result2 = _cacheService.GetCachedObject<CacheTestModel>(key);

            Assert.IsTrue(result1.Equals(result2));
        }

        [TestMethod]
        public void TestGetOrSetCacheGlobal()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = _cacheService.GetOrSetCachedObject<CacheTestModel>(key, () => source, true);

            Assert.IsTrue(source.Equals(result1));

            var result2 = _cacheService.GetCachedObject<CacheTestModel>(key, true);

            Assert.IsTrue(result1.Equals(result2));
        }

        [TestMethod]
        public void TestGetOrSetCacheGlobalDuration()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = _cacheService.GetOrSetCachedObject<CacheTestModel>(key, () => source, 1, true);

            Assert.IsTrue(source.Equals(result1));

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Assert.ThrowsException<CacheMissException>(() =>
            {
                var result = _cacheService.GetCachedObject<CacheTestModel>(key, true);
            });
        }

        [TestMethod]
        public async Task TestGetOrSetCacheAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = await _cacheService.GetOrSetCachedObjectAsync<CacheTestModel>(key, () => Task.FromResult(source));

            Assert.IsTrue(source.Equals(result1));

            var result2 = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key);

            Assert.IsTrue(result1.Equals(result2));
        }

        [TestMethod]
        public async Task TestGetOrSetCacheGlobalAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = await _cacheService.GetOrSetCachedObjectAsync<CacheTestModel>(key, () => Task.FromResult(source), true);

            Assert.IsTrue(source.Equals(result1));

            var result2 = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);

            Assert.IsTrue(result1.Equals(result2));
        }

        [TestMethod]
        public async Task TestGetOrSetCacheGlobalDurationAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = await _cacheService.GetOrSetCachedObjectAsync<CacheTestModel>(key, () => Task.FromResult(source), 1, true);

            Assert.IsTrue(source.Equals(result1));

            Thread.Sleep(TimeSpan.FromSeconds(2));

            await Assert.ThrowsExceptionAsync<CacheMissException>(async () =>
            {
                var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            });
        }
        #endregion
    }
}
