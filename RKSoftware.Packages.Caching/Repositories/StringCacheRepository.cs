using System;
using System.IO;
using System.Threading.Tasks;
using RKSoftware.Packages.Caching.Contract;
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
        /// <exception cref="ArgumentNullException">Thrown when db is null</exception>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        public async Task<T?> GetObjectAsync<T>(IDatabase db, string key) where T : class
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var result = await db.StringGetAsync(key, _connectionProvider.ReadFlags);

            if (string.IsNullOrEmpty(result))
            {
                return default;
            }

            return _objectConverter.FromString<T>(result!);
        }

        /// <summary>
        /// Set object to cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">object type to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="objectToCache">Object value to be stored</param>
        /// <param name="storageDuration">Time span to keep value in cache storage, in seconds. Has to be greater than zero.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when db is null</exception>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public async Task SetObjectAsync<T>(IDatabase db, string key, T objectToCache, long storageDuration) where T : class
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storageDuration);

            var value = _objectConverter.ToString(objectToCache);
            await db.StringSetAsync(
                    key,
                    value,
                    TimeSpan.FromSeconds(storageDuration),
                    keepTtl: false,
                    flags: _connectionProvider.WriteFlags);
        }

        /// <summary>
        /// Set a hash field in the cache storage asynchronously. Value is converted to string.
        /// </summary>
        /// <typeparam name="T">Type of the value to be stored</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field name</param>
        /// <param name="value">Value to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds. Has to be greater than zero.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when key or field is null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public Task SetHashFieldAsync<T>(IDatabase db, string key, string field, T value, long storageDuration) where T : class
        {
            HashStorage.ValidateSet(db, key, storageDuration);
            ArgumentException.ThrowIfNullOrEmpty(field);

            var entries = new[] { new HashEntry(field, _objectConverter.ToString(value)) };
            return HashStorage.SetFieldsAsync(db, key, entries, storageDuration, _connectionProvider.WriteFlags);
        }

        /// <summary>
        /// Set a hash field in the cache storage asynchronously. Bytes are stored as is.
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field name</param>
        /// <param name="value">Value to be stored as byte array</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds. Has to be greater than zero.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when key or field is null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public Task SetHashFieldAsync(IDatabase db, string key, string field, byte[] value, long storageDuration)
        {
            return SetHashFieldsAsync(db, key, storageDuration, (field, value));
        }

        /// <summary>
        /// Set multiple hash fields in the cache storage asynchronously. Bytes are stored as is.
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds. Has to be greater than zero.</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when key is null or empty, or field values are missing or invalid</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public Task SetHashFieldsAsync(IDatabase db, string key, long storageDuration, params (string Field, byte[] Value)[] fieldValues)
        {
            HashStorage.ValidateSet(db, key, storageDuration);

            var entries = HashStorage.ToEntries(fieldValues);
            return HashStorage.SetFieldsAsync(db, key, entries, storageDuration, _connectionProvider.WriteFlags);
        }

        /// <summary>
        /// Get a hash field from the cache storage asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the value to be retrieved</typeparam>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field name</param>
        /// <returns>Value retrieved from the cache storage, or null if not found</returns>
        /// <exception cref="ArgumentException">Thrown when key or field is null or empty</exception>
        /// <exception cref="ArgumentNullException">Thrown when db is null</exception>
        public async Task<T?> GetHashFieldAsync<T>(IDatabase db, string key, string field) where T : class
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentException.ThrowIfNullOrEmpty(field);

            var stringValue = await db.HashGetAsync(key, field, flags: _connectionProvider.ReadFlags);
            if (stringValue.IsNullOrEmpty)
            {
                return default;
            }

            return _objectConverter.FromString<T>(stringValue!);
        }

        /// <summary>
        /// Get a hash field from the cache storage asynchronously as a stream
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field name</param>
        /// <returns>Stream containing the value retrieved from the cache storage, or null if not found. Caller owns the stream and has to dispose it.</returns>
        /// <exception cref="ArgumentException">Thrown when key or field is null or empty</exception>
        /// <exception cref="ArgumentNullException">Thrown when db is null</exception>
        public Task<Stream?> GetHashFieldAsync(IDatabase db, string key, string field)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentException.ThrowIfNullOrEmpty(field);

            return HashStorage.GetFieldStreamAsync(db, key, field, _connectionProvider.ReadFlags);
        }
        #endregion
    }
}
