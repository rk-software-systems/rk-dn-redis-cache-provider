using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Infrastructure;
using StackExchange.Redis;
using System;
using System.Linq;

namespace RKSoftware.Packages.Caching.Implementation
{
    /// <summary>
    /// This service is used to get / set object to Cache
    /// </summary>
    public partial class CacheService : ICacheService
    {
        #region consts

        private const string GLOBAL_CACHE_KEY = "RKSoftware.Global";
        #endregion

        #region fields

        private readonly RedisCacheSettings _redisCacheSettings;
        private readonly string _projectName;
        private readonly ILogger _logger;
        private readonly IConnectionProvider _connectionProvider;
        private readonly ICacheRepository _cacheRepository;
        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class
        /// </summary>
        /// <param name="redisCacheProvider">Redis connectivity settings</param>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="connectionProvider">This class is used to obtain Connection Multiplexer <see cref="IConnectionProvider"/></param>
        /// <param name="cacheRepository"></param>
        /// <param name="scopedKeyPrefix">This prefix is used as a starting part of Redis DB keys</param>
        public CacheService(IOptions<RedisCacheSettings> redisCacheProvider,
            ILogger<CacheService> logger,
            IConnectionProvider connectionProvider,
            ICacheRepository cacheRepository,
            string scopedKeyPrefix)
        {
            if (redisCacheProvider == null)
            {
                throw new ArgumentNullException(nameof(redisCacheProvider));
            }

            if (string.IsNullOrEmpty(scopedKeyPrefix))
            {
                throw new ArgumentNullException(nameof(scopedKeyPrefix));
            }

            _redisCacheSettings = redisCacheProvider.Value;
            _logger = logger;
            _connectionProvider = connectionProvider;
            _projectName = scopedKeyPrefix;
            _cacheRepository = cacheRepository;
        }

        #endregion

        #region helpers

        /// <summary>
        /// Get Redis Database connection
        /// </summary>
        /// <returns>Redis database connection</returns>
        private IDatabase GetDatabase()
        {
            var redis = _connectionProvider.GetConnection();

            return redis.GetDatabase();
        }

        private string GetGlobalKey()
        {
            return string.IsNullOrEmpty(_redisCacheSettings.GlobalCacheKey) ?
                    GLOBAL_CACHE_KEY : _redisCacheSettings.GlobalCacheKey;
        }

        /// <summary>
        /// Get key that were used to store object
        /// </summary>
        /// <param name="key">Key that is used inside project</param>
        /// <param name="global">This flag indicates if key should be project unspecific</param>        
        /// <returns>Inter project independent cache key</returns>
        private string GetFullyQualifiedKey(string key,
            bool global)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (global)
            {
                return $"{GetGlobalKey()}.{key}";
            }
            else
            {
                return $"{_projectName}.{key}";
            }
        }

        /// <summary>
        /// Get Fully qualified partial key based on global and project name
        /// </summary>
        /// <param name="keyPart">Key part</param>
        /// <param name="global">Create Global Key</param>
        /// <returns></returns>
        private string GetFullyQualifiedPartialKey(string keyPart, bool global)
        {
            return GetFullyQualifiedKey($"*{keyPart}*", global);
        }

        /// <summary>
        /// Get all keys for specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>list of keys</returns>
        private RedisKey[] GetKeys(string partOfKey, bool global)
        {
            var keyPattern = GetFullyQualifiedPartialKey(partOfKey, global);
            var connection = _connectionProvider.GetConnection();
            var endPoint = connection.GetEndPoints().First();
            var keyArr = connection.GetServer(endPoint).Keys(pattern: $"{keyPattern}").ToArray();
            return keyArr;
        }
        #endregion

        #region logging

        private static readonly Action<ILogger, string, Exception> _logRedisObjectNotFoundWarning = LoggerMessage.Define<string>(
            LogLevel.Warning,
            LoggingConstants.RedisObjectNotFoundWarning,
            LogMessageResource.RedisObjectNotFound);

        private static readonly Action<ILogger, string, Exception> _logRedisGetObjectConnectionError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisGetObjectConnectionError,
            LogMessageResource.RedisGetObjectConnectionError);

        private static readonly Action<ILogger, string, Exception> _logRedisSetObjectError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisSetObjectError,
            LogMessageResource.RedisSetObjectError);

        private static readonly Action<ILogger, string, Exception> _logRedisConnectionError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisConnectionError,
            LogMessageResource.RedisConnectionError);

        private static readonly Action<ILogger, string, Exception> _logRedisGetObjectError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisGetObjectError,
            LogMessageResource.RedisGetObjectError);

        private static readonly Action<ILogger, string, Exception> _logRedisGetObjectInformation = LoggerMessage.Define<string>(
            LogLevel.Information,
            LoggingConstants.RedisGetObjectInformation,
            LogMessageResource.RedisGetObject);

        private static readonly Action<ILogger, string, Exception> _logRedisSetObjectConnectionError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisSetObjectConnectionError,
            LogMessageResource.RedisSetObjectError);

        private static readonly Action<ILogger, string, Exception> _logRedisSetObjectInformation = LoggerMessage.Define<string>(
            LogLevel.Information,
            LoggingConstants.RedisSetObjectInformation,
            LogMessageResource.RedisSetObject);

        private static readonly Action<ILogger, string, Exception> _logRedisResetConnectionError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisResetConnectionError,
            LogMessageResource.RedisConnectionError);

        private static readonly Action<ILogger, string, Exception> _logRedisRemoveObjectError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisRemoveObjectError,
            LogMessageResource.RedisRemoveObjectError);

        private static readonly Action<ILogger, string, Exception> _logRedisBulkResetError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisBulkResetError,
            LogMessageResource.RedisBulkResetError);

        private static readonly Action<ILogger, string, Exception> _logRedisBulkResetConnectionError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisBulkResetConnectionError,
            LogMessageResource.RedisBulkResetError);

        private static readonly Action<ILogger, string, Exception> _logRedisBulkPartialResetConnectionError = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisBulkPartialResetConnectionError,
            LogMessageResource.RedisBulkPartialReset);

        private static readonly Action<ILogger, string, Exception> _logRedisBulkPartialReset = LoggerMessage.Define<string>(
            LogLevel.Error,
            LoggingConstants.RedisBulkPartialReset,
            LogMessageResource.RedisBulkPartialReset);

        #endregion
    }
}
