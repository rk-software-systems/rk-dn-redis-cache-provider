using System;
using System.IO;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Repositories
{
    /// <summary>
    /// Hash commands shared by the cache repositories, operating on values that are already converted to their stored form
    /// </summary>
    internal static class HashStorage
    {
        /// <summary>
        /// Set hash fields and refresh the hash time to live in one MULTI / EXEC transaction,
        /// so that the hash can never be left in storage without an expiration
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="entries">Hash fields with values to be stored</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds</param>
        /// <param name="flags">Write command flags</param>
        /// <returns></returns>
        internal static Task SetFieldsAsync(IDatabase db, string key, HashEntry[] entries, long storageDuration, CommandFlags flags)
        {
            var transaction = db.CreateTransaction();
            var setTask = transaction.HashSetAsync(key, entries, flags);
            var expireTask = transaction.KeyExpireAsync(key, TimeSpan.FromSeconds(storageDuration), flags: flags);

            // queued command tasks complete once the transaction is executed
            return Task.WhenAll(transaction.ExecuteAsync(flags), setTask, expireTask);
        }

        /// <summary>
        /// Get a hash field as it is stored
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="field">Hash field name</param>
        /// <param name="flags">Read command flags</param>
        /// <returns>Stream containing the field value, null if not found. Disposing the stream releases the underlying lease.</returns>
        internal static async Task<Stream?> GetFieldStreamAsync(IDatabase db, string key, string field, CommandFlags flags)
        {
            var bytesValue = await db.HashGetLeaseAsync(key, field, flags);

            return bytesValue?.AsStream();
        }

        /// <summary>
        /// Validate arguments shared by every hash set operation
        /// </summary>
        /// <param name="db">Cache storage handler</param>
        /// <param name="key">Cache storage key</param>
        /// <param name="storageDuration">Time span to keep hash in cache storage, in seconds</param>
        internal static void ValidateSet(IDatabase db, string key, long storageDuration)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storageDuration);
        }

        /// <summary>
        /// Convert field / value pairs to hash entries, validating every pair
        /// </summary>
        /// <param name="fieldValues">Hash fields and corresponding byte array values</param>
        /// <returns>Hash entries to be stored</returns>
        internal static HashEntry[] ToEntries((string Field, byte[] Value)[] fieldValues)
        {
            if (fieldValues == null || fieldValues.Length == 0)
            {
                throw new ArgumentException("Field values cannot be null or empty", nameof(fieldValues));
            }

            return Array.ConvertAll(fieldValues, fieldValue =>
            {
                ArgumentException.ThrowIfNullOrEmpty(fieldValue.Field, nameof(fieldValues));
                ArgumentNullException.ThrowIfNull(fieldValue.Value, nameof(fieldValues));

                return new HashEntry(fieldValue.Field, fieldValue.Value);
            });
        }
    }
}
