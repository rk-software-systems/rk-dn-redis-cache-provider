using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
    public partial class CacheService
    {
        #region methods

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        public void SetCachedObject<T>(string key, T objectToCache)
        {
            SetCachedObject(key, objectToCache, false);
        }

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public void SetCachedObject<T>(string key, T objectToCache, bool useGlobalCache)
        {
            SetCachedObject(key,
                objectToCache,
                _redisCacheSettings.DefaultCacheDuration,
                useGlobalCache);
        }

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        public void SetCachedObject<T>(string key, T objectToCache, long storageDuration)
        {
            SetCachedObject(key, objectToCache, storageDuration, false);
        }

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public void SetCachedObject<T>(string key, T objectToCache, long storageDuration, bool useGlobalCache)
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            try
            {
                var db = GetDatabase();
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogInformation("Setting object from redis. Key: {key}", key);
                }
                db.StringSet(
                    key,
                    _objectConverter.ToString(objectToCache),
                    TimeSpan.FromSeconds(storageDuration),
                    flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                }
                throw;
            }
        }

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedObjectAsync<T>(string key, T objectToCache)
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
        public Task SetCachedObjectAsync<T>(string key, T objectToCache, bool useGlobalCache)
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
        public Task SetCachedObjectAsync<T>(string key, T obj, long storageDuration, bool useGlobalCache)
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            try
            {
                var db = GetDatabase();
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogInformation("Setting object from redis. Key: {key}", key);
                }

                return db.StringSetAsync(
                    key,
                    _objectConverter.ToString(obj),
                    TimeSpan.FromSeconds(storageDuration),
                    flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisSetObjectError, key);
                }
                throw;
            }
        }
        #endregion
    }
}
