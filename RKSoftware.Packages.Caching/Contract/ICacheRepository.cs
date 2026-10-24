using System;
using System.IO;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Contract
{
    /// <summary>
    /// This service is used to set object to cache storage and to get object from cache storage
    /// </summary>
    public interface ICacheRepository
    {
        /// <summary>
        /// Get object from cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <returns></returns>
        Task<T?> GetObjectAsync<T>(IDatabase db, string key) where T : class;

        /// <summary>
        /// Set object to cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="objectToCache">Object value to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache storage, in seconds. Has to be greater than zero.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        Task SetObjectAsync<T>(IDatabase db, string key, T objectToCache, long storageDuration) where T : class;

        /// <summary>
        /// Set single hash field value in cache storage asynchronously (HSET). Hash time to live is refreshed.
        /// </summary>
        /// <typeparam name="T">field value type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds. Has to be greater than zero.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        Task SetHashFieldAsync<T>(IDatabase db, string key, string field, T value, long storageDuration) where T : class;

        /// <summary>
        /// Set single hash field value in cache storage asynchronously (HSET) as a byte array. Hash time to live is refreshed.
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Byte array containing the field value to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds. Has to be greater than zero.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        Task SetHashFieldAsync(IDatabase db, string key, string field, byte[] value, long storageDuration);

        /// <summary>
        /// Set multiple hash fields value in cache storage asynchronously (HSET) as byte arrays. Hash time to live is refreshed.
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds. Has to be greater than zero.</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        Task SetHashFieldsAsync(IDatabase db, string key, long storageDuration, params (string Field, byte[] Value)[] fieldValues);

        /// <summary>
        /// Get single hash field value from cache storage asynchronously (HGET)
        /// </summary>
        /// <typeparam name="T">field value type</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <returns>Field value. Null if not found.</returns>
        Task<T?> GetHashFieldAsync<T>(IDatabase db, string key, string field) where T : class;

        /// <summary>
        /// Get single hash field value from cache storage asynchronously (HGET)
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <returns>Stream containing the field value. Null if not found. Caller owns the stream and has to dispose it.</returns>
        Task<Stream?> GetHashFieldAsync(IDatabase db, string key, string field);
    }
}
