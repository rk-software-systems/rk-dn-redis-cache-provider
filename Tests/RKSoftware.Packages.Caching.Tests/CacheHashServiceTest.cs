using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Tests.Models;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class CacheHashServiceTest
    {
        #region fields

        private const string HashKey = "hash.test";

        private readonly ICacheService _cacheService;
        #endregion

        public CacheHashServiceTest()
        {
            using var scope = Initialization.CreateScope();
            _cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        }

        #region methods

        [TestMethod]
        public async Task TestSetHashFieldAsync()
        {
            var source = CacheTestModel.TestModel;

            await _cacheService.SetCachedHashFieldAsync(HashKey, "model", source);

            var result = await _cacheService.GetCachedHashFieldAsync<CacheTestModel>(HashKey, "model");
            Assert.IsNotNull(result);
            Assert.IsTrue(source.Equals(result));
        }

        [TestMethod]
        public async Task TestSetHashFieldGlobalAsync()
        {
            var source = CacheTestModel.TestModel;

            await _cacheService.SetCachedHashFieldAsync(HashKey, "model", source, true);

            Assert.IsNull(await _cacheService.GetCachedHashFieldAsync<CacheTestModel>(HashKey, "model"),
                "a global hash must not be visible through the project scoped key");

            var result = await _cacheService.GetCachedHashFieldAsync<CacheTestModel>(HashKey, "model", true);
            Assert.IsNotNull(result);
            Assert.IsTrue(source.Equals(result));
        }

        [TestMethod]
        public async Task TestHashFieldBytesRoundTripAsync()
        {
            // bytes have to come back exactly as written, not passed through the object converter
            var bytes = Encoding.UTF8.GetBytes("raw \u0000 payload");

            await _cacheService.SetCachedHashFieldAsync(HashKey, "raw", bytes);

            using var stream = await _cacheService.GetCachedHashFieldStreamAsync(HashKey, "raw");
            Assert.IsNotNull(stream);

            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            CollectionAssert.AreEqual(bytes, memory.ToArray());
        }

        [TestMethod]
        public async Task TestMissingHashFieldReturnsNullAsync()
        {
            Assert.IsNull(await _cacheService.GetCachedHashFieldAsync<CacheTestModel>("hash.missing", "model"));
            Assert.IsNull(await _cacheService.GetCachedHashFieldStreamAsync("hash.missing", "raw"));

            await _cacheService.SetCachedHashFieldAsync(HashKey, "raw", new byte[] { 1 });

            // the hash exists, only the field is missing
            Assert.IsNull(await _cacheService.GetCachedHashFieldStreamAsync(HashKey, "other"));
        }

        [TestMethod]
        public async Task TestSetHashFieldsAsync()
        {
            await _cacheService.SetCachedHashFieldsAsync(HashKey,
                [("a", new byte[] { 1, 2 }), ("b", new byte[] { 3 })],
                120,
                false);

            foreach (var (field, expected) in new[] { ("a", new byte[] { 1, 2 }), ("b", new byte[] { 3 }) })
            {
                using var stream = await _cacheService.GetCachedHashFieldStreamAsync(HashKey, field);
                Assert.IsNotNull(stream);

                using var memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                CollectionAssert.AreEqual(expected, memory.ToArray());
            }

            Assert.AreEqual(TimeSpan.FromSeconds(120), Initialization.HashTtls[$"{Initialization.ProjectName}.{HashKey}"],
                "the hash time to live has to be applied together with the fields");
        }

        [TestMethod]
        public async Task TestResetHashFieldsAsync()
        {
            await _cacheService.SetCachedHashFieldsAsync(HashKey, ("a", new byte[] { 1 }), ("b", new byte[] { 2 }));

            await _cacheService.ResetHashFieldsAsync(HashKey, ["a"]);

            Assert.IsNull(await _cacheService.GetCachedHashFieldStreamAsync(HashKey, "a"));
            using var remaining = await _cacheService.GetCachedHashFieldStreamAsync(HashKey, "b");
            Assert.IsNotNull(remaining, "fields that were not reset have to be kept");
        }

        [TestMethod]
        public async Task TestSetHashFieldsInvalidArgumentsAsync()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _cacheService.SetCachedHashFieldsAsync(HashKey, [("a", new byte[] { 1 })], 0, false));

            await Assert.ThrowsAsync<ArgumentException>(
                () => _cacheService.SetCachedHashFieldsAsync(HashKey, ("", new byte[] { 1 })));

            await Assert.ThrowsAsync<ArgumentException>(
                () => _cacheService.SetCachedHashFieldsAsync(HashKey, ("a", null!)));

            await Assert.ThrowsAsync<ArgumentException>(
                () => _cacheService.SetCachedHashFieldsAsync(HashKey, Array.Empty<(string, byte[])>()));

            Assert.IsFalse(Initialization.HashTtls.Any(), "nothing should have been written");
        }
        #endregion
    }
}
