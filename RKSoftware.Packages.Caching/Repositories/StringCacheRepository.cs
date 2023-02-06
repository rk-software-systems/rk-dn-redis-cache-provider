using System;
using System.Threading.Tasks;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.ErrorHandling;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Repositories
{
    /// <summary>
    /// This service is used to set object to cache storage and to get object from cache storage by using string type for transferring
    /// </summary>
    public class StringCacheRepository : ICacheRepository
    {
        #region fields  

        private readonly IConnectionProvider _connectionProvider;
        private readonly IObjectToTextConverter _objectConverter;
        #endregion

        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="StringCacheRepository"/> class
        /// </summary>
        /// <param name="connectionProvider"><see cref="IConnectionProvider"/></param>
        /// <param name="objectConverter"><see cref="IObjectToTextConverter"/></param>
        public StringCacheRepository(
            IConnectionProvider connectionProvider,
            IObjectToTextConverter objectConverter)
        {
            _connectionProvider = connectionProvider;
            _objectConverter = objectConverter;
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
        public async Task<T> GetObjectAsync<T>(IDatabase db, string key)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            var result = await db.StringGetAsync(key, _connectionProvider.ReadFlags);

            if (string.IsNullOrEmpty(result))
            {
                throw new CacheMissException();
            }

            return _objectConverter.FromString<T>(result);
        }

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

            var result = db.StringGet(key, _connectionProvider.ReadFlags);

            if (string.IsNullOrEmpty(result))
            {
                throw new CacheMissException();
            }

            return _objectConverter.FromString<T>(result);
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

            var value = _objectConverter.ToString(objectToCache);
            db.StringSet(
                    key,
                    value,
                    TimeSpan.FromSeconds(storageDuration),
                    flags: _connectionProvider.WriteFlags);
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

            var value = _objectConverter.ToString(objectToCache);
            await db.StringSetAsync(
                    key,
                    value,
                    TimeSpan.FromSeconds(storageDuration),
                    flags: _connectionProvider.WriteFlags);
        }
        #endregion
    }
}
