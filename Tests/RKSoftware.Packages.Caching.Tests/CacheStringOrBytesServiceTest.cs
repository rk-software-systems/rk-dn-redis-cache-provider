using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RKSoftware.Packages.Caching.Implementation;
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.Tests.Models;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class CacheStringOrBytesServiceTest
    {
        private IDatabase _db;        

        [TestInitialize]
        public void Init()
        {
            var settingsMoq = new Mock<IOptions<RedisCacheSettings>>();
            settingsMoq.Setup(x => x.Value)
                .Returns(() =>
                {
                    return new RedisCacheSettings
                    {
                        GlobalCacheKey = "Global",
                        RedisUrl = "127.0.0.1:6379",
                        DefaultCacheDuration = 3600
                    };
                });

            var loggerMoq = new Mock<ILogger<RedisConnectionProvider>>();

            var connectionProvider = new RedisConnectionProvider(settingsMoq.Object, loggerMoq.Object);
            var redis = connectionProvider.GetConnection();
            _db = redis.GetDatabase();
        }

        #region set methods

        [TestMethod]
        public async Task TestCacheSetIntToString()
        {
            var testObj = 5;

            var key = new RedisKey("test_key_int_to_string");
            var strValue = JsonSerializer.Serialize(testObj);
            await _db.StringSetAsync(key, strValue);
        }

        [TestMethod]
        public async Task TestCacheSetIntToBytes()
        {
            var testObj = 5;

            var key = new RedisKey("test_key_int_to_bytes");
            var bytesValue = JsonSerializer.SerializeToUtf8Bytes(testObj);

            await _db.StringSetAsync(key, bytesValue);
        }

        [TestMethod]
        public async Task TestCacheSetString()
        {
            var testObj = await GetObject();

            var key = new RedisKey("test_key_str");
            var strValue = JsonSerializer.Serialize(testObj, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.
            });
            await _db.StringSetAsync(key, strValue);
        }

        [TestMethod]
        public async Task TestCacheSetBytes()
        {
            var testObj = await GetObject();

            var key = new RedisKey("test_key_bytes");
            var bytesValue = JsonSerializer.SerializeToUtf8Bytes(testObj);

            await _db.StringSetAsync(key, bytesValue);
        }

        [TestMethod]
        public async Task TestCacheSetStream()
        {
            var testObj = await GetObject();

            var key = new RedisKey("test_key_stream");

            var bytesValue = JsonSerializer.SerializeToUtf8Bytes(testObj);
            using var ms = new MemoryStream(bytesValue);
            var redisValue = RedisValue.CreateFrom(ms);

            await _db.StringSetAsync(key, redisValue);
        }

        #endregion

        #region get methods
        [TestMethod]
        public async Task TestCacheGetString()
        {
            var key = new RedisKey("test_key_str");
            var strValue = await _db.StringGetAsync(key);
            var testObj = JsonSerializer.Deserialize<BigObjectTestModel>(strValue);
            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public async Task TestCacheGetIntFromString()
        {
            var key = new RedisKey("test_key_int_to_string");
            var strValue = await _db.StringGetAsync(key);
            var testObj = JsonSerializer.Deserialize<int?>(strValue);
            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public async Task TestCacheGetBytes()
        {
            var key = new RedisKey("test_key_str");
            var bytesValue = await _db.StringGetLeaseAsync(key);
            var stream = bytesValue.AsStream();
            var testObj = await JsonSerializer.DeserializeAsync<BigObjectTestModel>(stream);
            Assert.IsNotNull(testObj);
        }

        [TestMethod]
        public async Task TestCachegetIntFromBytes()
        {
            var key = new RedisKey("test_key_int_to_bytes");
            var bytesValue = await _db.StringGetLeaseAsync(key);
            var stream = bytesValue.AsStream();
            var testObj = await JsonSerializer.DeserializeAsync<int?>(stream);
            Assert.IsNotNull(testObj);
        }
        #endregion

        #region helper
        private static async Task<BigObjectTestModel> GetObject()
        {
            var path = Path.Combine("files", "big_object.json");
            using var fs = File.OpenRead(path);
            var obj = await JsonSerializer.DeserializeAsync<BigObjectTestModel>(fs, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            obj.Data = obj.Data.Take(10).ToList();

            return obj;
        }
        #endregion


    }
}


