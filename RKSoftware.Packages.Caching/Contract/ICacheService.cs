using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RKSoftware.Packages.Caching.ErrorHandling;

namespace RKSoftware.Packages.Caching.Contract
{
    /// <summary>
    /// This service is used to get / set object to Cache
    /// </summary>F
    public interface ICacheService
    {
        /// <summary>
        /// Get object from cache using cache Key
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        T GetCachedObject<T>(string key);

        /// <summary>
        /// Get object from cache using cache Key
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        T GetCachedObject<T>(string key, bool useGlobalCache);

        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        Task<T> GetCachedObjectAsync<T>(string key);

        /// <summary>
        /// Get object from cache using cache Key asynchronously
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
        /// <returns>Object from cache</returns>
        Task<T> GetCachedObjectAsync<T>(string key, bool useGlobalCache);

        /// <summary>
        /// Get object from cache.
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache</returns>
        T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver);

        /// <summary>
        /// Get object from cache.
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, bool useGlobalCache);

        /// <summary>
        /// Get object from cache.
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache</returns>
        T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, long storageDuration);

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
        T GetOrSetCachedObject<T>(string key, Func<T> objectReceiver, long storageDuration, bool useGlobalCache);

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache</returns>
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver);

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver, bool useGlobalCache);

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache</returns>
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver, long storageDuration);

        /// <summary>
        /// Get object from cache asynchronously
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<T> objectReceiver, long storageDuration, bool useGlobalCache);

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <returns>Object from cache</returns>
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver);

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Object from cache</returns>
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, bool useGlobalCache);

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <returns>Object from cache</returns>
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, long storageDuration);

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
        Task<T> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T>> objectReceiver, long storageDuration, bool useGlobalCache);

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        void SetCachedObject<T>(string key, T objectToCache);

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        void SetCachedObject<T>(string key, T objectToCache, bool useGlobalCache);

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        void SetCachedObject<T>(string key, T objectToCache, long storageDuration);

        /// <summary>
        /// Set object value in cache
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        void SetCachedObject<T>(string key, T objectToCache, long storageDuration, bool useGlobalCache);

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedObjectAsync<T>(string key, T objectToCache);

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedObjectAsync<T>(string key, T objectToCache, bool useGlobalCache);

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <returns>Task awaiter</returns>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        Task SetAsync<T>(string key, T objectToCache, long storageDuration);

        /// <summary>
        /// Set object value in cache asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the object to be set</typeparam>
        /// <param name="key">Object cache storage key</param>
        /// <param name="objectToCache">Object to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetAsync<T>(string key, T objectToCache, long storageDuration, bool useGlobalCache);

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        void Reset(string key);

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        void Reset(string key, bool useGlobalCache);

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
        /// <param name="keys">list of cache storage key</param>
        /// <param name="projectName">Reset keys in terms of specified project name</param>
        void ResetBulk(IEnumerable<string> keys, string projectName = null);

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">list of cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <param name="projectName">specific project system name</param>
        void ResetBulk(IEnumerable<string> keys, bool useGlobalCache, string projectName = null);

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage key</param>
        /// <param name="projectName">This parameter provides Project name to be appended to key</param>
        Task ResetBulkAsync(IEnumerable<string> keys, string projectName = null);

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <param name="projectName">specific project system name</param>
        Task ResetBulkAsync(IEnumerable<string> keys, bool useGlobalCache, string projectName = null);


        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, 
        /// for example "TextResource.en."</param>
        /// <param name="projectName">project system name</param>
        void ResetBulk(string partOfKey, string projectName = null);

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, 
        /// for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        /// <param name="projectName">project system name</param>
        void ResetBulk(string partOfKey, bool globalCache, string projectName = null);

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="projectName">project system name</param>
        Task ResetBulkAsync(string partOfKey, string projectName = null);

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        /// <param name="projectName">project system name</param>
        Task ResetBulkAsync(string partOfKey, bool globalCache, string projectName = null);
    }
}
