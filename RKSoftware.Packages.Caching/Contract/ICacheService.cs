using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RKSoftware.Packages.Caching.Contract
{
    /// <summary>
    /// This service is used to get / set object to Cache
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <returns>Object from cache. Null value if not found</returns>
        Task<T?> GetCachedObjectAsync<T>(string key) where T : class;

        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null value if not found or if objectReceiver returns null.</returns>
        Task<T?> GetCachedObjectAsync<T>(string key, bool useGlobalCache) where T : class;

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache. Null value if not found or if objectReceiver returns null.</returns>
        Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver) where T : class;

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null value if not found or if objectReceiver returns null.</returns>
        Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, bool useGlobalCache) where T : class;

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache. Null value if not found or if objectReceiver returns null.</returns>
        Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, long storageDuration) where T : class;

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache. Null value if not found or if objectReceiver returns null.</returns>
        Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, long storageDuration, bool useGlobalCache) where T : class;

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedObjectAsync<T>(string key, T objectToCache) where T : class;

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedObjectAsync<T>(string key, T objectToCache, bool useGlobalCache) where T : class;

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="obj">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedObjectAsync<T>(string key, T obj, long storageDuration, bool useGlobalCache) where T : class;

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <returns>Task awaiter</returns>
        Task ResetAsync(string key);

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task ResetAsync(string key, bool useGlobalCache);

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage key</param>
        Task ResetBulkAsync(IEnumerable<string> keys);

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        Task ResetBulkAsync(IEnumerable<string> keys, bool useGlobalCache);

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        Task ResetBulkAsync(string partOfKey);

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        Task ResetBulkAsync(string partOfKey, bool globalCache);
    }
}
