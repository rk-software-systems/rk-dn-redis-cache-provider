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
        /// Entry is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
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
        /// Entry is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        public Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, bool useGlobalCache) where T : class
        {
            return GetOrSetCachedObjectAsync(key,
                objectReceiver,
                _redisCacheSettings.DefaultCacheDuration,
                useGlobalCache);
        }

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds. Has to be greater than zero.</param>
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
        /// <param name="storageDuration">Time span to keep value in cache, in seconds. Has to be greater than zero.</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, long storageDuration, bool useGlobalCache) where T : class
        {
            ArgumentNullException.ThrowIfNull(objectReceiver);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storageDuration);

            return GetOrSetAsyncBase(key, objectReceiver, storageDuration, useGlobalCache);
        }
        #endregion

        #region helpers

        /// <summary>
        /// Base method for getting object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// Cache storage failures are logged and tolerated, the value obtained from objectReceiver
        /// is still returned. Argument errors are not tolerated, they are validated by the public
        /// overloads before this method is reached.
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null if not found and if objectReceiver returns null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "A failed cache read, a value that can no longer be deserialized included, is safely equivalent to a cache miss, so every read failure is logged and the value is obtained from objectReceiver instead")]
        private async Task<T?> GetOrSetAsyncBase<T>(string key,
            Func<Task<T?>> objectReceiver,
            long storageDuration,
            bool global) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            ArgumentNullException.ThrowIfNull(objectReceiver);

            T? val = null;

            try
            {
                val = await GetCachedObjectAsync<T>(key, global);

                if (_redisCacheSettings.UseLogging && val == null)
                {
                    _logRedisObjectNotFoundWarning(_logger, key, null);
                }
            }
            catch (Exception ex)
            {
                // Any failure to read is equivalent to a cache miss, so the value is obtained from
                // objectReceiver instead. Logged unconditionally, UseLogging governs informational
                // logging only and must never hide an error.
                _logRedisGetObjectError(_logger, key, ex);
            }

            if (val == null)
            {
                val = await objectReceiver();

                if (val != null)
                {
                    try
                    {
                        await SetCachedObjectAsync(key, val, storageDuration, global);
                    }
                    catch (RedisException ex)
                    {
                        // get or set should not fail on redis errors, so the error is logged and the
                        // value obtained from objectReceiver is returned. Anything that is not a
                        // storage failure, an unserializable value for instance, is left to surface.
                        _logRedisSetObjectError(_logger, key, ex);
                    }
                    catch (TimeoutException ex)
                    {
                        // RedisTimeoutException derives from TimeoutException, not RedisException
                        _logRedisSetObjectError(_logger, key, ex);
                    }
                }
            }

            return val;
        }
        #endregion
    }
}
