using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="storageDuration">Time span to keep value in cache, in seconds. Has to be greater than zero.</param>
        /// <returns>Object from cache. Null value if not found or if objectReceiver returns null.</returns>
        Task<T?> GetOrSetCachedObjectAsync<T>(string key, Func<Task<T?>> objectReceiver, long storageDuration) where T : class;

        /// <summary>
        /// Get object from cache asynchronously using asynchronous obtainer
        /// In case object not found in cache, obtain its value and set it to cache
        /// </summary>
        /// <typeparam name="T">Resulting object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="objectReceiver">Async Delegate that allows us to obtain object to be cached</param>
        /// <param name="storageDuration">Time span to keep value in cache, in seconds. Has to be greater than zero.</param>
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
        /// <param name="storageDuration">Time span to keep value in cache, in seconds. Has to be greater than zero.</param>
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
        /// Bulk reset entry in cache.
        /// On a Redis Cluster the removals are batched per hash slot so that no single DEL spans
        /// slots, everywhere else the keys are removed with one DEL.
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        Task ResetBulkAsync(IEnumerable<string> keys, bool useGlobalCache);

        /// <summary>
        /// Reset items which have a specific part of key.
        /// On a Redis Cluster the keys are enumerated on every connected master so that all shards
        /// are covered, and the removals are batched per hash slot so that no single DEL spans slots.
        /// On a standalone or Sentinel managed group one node holds the whole keyspace and the
        /// matching keys are removed with a single DEL.
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        Task ResetBulkAsync(string partOfKey);

        /// <summary>
        /// Reset items which have a specific part of key.
        /// On a Redis Cluster the keys are enumerated on every connected master so that all shards
        /// are covered, and the removals are batched per hash slot so that no single DEL spans slots.
        /// On a standalone or Sentinel managed group one node holds the whole keyspace and the
        /// matching keys are removed with a single DEL.
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        Task ResetBulkAsync(string partOfKey, bool globalCache);

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Value is serialized. Hash time to live is refreshed.
        /// </summary>
        /// <typeparam name="T">Type of the field value to be set</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldAsync<T>(string key, string field, T value) where T : class;

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Value is serialized. Hash time to live is refreshed.
        /// </summary>
        /// <typeparam name="T">Type of the field value to be set</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldAsync<T>(string key, string field, T value, bool useGlobalCache) where T : class;

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Value is serialized. Hash time to live is refreshed.
        /// </summary>
        /// <typeparam name="T">Type of the field value to be set</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache, in seconds. Has to be greater than zero.</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldAsync<T>(string key, string field, T value, long storageDuration, bool useGlobalCache) where T : class;

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Bytes are stored as is, with no conversion. Hash time to live is refreshed.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldAsync(string key, string field, byte[] value);

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Bytes are stored as is, with no conversion. Hash time to live is refreshed.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldAsync(string key, string field, byte[] value, bool useGlobalCache);

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Bytes are stored as is, with no conversion. Hash time to live is refreshed.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache, in seconds. Has to be greater than zero.</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldAsync(string key, string field, byte[] value, long storageDuration, bool useGlobalCache);

        /// <summary>
        /// Set multiple hash fields value in cache asynchronously (HSET). Bytes are stored as is, with no conversion. Hash time to live is refreshed.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldsAsync(string key, params (string Field, byte[] Value)[] fieldValues);

        /// <summary>
        /// Set multiple hash fields value in cache asynchronously (HSET). Bytes are stored as is, with no conversion. Hash time to live is refreshed.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldsAsync(string key, (string Field, byte[] Value)[] fieldValues, bool useGlobalCache);

        /// <summary>
        /// Set multiple hash fields value in cache asynchronously (HSET). Bytes are stored as is, with no conversion. Hash time to live is refreshed.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache, in seconds. Has to be greater than zero.</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task SetCachedHashFieldsAsync(string key, (string Field, byte[] Value)[] fieldValues, long storageDuration, bool useGlobalCache);

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Value is deserialized.
        /// </summary>
        /// <typeparam name="T">Field value type</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <returns>Field value. Null value if not found.</returns>
        Task<T?> GetCachedHashFieldAsync<T>(string key, string field) where T : class;

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Value is deserialized.
        /// </summary>
        /// <typeparam name="T">Field value type</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Field value. Null value if not found.</returns>
        Task<T?> GetCachedHashFieldAsync<T>(string key, string field, bool useGlobalCache) where T : class;

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Bytes are returned as stored, with no conversion.
        /// Intended for values written with the byte array setters, a serialized value is returned in its stored form.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <returns>Field value. Null value if not found. Caller owns the stream and has to dispose it.</returns>
        Task<Stream?> GetCachedHashFieldStreamAsync(string key, string field);

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Bytes are returned as stored, with no conversion.
        /// Intended for values written with the byte array setters, a serialized value is returned in its stored form.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Field value. Null value if not found. Caller owns the stream and has to dispose it.</returns>
        Task<Stream?> GetCachedHashFieldStreamAsync(string key, string field, bool useGlobalCache);

        /// <summary>
        /// Remove fields from hash in cache asynchronously (HDEL)
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fields">Hash fields to be removed</param>
        /// <returns>Task awaiter</returns>
        Task ResetHashFieldsAsync(string key, IEnumerable<string> fields);

        /// <summary>
        /// Remove fields from hash in cache asynchronously (HDEL)
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fields">Hash fields to be removed</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        Task ResetHashFieldsAsync(string key, IEnumerable<string> fields, bool useGlobalCache);
    }
}
