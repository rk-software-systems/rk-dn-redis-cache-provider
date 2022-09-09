using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RKSoftware.Packages.Caching.ErrorHandling;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Implementation
{
	public partial class CacheService
	{
		#region methods

		/// <summary>
		/// Get object from cache using cache Key
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="key">Cache storage key</param>
		/// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
		/// <returns>Object from cache</returns>
		public T GetCachedObject<T>(string key)
		{
			return GetCachedObject<T>(key, false);
		}

		/// <summary>
		/// Get object from cache using cache Key
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="key">Cache storage key</param>
		/// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
		/// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
		/// <returns>Object from cache</returns>
		public T GetCachedObject<T>(string key, bool useGlobalCache)
		{
			key = GetFullyQualifiedKey(key, useGlobalCache);

			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			IDatabase db;
			string result;

			try
			{
				db = GetDatabase();

				if (_redisCacheSettings.UseLogging)
				{
					_logger.LogInformation("Getting object from redis. Key: {key}", key);
				}

				result = db.StringGet(key, GetReadFlags());
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
					_logger.LogError(ex, LogMessageResource.RedisGetObjectError, key);
				}

				throw;
			}

			if (string.IsNullOrEmpty(result))
			{
				throw new CacheMissException();
			}

			return _objectConverter.FromString<T>(result);
		}


		/// <summary>
		/// Get object as strem from cache using cache Key
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="key">Cache storage key</param>
		/// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
		/// <exception cref="CacheMissException">This exception may appear in case object not found in cache</exception>
		/// <returns>Object from cache</returns>
		public T GetCachedSteramObject<T>(string key, bool useGlobalCache)
		{
			key = GetFullyQualifiedKey(key, useGlobalCache);

			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			IDatabase db;
			T result;

			try
			{
				db = GetDatabase();

				if (_redisCacheSettings.UseLogging)
				{
					_logger.LogInformation("Getting object from redis. Key: {key}", key);
				}

				result = GetItem<T>(key, GetReadFlags());
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
					_logger.LogError(ex, LogMessageResource.RedisGetObjectError, key);
				}

				throw;
			}

			if (result == null)
			{
				throw new CacheMissException();
			}

			return result;
		}


		/// <summary>
		/// Get object from cache using cache Key asynchronously
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="key">Cache storage key</param>
		/// <exception cref="ArgumentNullException">This exception may appear in case object not found in cache</exception>
		/// <returns>Object from cache</returns>
		public Task<T> GetCachedObjectAsync<T>(string key)
		{
			return GetCachedObjectAsync<T>(key, false);
		}

		/// <summary>
		/// Get object from cache using cache Key asynchronously
		/// </summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="key">Cache storage key</param>
		/// <param name="useGlobalCache">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
		/// <exception cref="ArgumentNullException">This exception may appear in case object not found in cache</exception>
		/// <returns>Object from cache</returns>
		public async Task<T> GetCachedObjectAsync<T>(string key, bool useGlobalCache)
		{
			key = GetFullyQualifiedKey(key, useGlobalCache);

			if (string.IsNullOrEmpty(key))
			{
				throw new CacheMissException();
			}

			IDatabase db;
			string result;

			try
			{
				db = GetDatabase();

				if (_redisCacheSettings.UseLogging)
				{
					_logger.LogInformation("Getting object from redis. Key: {key}", key);
				}

				result = await db.StringGetAsync(key, GetReadFlags())
				                 .ConfigureAwait(false);
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
					_logger.LogError(ex, LogMessageResource.RedisGetObjectError, key);
				}

				throw;
			}

			if (string.IsNullOrEmpty(result))
			{
				throw new CacheMissException();
			}

			return _objectConverter.FromString<T>(result);
		}

		#endregion
	}
}