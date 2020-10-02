using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.ErrorHandling;
using RKSoftware.Packages.Caching.Infrastructure;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RKSoftware.Packages.Caching.Implementation
{
    /// <summary>
    /// This service is used to get / set object to Cache
    /// </summary>
    public class CacheService : ICacheService
    {
        #region Constants

        private const string GLOBAL_CACHE_KEY = "RKSoftware.Global";

        #endregion

        #region fields

        private readonly RedisCacheSettings _redisCacheSettings;
        private readonly string _projectName;
        private readonly ILogger _logger;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IObjectToTextConverter _objectConverter;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class
        /// </summary>
        /// <param name="redisCacheProvider">Redis connectivity settings</param>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="connectionProvider">This class is used to obtain Connection Multiplexer <see cref="IConnectionProvider"/></param>
        /// <param name="objectConverter">This service is used to serialize objects from / to strings</param>
        /// <param name="scopedKeyPrefix">This prefix is used as a starting part of Redis DB keys</param>
        public CacheService(IOptions<RedisCacheSettings> redisCacheProvider,
            ILogger<CacheService> logger,
            IConnectionProvider connectionProvider,
            IObjectToTextConverter objectConverter,
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
            _objectConverter = objectConverter;
            _projectName = scopedKeyPrefix;
        }

        #endregion

        #region methods

        /// <summary>
        /// Get object from cache using cache Key
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        public T GetCachedObject<T>(string key)
        {
            return GetCachedObject<T>(key, false);
        }

        /// <summary>
        /// Get object from cache using cache Key
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        public T GetCachedObject<T>(string key, bool global)
        {
            key = GetFullyQualifiedKey(key, global);

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            IDatabase db;
            string result;

            try
            {
                db = GetDatabase();

                result = db.StringGet(key);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisConnectionError, key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisGetObjectError, key);
                throw;
            }

            if (string.IsNullOrEmpty(result))
            {
                throw new CacheMissException();
            }

            return _objectConverter.FromString<T>(result);
        }

        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <exception cref="ArgumentNullException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        public Task<T> GetCachedObjectAsync<T>(string key)
        {
            return GetCachedObjectAsync<T>(key, false);
        }

        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <exception cref="ArgumentNullException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        public async Task<T> GetCachedObjectAsync<T>(string key, bool global)
        {
            key = GetFullyQualifiedKey(key, global);

            if (string.IsNullOrEmpty(key))
            {
                throw new CacheMissException();
            }

            IDatabase db;
            string result;
            try
            {
                db = GetDatabase();

                result = await db.StringGetAsync(key)
                    .ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisConnectionError, key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisGetObjectError, key);
                throw;
            }

            if (string.IsNullOrEmpty(result))
            {
                throw new CacheMissException();
            }

            return _objectConverter.FromString<T>(result);
        }

        /// <summary>
        /// Get object from cache.
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache</returns>
        public T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver)
        {
            return GetOrSetCachedObject(key, objectReceiver, false);
        }

        /// <summary>
        /// Get object from cache.
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, bool global)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetBase(key, objectReceiver, null, global);
        }

        /// <summary>
        /// Get object from cache.
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache</returns>
        public T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, long storageDuration)
        {
            return GetOrSetCachedObject(key, objectReceiver, storageDuration, false);
        }

        /// <summary>
        /// Get object from cache.
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, long storageDuration, bool global)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetBase(key, objectReceiver, storageDuration, global);
        }

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver)
        {
            return GetOrSetCachedObjectAsync(key, objectReceiver, false);
        }

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver, bool global)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetAsyncBase(key, objectReceiver, null, global);
        }

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver, long storageDuration)
        {
            return GetOrSetCachedObjectAsync(key, objectReceiver, storageDuration, false);
        }

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver, long storageDuration, bool global)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetAsyncBase(key, objectReceiver, storageDuration, global);
        }

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver)
        {
            return GetOrSetCachedObjectAsync(key, objectReceiver, false);
        }

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, bool global)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetAsyncBase(key, objectReceiver, null, global);
        }

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, long storageDuration)
        {
            return GetOrSetCachedObjectAsync(key, objectReceiver, storageDuration, false);
        }

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, long storageDuration, bool global)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetAsyncBase(key, objectReceiver, storageDuration, global);
        }

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        public void Reset(string key)
        {
            Reset(key, false);
        }

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public void Reset(string key, bool global)
        {
            key = GetFullyQualifiedKey(key, global);

            try
            {
                var db = GetDatabase();

                db.KeyDelete(key, flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisConnectionError, key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisRemoveObjectError, key);
                throw;
            }
        }

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <returns>Task awaiter</returns>
        public Task ResetAsync(string key)
        {
            return ResetAsync(key, false);
        }

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task ResetAsync(string key, bool global)
        {
            key = GetFullyQualifiedKey(key, global);

            try
            {
                var db = GetDatabase();

                return db.KeyDeleteAsync(key, flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisRemoveObjectError, key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisRemoveObjectError, key);
                throw;
            }
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">list of cache storage key</param>
        /// <param name="projectName">specific project system name</param>
        public void ResetBulk(IEnumerable<string> keys, string projectName = null)
        {
            ResetBulk(keys, false, projectName);
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">list of cache storage key</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <param name="projectName">specific project system name</param>
        public void ResetBulk(IEnumerable<string> keys, bool global, string projectName = null)
        {
            var keyArr = new List<RedisKey>();
            if (keys == null || (keys?.Count()).GetValueOrDefault() == 0)
            {
                // no keys to delete, exit
                return;
            }

            foreach (var key in keys)
            {
                keyArr.Add(GetFullyQualifiedKey(key, global, projectName));
            }

            try
            {
                var db = GetDatabase();
                db.KeyDelete(keyArr.ToArray(), flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisBulkResetError, keyArr);
                throw;
            }
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        /// <param name="projectName">specific project system name</param>
        public Task ResetBulkAsync(IEnumerable<string> keys, string projectName = null)
        {
            return ResetBulkAsync(keys, false, projectName);
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <param name="projectName">specific project system name</param>
        public Task ResetBulkAsync(IEnumerable<string> keys, bool global, string projectName = null)
        {
            if (keys == null || (keys?.Count()).GetValueOrDefault() == 0)
            {
                throw new ArgumentException(LogMessageResource.RedisBulkResetNoKeys, nameof(keys));
            }

            var keyArr = new List<RedisKey>();
            foreach (var key in keys)
            {
                keyArr.Add(GetFullyQualifiedKey(key, global, projectName));
            }

            try
            {
                var db = GetDatabase();
                return db.KeyDeleteAsync(keyArr.ToArray(), flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisBulkResetError, keyArr);
                throw;
            }
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="projectName">project system name</param>
        public void ResetBulk(string partOfKey, string projectName = null)
        {
            ResetBulk(partOfKey, false, projectName);
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, 
        /// for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        /// <param name="projectName">project system name</param>
        public void ResetBulk(string partOfKey, bool globalCache, string projectName = null)
        {
            try
            {
                var keyArr = GetKeys(partOfKey, projectName, globalCache);
                var db = GetDatabase();
                db.KeyDelete(keyArr, flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisBulkPartialReset, partOfKey, projectName ?? "");
                throw;
            }
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="projectName">project system name</param>
        public Task ResetBulkAsync(string partOfKey, string projectName = null)
        {
            return ResetBulkAsync(partOfKey, false, projectName);
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        /// <param name="projectName">project system name</param>
        public Task ResetBulkAsync(string partOfKey, bool globalCache, string projectName = null)
        {
            try
            {
                var keyArr = GetKeys(partOfKey, projectName, globalCache);
                var db = GetDatabase();
                return db.KeyDeleteAsync(keyArr, flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisBulkPartialReset, partOfKey, projectName ?? "");
                throw;
            }
        }

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        public void SetCachedObject<T>(string key, T obj)
        {
            SetCachedObject(key, obj, false);
        }

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public void SetCachedObject<T>(string key, T obj, bool global)
        {
            SetCachedObject(key,
                obj,
                _redisCacheSettings.DefaultCacheDuration,
                false);
        }

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        public void SetCachedObject<T>(string key, T obj, long storageDuration)
        {
            SetCachedObject(key, obj, storageDuration, false);
        }

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public void SetCachedObject<T>(string key, T obj, long storageDuration, bool global)
        {
            key = GetFullyQualifiedKey(key, global);

            try
            {
                var db = GetDatabase();

                db.StringSet(
                    key,
                    _objectConverter.ToString(obj),
                    TimeSpan.FromSeconds(storageDuration),
                    flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                throw;
            }
        }

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedObjectAsync<T>(string key, T obj)
        {
            return SetCachedObjectAsync(key, obj, false);
        }

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedObjectAsync<T>(string key, T obj, bool global)
        {
            return SetCachedObjectAsync(key,
                obj,
                _redisCacheSettings.DefaultCacheDuration,
                false);
        }

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedObjectAsync<T>(string key, T obj, long storageDuration, bool global)
        {
            key = GetFullyQualifiedKey(key, global);

            try
            {
                var db = GetDatabase();

                return db.StringSetAsync(
                    key,
                    _objectConverter.ToString(obj),
                    TimeSpan.FromSeconds(storageDuration),
                    flags: CommandFlags.FireAndForget);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                throw;
            }
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
        /// <param name="projectName">specific project system name</param>
        /// <returns>Inter project independent cache key</returns>
        private string GetFullyQualifiedKey(string key,
            bool global,
            string projectName = null)
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
                return $"{(string.IsNullOrEmpty(projectName) ? _projectName : projectName)}.{key}";
            }
        }

        /// <summary>
        /// Get Fully qualified partial key based on global and project name
        /// </summary>
        /// <param name="keyPart">Key part</param>
        /// <param name="global">Create Global Key</param>
        /// <param name="projectName">Project name</param>
        /// <returns></returns>
        private string GetFullyQualifiedPartialKey(string keyPart,
            bool global,
            string projectName = null)
        {
            if (string.IsNullOrEmpty(keyPart))
            {
                throw new ArgumentNullException(nameof(keyPart));
            }
            if (global)
            {
                return $"{GetGlobalKey()}.*{keyPart}*";
            }
            else
            {
                return $"{(string.IsNullOrEmpty(projectName) ? _projectName : projectName)}.*{keyPart}*";
            }
        }


        /// <summary>
        /// Base method for getting object from cache 
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds, nullable</param>
        /// <param name="global">This flag indicates if object should be set for Global / Project specific cache</param>
        /// <returns>Object from cache</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This warning is suppressed as we need to return result no matter of Redis GET / SET operation result")]
        private T GetOrSetBase<T>(string key, Func<T> objectReceiver, long? storageDuration, bool global)
        {
            T val = default;
            bool isSet = false;
            try
            {
                val = GetCachedObject<T>(key, global);
                isSet = true;
            }
            catch (CacheMissException ex)
            {
                _logger.LogWarning(ex, LogMessageResource.RedisObjectNotFound, key);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisGetObjectRedisConnectionError, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisGetObjectError, key);
            }


            if (!isSet)
            {
                val = objectReceiver();

                try
                {
                    if (storageDuration.HasValue)
                    {
                        SetCachedObject(key, val, storageDuration.Value, global);
                    }
                    else
                    {
                        SetCachedObject(key, val, global);
                    }
                }
                catch (RedisConnectionException ex)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectRedisConnectionError, key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                }
            }

            return val;
        }

        /// <summary>
        /// Base method for getting object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds, nullable</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        private Task<T> GetOrSetAsyncBase<T>(string key, Func<T> objectReceiver, long? storageDuration, bool global)
        {
            return GetOrSetAsyncBase(key, async () =>
            {
                return await Task.FromResult(objectReceiver())
                .ConfigureAwait(false);
            }, storageDuration, global);
        }


        /// <summary>
        /// Base method for getting object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds, nullable</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This warning is suppressed as we need to return result no matter of Redis GET / SET operation result")]
        private async Task<T> GetOrSetAsyncBase<T>(string key,
            Func<Task<T>> objectReceiver,
            long? storageDuration,
            bool global)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            T val = default;
            bool isSet = false;
            try
            {
                val = await GetCachedObjectAsync<T>(key, global)
                    .ConfigureAwait(false);
                isSet = true;
            }
            catch (CacheMissException ex)
            {
                _logger.LogWarning(ex, LogMessageResource.RedisObjectNotFound, key);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisGetObjectRedisConnectionError, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessageResource.RedisGetObjectError, key);
            }


            if (!isSet)
            {
                val = await objectReceiver()
                    .ConfigureAwait(false);

                try
                {
                    if (storageDuration.HasValue)
                    {
                        await SetCachedObjectAsync(key, val, storageDuration.Value, global)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await SetCachedObjectAsync(key, val, global)
                            .ConfigureAwait(false);
                    }
                }
                catch(RedisConnectionException ex)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectRedisConnectionError, key);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                }
            }


            return val;
        }

        /// <summary>
        /// Get all keys for specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="projectName">project system name</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>list of keys</returns>
        private RedisKey[] GetKeys(string partOfKey, string projectName, bool global)
        {
            var keyPattern = GetFullyQualifiedPartialKey(partOfKey, global, projectName);
            var connection = _connectionProvider.GetConnection();
            var endPoint = connection.GetEndPoints().First();
            var keyArr = connection.GetServer(endPoint).Keys(pattern: $"{keyPattern}").ToArray();
            return keyArr;
        }
        #endregion
    }
}
