using System;

namespace RKSoftware.Packages.Caching.Infrastructure
{
    /// <summary>
    /// This class contains Redis cache settings
    /// </summary>
    public class RedisCacheSettings
    {
        /// <summary>
        /// Path to the Redis container
        /// </summary>
        public Uri RedisUrl { get; set; }

        /// <summary>
        /// Amount of second to keep value in cache in case cache timeout have not been implicitly specified
        /// </summary>
        public long DefaultCacheDuration { get; set; }

        /// <summary>
        /// This key prefix is going to be used as a prefix for Global cache entries
        /// </summary>
        public string GlobalCacheKey { get; set; }
    }
}
