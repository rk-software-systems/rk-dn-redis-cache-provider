using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
    public partial class CacheService
    {
        #region methods

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        public Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver) where T : class
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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        public Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, bool useGlobalCache) where T : class
        {
            ArgumentNullException.ThrowIfNull(objectReceiver);

            return GetOrSetAsyncBase(key, objectReceiver, null, useGlobalCache);
        }

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        public Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, long storageDuration) where T : class
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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        public Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, long storageDuration, bool useGlobalCache) where T : class
        {
            ArgumentNullException.ThrowIfNull(objectReceiver);

            return GetOrSetAsyncBase(key, objectReceiver, storageDuration, useGlobalCache);
        }
        #endregion

        #region helpers

        /// <summary>
        /// Base method for getting object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds, nullable</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This warning is suppressed as we need to return result no matter of Redis GET / SET operation result")]
        private async Task<T?> GetOrSetAsyncBase<T>(string key,
            Func<Task<T?>> objectReceiver,
            long? storageDuration,
            bool global) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            ArgumentNullException.ThrowIfNull(objectReceiver);

            bool isSet = false;
            T? val = null;

            try
            {
                val = await GetCachedObjectAsync<T>(key, global);

                if (_redisCacheSettings.UseLogging && val == null)
                {
                    _logRedisObjectNotFoundWarning(_logger, key, null);
                }
            }catch(Exception ex)
            {
                if (_redisCacheSettings.    UseLogging)
                {
                    _logRedisGetObjectError(_logger, key, ex);
                }
            }
            

            isSet = val != null;

            if (!isSet)
            {
                val = await objectReceiver();

                if (val != null)
                {
                    if (storageDuration.HasValue)
                    {
                        await SetCachedObjectAsync(key, val, storageDuration.Value, global);
                    }
                    else
                    {
                        await SetCachedObjectAsync(key, val, global);
                    }
                }
            }

            return val;
        }
        #endregion        
    }
}
