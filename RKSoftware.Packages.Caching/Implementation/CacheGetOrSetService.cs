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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, bool useGlobalCache)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetBase(key, objectReceiver, null, useGlobalCache);
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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, long storageDuration, bool useGlobalCache)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetBase(key, objectReceiver, storageDuration, useGlobalCache);
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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, bool useGlobalCache)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

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
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        public Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, long storageDuration, bool useGlobalCache)
        {
            if (objectReceiver == null)
            {
                throw new ArgumentNullException(nameof(objectReceiver));
            }

            return GetOrSetAsyncBase(key, objectReceiver, storageDuration, useGlobalCache);
        }
        #endregion

        #region helpers

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
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisObjectNotFoundWarning(_logger, key, ex);
                }
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisGetObjectConnectionError(_logger, key, ex);
                }
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisSetObjectError(_logger, key, ex);
                }
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
                    if (_redisCacheSettings.UseLogging)
                    {
                        _logRedisGetObjectConnectionError(_logger, key, ex);
                    }
                }
                catch (Exception ex)
                {
                    if (_redisCacheSettings.UseLogging)
                    {
                        _logRedisSetObjectError(_logger, key, ex);
                    }
                }
            }

            return val;
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
                val = await GetCachedObjectAsync<T>(key, global);
                isSet = true;
            }
            catch (CacheMissException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisObjectNotFoundWarning(_logger, key, ex);
                }
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisGetObjectConnectionError(_logger, key, ex);
                }
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisSetObjectError(_logger, key, ex);
                }
            }


            if (!isSet)
            {
                val = await objectReceiver();

                try
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
                catch (RedisConnectionException ex)
                {
                    if (_redisCacheSettings.UseLogging)
                    {
                        _logRedisGetObjectConnectionError(_logger, key, ex);
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    if (_redisCacheSettings.UseLogging)
                    {
                        _logRedisSetObjectError(_logger, key, ex);
                    }
                }
            }


            return val;
        }
        #endregion        
    }
}
