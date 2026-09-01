using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

                return db.KeyDeleteAsync(key, flags: _connectionProvider.RemoveFlags);
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

        /// <summary>
        /// Bulk reset entry in cache
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        public Task ResetBulkAsync(IEnumerable<string> keys)
        {
            return ResetBulkAsync(keys, false);
        }

        /// <summary>
        /// Bulk reset entry in cache.
        /// On a Redis Cluster the removals are batched per hash slot so that no single DEL spans
        /// slots, everywhere else the keys are removed with one DEL.
        /// </summary>
        /// <param name="keys">Cache storage keys</param>
        /// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
        public async Task ResetBulkAsync(IEnumerable<string> keys, bool useGlobalCache)
        {
            if (keys == null)
            {
                // no keys to delete, exit
                return;
            }

            var keysList = keys.ToList();

            if (keysList.Count == 0)
            {
                // no keys to delete, exit
                return;
            }

            var keyArr = new RedisKey[keysList.Count];
            int i = 0;
            foreach (var key in keysList)
            {
                keyArr[i] = GetFullyQualifiedKey(key, useGlobalCache);
                i++;
            }

            try
            {
                var connection = _connectionProvider.GetConnection();
                var db = GetDatabase();

                if (!IsCluster(connection))
                {
                    // Non-clustered Redis, delete all keys at once
                    await db.KeyDeleteAsync(keyArr, flags: _connectionProvider.RemoveFlags);
                    return;
                }
                else
                {
                    // Clustered Redis, delete keys in batches by hash slot
                    var keyGrouped = keyArr.GroupBy(k => connection.GetHashSlot(k));

                    foreach (var group in keyGrouped)
                    {
                        await db.KeyDeleteAsync(group.ToArray(), flags: _connectionProvider.RemoveFlags);
                    }
                }
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    var keyStr = string.Join(", ", keyArr);
                    _logRedisBulkResetConnectionError(_logger, keyStr, ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    var keyStr = string.Join(", ", keyArr);
                    _logRedisBulkResetError(_logger, keyStr, ex);
                }
                throw;
            }
        }

        /// <summary>
        /// Reset items which have a specific part of key.
        /// On a Redis Cluster the keys are enumerated on every connected master so that all shards
        /// are covered, and the removals are batched per hash slot so that no single DEL spans slots.
        /// On a standalone or Sentinel managed group one node holds the whole keyspace and the
        /// matching keys are removed with a single DEL.
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        public Task ResetBulkAsync(string partOfKey)
        {
            return ResetBulkAsync(partOfKey, false);
        }

        /// <summary>
        /// Reset items which have a specific part of key.
        /// On a Redis Cluster the keys are enumerated on every connected master so that all shards
        /// are covered, and the removals are batched per hash slot so that no single DEL spans slots.
        /// On a standalone or Sentinel managed group one node holds the whole keyspace and the
        /// matching keys are removed with a single DEL.
        /// </summary>
        /// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
        /// <param name="globalCache">Reset in global cache</param>
        public async Task ResetBulkAsync(string partOfKey, bool globalCache)
        {
            try
            {
                var keyPattern = GetFullyQualifiedPartialKey(partOfKey, globalCache);
                var connection = _connectionProvider.GetConnection();

                var db = GetDatabase();

                if (IsCluster(connection))
                {
                    var servers = GetKeyScanServers(connection);
                    // perform cluster-aware key deletion by grouping keys by hash slot and deleting them in batches
                    var serverKeys = await Task.WhenAll(servers.Select(server => server.KeysAsync(pattern: keyPattern).ToListAsync().AsTask()));

                    var removedKeys = new HashSet<RedisKey>();
                    var groupedBySlotServerKeys = serverKeys.Select(x =>
                    {
                        return x.GroupBy(k => connection.GetHashSlot(k)).ToList();
                    }).ToList();

                    foreach (var keyArrayGroup in groupedBySlotServerKeys)
                    {
                        foreach (var keysGroup in keyArrayGroup)
                        {
                            var keysToRemove = new List<RedisKey>();

                            foreach (var key in keysGroup)
                            {
                                if (removedKeys.Contains(key))
                                {
                                    // Skip already removed keys
                                    continue;
                                }

                                removedKeys.Add(key);
                                keysToRemove.Add(key);
                            }

                            if (keysToRemove.Count > 0)
                            {
                                await db.KeyDeleteAsync(keysToRemove.ToArray(), flags: _connectionProvider.RemoveFlags);
                            }
                        }
                    }
                }
                else
                {
                    var server = GetKeyScanServers(connection).First();
                    var keyArr = (await server.KeysAsync(pattern: keyPattern).ToListAsync()).ToArray();
                    if(keyArr.Length == 0)
                    {
                        // no keys to delete, exit
                        return;
                    }

                    await db.KeyDeleteAsync(keyArr, flags: _connectionProvider.RemoveFlags);
                }
            }
            catch (RedisConnectionException ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisBulkPartialResetConnectionError(_logger, partOfKey, ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (_redisCacheSettings.UseLogging)
                {
                    _logRedisBulkPartialReset(_logger, partOfKey, ex);
                }
                throw;
            }
        }
        #endregion

        #region  helpers

        /// <summary>
        /// Get the servers that together hold the whole keyspace.
        /// On a Redis Cluster every master owns its own slot range, so all of them have to be scanned.
        /// On a standalone or Sentinel managed replication group any single master holds everything.
        /// </summary>
        /// <param name="connection">Redis database connection multiplexer <see cref="IConnectionMultiplexer"/></param>
        /// <returns>Servers to be scanned for cache keys</returns>
        /// <exception cref="RedisConnectionException">Thrown when no connected master was found</exception>
        private static IEnumerable<IServer> GetKeyScanServers(IConnectionMultiplexer connection)
        {
            var masters = connection.GetServers()
                .Where(server => server.IsConnected
                    && !server.IsReplica
                    && server.ServerType != ServerType.Sentinel)
                .ToArray();

            if (masters.Length == 0)
            {
                throw new RedisConnectionException(
                    ConnectionFailureType.UnableToResolvePhysicalConnection,
                    CommandFlags.None,
                    "No connected Redis master was found to enumerate cache keys.");
            }

            return masters.Any(server => server.ServerType == ServerType.Cluster)
                ? masters
                : masters.Take(1);
        }

        private static bool IsCluster(IConnectionMultiplexer connection) =>
            connection.GetServers().Any(server => server.ServerType == ServerType.Cluster);
        #endregion
    }
}
