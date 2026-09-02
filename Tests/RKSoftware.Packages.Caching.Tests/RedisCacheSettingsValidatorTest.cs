using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RKSoftware.Packages.Caching.Infrastructure;

namespace RKSoftware.Packages.Caching.Tests
{
    [TestClass]
    public class RedisCacheSettingsValidatorTest
    {
        #region fields

        private const string ValidUrl = "test.url";
        private const long ValidDuration = 3600;

        private readonly RedisCacheSettingsValidator _validator = new RedisCacheSettingsValidator();
        #endregion

        #region helpers

        private static RedisCacheSettings GetSettings(string redisUrl = ValidUrl,
            long defaultCacheDuration = ValidDuration) => new RedisCacheSettings
            {
                RedisUrl = redisUrl,
                DefaultCacheDuration = defaultCacheDuration,
                GlobalCacheKey = "Global"
            };
        #endregion

        #region methods

        [TestMethod]
        public void TestValidSettingsSucceed()
        {
            var result = _validator.Validate(null, GetSettings());

            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.Failed);
            Assert.IsNull(result.FailureMessage);
        }

        [TestMethod]
        public void TestSmallestUsableDurationSucceeds()
        {
            var result = _validator.Validate(null, GetSettings(defaultCacheDuration: 1));

            Assert.IsTrue(result.Succeeded);
        }

        [TestMethod]
        public void TestNamedOptionsAreValidated()
        {
            // The validator ignores the options name, every named instance is held to the same rules
            var result = _validator.Validate("secondary", GetSettings(defaultCacheDuration: 0));

            Assert.IsTrue(result.Failed);
        }

        [TestMethod]
        public void TestMissingSettingsFail()
        {
            var result = _validator.Validate(null, null!);

            Assert.IsTrue(result.Failed);
            Assert.IsTrue(result.FailureMessage!.Contains(nameof(RedisCacheSettings), StringComparison.Ordinal));
        }

        [TestMethod]
        [DataRow(0L, DisplayName = "duration not configured at all binds to zero")]
        [DataRow(-1L, DisplayName = "negative duration")]
        [DataRow(long.MinValue, DisplayName = "extreme negative duration")]
        public void TestNonPositiveDurationFails(long duration)
        {
            var result = _validator.Validate(null, GetSettings(defaultCacheDuration: duration));

            Assert.IsTrue(result.Failed);
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Failures!.Count());
            Assert.IsTrue(result.FailureMessage!.Contains(nameof(RedisCacheSettings.DefaultCacheDuration), StringComparison.Ordinal),
                $"failure message should name the setting, was: {result.FailureMessage}");
        }

        [TestMethod]
        [DataRow("", DisplayName = "empty connection")]
        [DataRow("   ", DisplayName = "whitespace only connection")]
        public void TestMissingRedisUrlFails(string connection)
        {
            var result = _validator.Validate(null, GetSettings(redisUrl: connection));

            Assert.IsTrue(result.Failed);
            Assert.AreEqual(1, result.Failures!.Count());
            Assert.IsTrue(result.FailureMessage!.Contains(nameof(RedisCacheSettings.RedisUrl), StringComparison.Ordinal),
                $"failure message should name the setting, was: {result.FailureMessage}");
        }

        [TestMethod]
        public void TestEveryFailureIsReportedAtOnce()
        {
            // A misconfigured section should surface all of its problems in one go, so that fixing
            // one setting does not just reveal the next one on the following run
            var result = _validator.Validate(null, GetSettings(redisUrl: "", defaultCacheDuration: 0));

            Assert.IsTrue(result.Failed);
            Assert.AreEqual(2, result.Failures!.Count());
            Assert.IsTrue(result.FailureMessage!.Contains(nameof(RedisCacheSettings.RedisUrl), StringComparison.Ordinal));
            Assert.IsTrue(result.FailureMessage.Contains(nameof(RedisCacheSettings.DefaultCacheDuration), StringComparison.Ordinal));
        }

        [TestMethod]
        public void TestOptionalSettingsAreNotRequired()
        {
            // Only RedisUrl and DefaultCacheDuration are mandatory, leaving the rest unset is valid
            var settings = new RedisCacheSettings
            {
                RedisUrl = ValidUrl,
                DefaultCacheDuration = ValidDuration
            };

            var result = _validator.Validate(null, settings);

            Assert.IsTrue(result.Succeeded);
            Assert.IsNull(settings.GlobalCacheKey);
            Assert.IsNull(settings.SyncTimeout);
            Assert.IsNull(settings.ConnectionMultiplexerPoolSize);
            Assert.IsNull(settings.Password);
            Assert.IsFalse(settings.UseLogging);
        }
        #endregion
    }
}
