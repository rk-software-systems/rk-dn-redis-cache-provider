using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Tests.Models;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class CacheSetServiceTest
    {
        #region fields

        private readonly ICacheService _cacheService;
        #endregion

        public CacheSetServiceTest()
        {
            using var scope = Initialization.CreateScope();
            _cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        }

        #region methods


        [TestMethod]
        public async Task TestSetCacheAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key);
            Assert.IsTrue(source.Equals(result!));
        }

        [TestMethod]
        public async Task TestSetCacheGlobalAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source, true);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);

            Assert.IsTrue(source.Equals(result!));
        }

        [TestMethod]
        public async Task TestSetCacheGlobalDurationAsync()
        {
            var source = CacheTestModel.TestModel;
            var key = CacheTestModel.TestKey;

            await _cacheService.SetCachedObjectAsync<CacheTestModel>(key, source, 1, true);

            var result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            Assert.IsTrue(source.Equals(result!));
            Thread.Sleep(TimeSpan.FromSeconds(2));
            
            result = await _cacheService.GetCachedObjectAsync<CacheTestModel>(key, true);
            Assert.IsNull(result);
        }
        #endregion
    }
}
