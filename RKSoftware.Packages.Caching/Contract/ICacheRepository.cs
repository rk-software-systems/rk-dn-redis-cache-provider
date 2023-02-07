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
        Task<T> GetObjectAsync<T>(IDatabase db, string key);

        /// <summary>
        /// Get object from cache storage synchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <returns></returns>
        T GetObject<T>(IDatabase db, string key);

        /// <summary>
        /// Set object to cache storage synchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="objectToCache">Object value to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache storage, in seconds</param>
        void SetObject<T>(IDatabase db, string key, T objectToCache, long storageDuration);

        /// <summary>
        /// Set object to cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="objectToCache">Object value to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache storage, in seconds</param>
        /// <returns></returns>
        Task SetObjectAsync<T>(IDatabase db, string key, T objectToCache, long storageDuration);
    }
}
