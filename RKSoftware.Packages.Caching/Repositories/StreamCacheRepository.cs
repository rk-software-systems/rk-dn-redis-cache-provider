using System;
using System.Threading.Tasks;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.ErrorHandling;
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
        /// Get object from cache storage synchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <returns></returns>
        public T GetObject<T>(IDatabase db, string key)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            var bytesValue = db.StringGetLease(key, _connectionProvider.ReadFlags) ?? throw new CacheMissException();
            var stream = bytesValue.AsStream() ?? throw new CacheMissException();
            return _objectToStreamConverter.FromStream<T>(stream);
        }

        /// <summary>
        /// Get object from cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <returns></returns>
        public async Task<T> GetObjectAsync<T>(IDatabase db, string key)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            var bytesValue = (await db.StringGetLeaseAsync(key, _connectionProvider.ReadFlags)) ?? throw new CacheMissException();
            var stream = bytesValue.AsStream() ?? throw new CacheMissException();
            return await _objectToStreamConverter.FromStreamAsync<T>(stream);
        }

        /// <summary>
        /// Set object to cache storage synchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="objectToCache">Object value to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache storage, in seconds</param>
        public void SetObject<T>(IDatabase db, string key, T objectToCache, long storageDuration)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            var bytesValue = _objectToStreamConverter.ToBytes(objectToCache);
            db.StringSet(key, bytesValue, flags: _connectionProvider.WriteFlags);
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
        public async Task SetObjectAsync<T>(IDatabase db, string key, T objectToCache, long storageDuration)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            var bytesValue = _objectToStreamConverter.ToBytes(objectToCache);
            await db.StringSetAsync(key, bytesValue, flags: _connectionProvider.WriteFlags);
        }
        #endregion
    }
}
