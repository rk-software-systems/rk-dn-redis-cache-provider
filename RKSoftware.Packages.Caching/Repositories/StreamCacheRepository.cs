using System;
using System.Threading.Tasks;
using RKSoftware.Packages.Caching.Contract;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Repositories
{
    /// <summary>
    /// This service is used to set object to cache storage and to get object from cache storage by using byte stream for transferring
    /// </summary>
    public class StreamCacheRepository : ICacheRepository
    {
        #region fields  

        private readonly IConnectionProvider _connectionProvider;
        private readonly IObjectToStreamConverter _objectToStreamConverter;
        #endregion

        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCacheRepository"/> class
        /// </summary>
        /// <param name="connectionProvider"><see cref="IConnectionProvider"/></param>
        /// <param name="objectToStreamConverter"><see cref="IObjectToStreamConverter"/></param>
        public StreamCacheRepository(
            IConnectionProvider connectionProvider,
            IObjectToStreamConverter objectToStreamConverter)
        {
            _connectionProvider = connectionProvider;
            _objectToStreamConverter = objectToStreamConverter;
        }
        #endregion

        #region methods


        /// <summary>
        /// Get object from cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <returns></returns>
        public async Task<T?> GetObjectAsync<T>(IDatabase db, string key) where T : class
        {
            ArgumentNullException.ThrowIfNull(db);

            using var bytesValue = await db.StringGetLeaseAsync(key, _connectionProvider.ReadFlags);
            if(bytesValue == null)
            {
                return default;
            }

            using var stream = bytesValue.AsStream();
            if(stream == null)
            {
                return default;
            }

            return await _objectToStreamConverter.FromStreamAsync<T>(stream);
        }


        /// <summary>
        /// Set object to cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="objectToCache">Object value to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache storage, in seconds</param>
        /// <returns></returns>
        public async Task SetObjectAsync<T>(IDatabase db, string key, T objectToCache, long storageDuration) where T : class
        {
            ArgumentNullException.ThrowIfNull(db);

            var bytesValue = _objectToStreamConverter.ToBytes(objectToCache);
            await db.StringSetAsync(key, bytesValue, flags: _connectionProvider.WriteFlags);
        }
        #endregion
    }
}
