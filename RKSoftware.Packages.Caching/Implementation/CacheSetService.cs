using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
    public partial class CacheService
    {
        #region methods

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedObjectAsync<T>(string key, T objectToCache) where T : class
        {
            return SetCachedObjectAsync(key, objectToCache, false);
        }

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedObjectAsync<T>(string key, T objectToCache, bool useGlobalCache) where T : class
        {
            return SetCachedObjectAsync(key,
                objectToCache,
                _redisCacheSettings.DefaultCacheDuration,
                useGlobalCache);
        }

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedObjectAsync<T>(string key, T obj, long storageDuration, bool useGlobalCache) where T : class
        {
            return SetCachedObjectAsync(key, obj, storageDuration, useGlobalCache, _cacheRepository.SetObjectAsync);
        }
        #endregion

        #region helpers

        private Task SetCachedObjectAsync<T>(string key, T obj, long storageDuration, bool useGlobalCache, Func<IDatabase, string, T, long, Task> resultExecutor) where T : class
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            try
            {
                var db = GetDatabase();
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisSetObjectInformation(_logger, key, null);
                }

                return resultExecutor(db, key, obj, storageDuration);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisSetObjectConnectionError(_logger, key, ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisSetObjectError(_logger, key, ex);
                }
                throw;
            }
        }
        #endregion
    }
}
