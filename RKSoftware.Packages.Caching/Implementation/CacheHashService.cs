using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
    public partial class CacheService
    {
        #region methods

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Value is serialized.
        /// Hash is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
        /// </summary>
        /// <typeparam name="T">Type of the field value to be set</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedHashFieldAsync<T>(string key, string field, T value) where T : class
        {
            return SetCachedHashFieldAsync(key, field, value, false);
        }

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Value is serialized.
        /// Hash is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
        /// </summary>
        /// <typeparam name="T">Type of the field value to be set</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedHashFieldAsync<T>(string key, string field, T value, bool useGlobalCache) where T : class
        {
            return SetCachedHashFieldAsync(key,
                field,
                value,
                _redisCacheSettings.DefaultCacheDuration,
                useGlobalCache);
        }

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
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public Task SetCachedHashFieldAsync<T>(string key, string field, T value, long storageDuration, bool useGlobalCache) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentException.ThrowIfNullOrEmpty(field);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storageDuration);

            return SetCachedObjectAsync(key, value, storageDuration, useGlobalCache,
                (db, fullKey, val, duration) => _cacheRepository.SetHashFieldAsync(db, fullKey, field, val, duration));
        }

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Bytes are stored as is, with no conversion.
        /// Hash is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedHashFieldAsync(string key, string field, byte[] value)
        {
            return SetCachedHashFieldAsync(key, field, value, false);
        }

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Bytes are stored as is, with no conversion.
        /// Hash is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedHashFieldAsync(string key, string field, byte[] value, bool useGlobalCache)
        {
            return SetCachedHashFieldAsync(key,
                field,
                value,
                _redisCacheSettings.DefaultCacheDuration,
                useGlobalCache);
        }

        /// <summary>
        /// Set single hash field value in cache asynchronously (HSET). Bytes are stored as is, with no conversion.
        /// Hash is kept for the specified storage duration in seconds
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="value">Field value to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache, in seconds. Has to be greater than zero.</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public Task SetCachedHashFieldAsync(string key, string field, byte[] value, long storageDuration, bool useGlobalCache)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentException.ThrowIfNullOrEmpty(field);
            ArgumentNullException.ThrowIfNull(value);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storageDuration);

            return SetCachedObjectAsync(key, value, storageDuration, useGlobalCache,
                (db, fullKey, val, duration) => _cacheRepository.SetHashFieldAsync(db,
                    fullKey,
                    field,
                    val,
                    duration));
        }

        /// <summary>
        /// Set multiple hash fields value in cache asynchronously (HSET). Bytes are stored as is, with no conversion.
        /// Hash is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedHashFieldsAsync(string key, params (string Field, byte[] Value)[] fieldValues)
        {
            return SetCachedHashFieldsAsync(key, fieldValues, false);
        }

        /// <summary>
        /// Set multiple hash fields value in cache asynchronously (HSET). Bytes are stored as is, with no conversion.
        /// Hash is kept for <see cref="Infrastructure.RedisCacheSettings.DefaultCacheDuration"/> seconds
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task SetCachedHashFieldsAsync(string key, (string Field, byte[] Value)[] fieldValues, bool useGlobalCache)
        {
            return SetCachedHashFieldsAsync(key, fieldValues, _redisCacheSettings.DefaultCacheDuration, useGlobalCache);
        }

        /// <summary>
        /// Set multiple hash fields value in cache asynchronously (HSET). Bytes are stored as is, with no conversion.
        /// Hash time to live is refreshed.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fieldValues">Hash fields and corresponding byte array values to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache, in seconds. Has to be greater than zero.</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when storageDuration is not greater than zero</exception>
        public Task SetCachedHashFieldsAsync(string key, (string Field, byte[] Value)[] fieldValues, long storageDuration, bool useGlobalCache)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(fieldValues);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storageDuration);

            return SetCachedObjectAsync(key, fieldValues, storageDuration, useGlobalCache,
                (db, fullKey, val, duration) => _cacheRepository.SetHashFieldsAsync(db, fullKey, duration, val));
        }

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Value is deserialized.
        /// </summary>
        /// <typeparam name="T">Field value type</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <returns>Field value. Null value if not found.</returns>
        public Task<T?> GetCachedHashFieldAsync<T>(string key, string field) where T : class
        {
            return GetCachedHashFieldAsync<T>(key, field, false);
        }

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Value is deserialized.
        /// </summary>
        /// <typeparam name="T">Field value type</typeparam>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Field value. Null value if not found.</returns>
        public Task<T?> GetCachedHashFieldAsync<T>(string key, string field, bool useGlobalCache) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(field);

            return GetCachedObjectAsync<T>(key, useGlobalCache,
                (db, fullKey) => _cacheRepository.GetHashFieldAsync<T>(db, fullKey, field));
        }

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Bytes are returned as stored, with no conversion.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <returns>Field value. Null value if not found.</returns>
        public Task<Stream?> GetCachedHashFieldStreamAsync(string key, string field)
        {
            return GetCachedHashFieldStreamAsync(key, field, false);
        }

        /// <summary>
        /// Get single hash field value from cache asynchronously (HGET). Bytes are returned as stored, with no conversion.
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="field">Hash field</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Field value. Null value if not found.</returns>
        public Task<Stream?> GetCachedHashFieldStreamAsync(string key, string field, bool useGlobalCache)
        {
            ArgumentException.ThrowIfNullOrEmpty(field);

            return GetCachedObjectAsync<Stream>(key, useGlobalCache,
                (db, fullKey) => _cacheRepository.GetHashFieldAsync(db, fullKey, field));
        }

        /// <summary>
        /// Remove fields from hash in cache asynchronously (HDEL)
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fields">Hash fields to be removed</param>
        /// <returns>Task awaiter</returns>
        public Task ResetHashFieldsAsync(string key, IEnumerable<string> fields)
        {
            return ResetHashFieldsAsync(key, fields, false);
        }

        /// <summary>
        /// Remove fields from hash in cache asynchronously (HDEL)
        /// </summary>
        /// <param name="key">Hash cache storage key</param>
        /// <param name="fields">Hash fields to be removed</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task ResetHashFieldsAsync(string key, IEnumerable<string> fields, bool useGlobalCache)
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            if (fields == null)
            {
                // no fields to delete, exit
                return Task.CompletedTask;
            }

            var fieldArr = fields.Select(field =>
            {
                ArgumentException.ThrowIfNullOrEmpty(field, nameof(fields));
                return (RedisValue)field;
            }).ToArray();

            if (fieldArr.Length == 0)
            {
                // no fields to delete, exit
                return Task.CompletedTask;
            }

            try
            {
                var db = GetDatabase();

                return db.HashDeleteAsync(key, fieldArr, _connectionProvider.RemoveFlags);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisResetConnectionError(_logger, key, ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisRemoveObjectError(_logger, key, ex);
                }
                throw;
            }
        }
        #endregion
    }
}
