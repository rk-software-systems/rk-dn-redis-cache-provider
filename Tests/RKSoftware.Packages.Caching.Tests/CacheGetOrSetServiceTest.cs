using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Contract;
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

        private readonly ICacheService _cacheService;

        #endregion

        #region methods
        public CacheGetOrSetServiceTest()
        {
            using var scope = Initialization.CreateScope();
            _cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        }

        [TestMethod]
        public async Task TestGetOrSetCacheAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = await _cacheService.GetOrSetCachedObjectAsync<CacheTestModel>(key, () => Task.FromResult(source)!);

            Assert.IsTrue(source.Equals(result1!));

            var result2 = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key);

            Assert.IsTrue(result1!.Equals(result2!));
        }

        [TestMethod]
        public async Task TestGetOrSetCacheGlobalAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = await _cacheService.GetOrSetCachedObjectAsync<CacheTestModel>(key, () => Task.FromResult(source)!, true);

            Assert.IsTrue(source.Equals(result1!));

            var result2 = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);

            Assert.IsTrue(result1!.Equals(result2!));
        }

        [TestMethod]
        public async Task TestGetOrSetCacheGlobalDurationPositiveAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            var result1 = await _cacheService.GetOrSetCachedObjectAsync<CacheTestModel>(key, () => Task.FromResult(source)!, 3, true);

            Assert.IsTrue(source.Equals(result1!));

            await Task.Delay(TimeSpan.FromSeconds(2));

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            Assert.IsNotNull(result);

            await Task.Delay(TimeSpan.FromSeconds(4));

            result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            Assert.IsNull(result);
        }

        #endregion
    }
}
