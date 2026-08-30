using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
    public partial class CacheService
    {
        #region methods

        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <returns>Object from cache</returns>
        public Task<T?> GetCachedObjectAsync<T>(string key) where T : class
        {
            return GetCachedObjectAsync<T>(key, false);
        }

        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache.</returns>
        public async Task<T?> GetCachedObjectAsync<T>(string key, bool useGlobalCache) where T : class
        {
            return await GetCachedObjectAsync<T>(key, useGlobalCache, _cacheRepository.GetObjectAsync<T>);
        }
        #endregion

        #region helpers

        private Task<T?> GetCachedObjectAsync<T>(string key, bool useGlobalCache, Func<IDatabase, string,  Task<T?>> resultExecutor)
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
