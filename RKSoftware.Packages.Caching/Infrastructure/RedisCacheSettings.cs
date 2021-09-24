namespace RKSoftware.Packages.Caching.Infrastructure
{
    /// <summary>
    /// This class contains Redis cache settings
    /// </summary>
    public class RedisCacheSettings
    {
        /// <summary>
        /// Path to the Redis instanse
        /// Single Redis connection: localhost:6379
        /// Redis Sentinel: localhost:23679,serviceName=redis_master
        /// Follow StackExchange.Redis endpoint URL convention
        /// In Sentinel mode all READ operations are processed against READ replica first
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "This URL may be not an URL, but StackExchange based Connection")]
        public string RedisUrl { get; set; }

        /// <summary>
        /// Amount of second to keep value in cache in case cache timeout have not been implicitly specified
        /// </summary>
        public long DefaultCacheDuration { get; set; }

        /// <summary>
        /// This key prefix is going to be used as a prefix for Global cache entries
        /// </summary>
        public string GlobalCacheKey { get; set; }

        /// <summary>
        /// Specifies the time in milliseconds that the system should allow for synchronous
        ///  operations (defaults to 5 seconds)
        /// </summary>
        public int? SyncTimeout { get; set; }

        /// <summary>
        /// Size of connection multiplexer pool
        /// </summary>
        public int? ConnectionMultiplexerPoolSize { get; set; }

        /// <summary>
        /// This flag indicates if logs need to be enabled
        /// </summary>
        public bool UseLogging { get; set; }

        /// <summary>
        /// Redis password that can be used to access redis instancess (requirepass redis settings) ACL is not used in this case
        /// </summary>
        public string Password { get; set; }
    }
}
