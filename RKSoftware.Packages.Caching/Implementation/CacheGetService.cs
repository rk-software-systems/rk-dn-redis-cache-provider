using System;
using System.Threading.Tasks;
using RKSoftware.Packages.Caching.ErrorHandling;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
    public partial class CacheService
    {
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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        public T GetCachedObject<T>(string key, bool useGlobalCache)
        {
            return GetCachedObject<T>(key, useGlobalCache, _cacheRepository.GetObject<T>);
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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <exception cref="ArgumentNullException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        public async Task<T> GetCachedObjectAsync<T>(string key, bool useGlobalCache)
        {
            return await GetCachedObjectAsync<T>(key, useGlobalCache, _cacheRepository.GetObjectAsync<T>);
        }
        #endregion

        #region helpers

        private T GetCachedObject<T>(string key, bool useGlobalCache, Func<IDatabase, string, T> resultExecutor)
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            try
            {
                var db = GetDatabase();
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisGetObjectInformation(_logger, key, null);
                }

                return resultExecutor(db, key);
            }
            catch (RedisConnectionException ex)
            {

                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisConnectionError(_logger, key, ex);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisGetObjectError(_logger, key, ex);
                }

                throw;
            }
        }

        private Task<T> GetCachedObjectAsync<T>(string key, bool useGlobalCache, Func<IDatabase, string,  Task<T>> resultExecutor)
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            try
            {
                var db = GetDatabase();

                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisGetObjectInformation(_logger, key, null);
                }

                return resultExecutor(db, key);

            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisConnectionError(_logger, key, ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisGetObjectError(_logger, key, ex);
                }
                throw;
            }
        }

        #endregion
    }
}
