using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
    public partial class CacheService
    {
        #region methods

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        public void Reset(string key)
        {
            Reset(key, false);
        }

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public void Reset(string key, bool useGlobalCache)
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            try
            {
                var db = GetDatabase();

                db.KeyDelete(key, flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisConnectionError, key);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisRemoveObjectError, key);
                }
                throw;
            }
        }

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <returns>Task awaiter</returns>
        public Task ResetAsync(string key)
        {
            return ResetAsync(key, false);
        }

        /// <summary>
        /// Reset entry in cache
        /// </summary>
        /// <param name="key">Cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        /// <returns>Task awaiter</returns>
        public Task ResetAsync(string key, bool useGlobalCache)
        {
            key = GetFullyQualifiedKey(key, useGlobalCache);

            try
            {
                var db = GetDatabase();

                return db.KeyDeleteAsync(key, flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisRemoveObjectError, key);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisRemoveObjectError, key);
                }
                throw;
            }
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">list of cache storage key</param>
        public void ResetBulk(IEnumerable<string> keys)
        {
            ResetBulk(keys, false);
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">list of cache storage key</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public void ResetBulk(IEnumerable<string> keys, bool useGlobalCache)
        {
            var keyArr = new List<RedisKey>();
            if ((keys?.Count()).GetValueOrDefault() == 0)
            {
                // no keys to delete, exit
                return;
            }

            foreach (var key in keys)
            {
                keyArr.Add(GetFullyQualifiedKey(key, useGlobalCache));
            }

            try
            {
                var db = GetDatabase();
                db.KeyDelete(keyArr.ToArray(), flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisBulkResetError, keyArr);
                }
                throw;
            }
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        public Task ResetBulkAsync(IEnumerable<string> keys)
        {
            return ResetBulkAsync(keys, false);
        }

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public Task ResetBulkAsync(IEnumerable<string> keys, bool useGlobalCache)
        {
            if ((keys?.Count()).GetValueOrDefault() == 0)
            {
                throw new ArgumentException(LogMessageResource.RedisBulkResetNoKeys, nameof(keys));
            }

            var keyArr = new List<RedisKey>();
            foreach (var key in keys)
            {
                keyArr.Add(GetFullyQualifiedKey(key, useGlobalCache));
            }

            try
            {
                var db = GetDatabase();
                return db.KeyDeleteAsync(keyArr.ToArray(), flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisBulkResetError, keyArr);
                }
                throw;
            }
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        public void ResetBulk(string partOfKey)
        {
            ResetBulk(partOfKey, false);
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, 
        /// for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        public void ResetBulk(string partOfKey, bool globalCache)
        {
            try
            {
                var keyArr = GetKeys(partOfKey, globalCache);
                var db = GetDatabase();
                db.KeyDelete(keyArr, flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisBulkPartialReset, partOfKey);
                }
                throw;
            }
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        public Task ResetBulkAsync(string partOfKey)
        {
            return ResetBulkAsync(partOfKey, false);
        }

        /// <summary>
        /// Reset items which have a specific part of key
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        public Task ResetBulkAsync(string partOfKey, bool globalCache)
        {
            try
            {
                var keyArr = GetKeys(partOfKey, globalCache);
                var db = GetDatabase();
                return db.KeyDeleteAsync(keyArr, flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logger.LogError(ex, LogMessageResource.RedisBulkPartialReset, partOfKey);
                }
                throw;
            }
        }
        #endregion
    }
}
