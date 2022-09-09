using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Infrastructure;
using StackExchange.Redis;
using System;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;

namespace RKSoftware.Packages.Caching.Implementation
{
	/// <summary>
	/// This service is used to get / set object to Cache
	/// </summary>
	public partial class CacheService : ICacheService
	{
		#region consts

		private const string GLOBAL_CACHE_KEY = "RKSoftware.Global";

		#endregion

		#region fields

		private readonly RedisCacheSettings _redisCacheSettings;
		private readonly string _projectName;
		private readonly ILogger _logger;
		private readonly IConnectionProvider _connectionProvider;
		private readonly IObjectToTextConverter _objectConverter;

		#endregion

		#region ctor

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheService"/> class
		/// </summary>
		/// <param name="redisCacheProvider">Redis connectivity settings</param>
		/// <param name="logger"><see cref="ILogger"/></param>
		/// <param name="connectionProvider">This class is used to obtain Connection Multiplexer <see cref="IConnectionProvider"/></param>
		/// <param name="objectConverter">This service is used to serialize objects from / to strings</param>
		/// <param name="scopedKeyPrefix">This prefix is used as a starting part of Redis DB keys</param>
		public CacheService(IOptions<RedisCacheSettings> redisCacheProvider,
			ILogger<CacheService> logger,
			IConnectionProvider connectionProvider,
			IObjectToTextConverter objectConverter,
			string scopedKeyPrefix)
		{
			if (redisCacheProvider == null)
			{
				throw new ArgumentNullException(nameof(redisCacheProvider));
			}

			if (string.IsNullOrEmpty(scopedKeyPrefix))
			{
				throw new ArgumentNullException(nameof(scopedKeyPrefix));
			}

			_redisCacheSettings = redisCacheProvider.Value;
			_logger = logger;
			_connectionProvider = connectionProvider;
			_objectConverter = objectConverter;
			_projectName = scopedKeyPrefix;
		}

		#endregion

		#region helpers

		/// <summary>
		/// Get Redis Database connection
		/// </summary>
		/// <returns>Redis database connection</returns>
		private IDatabase GetDatabase()
		{
			var redis = _connectionProvider.GetConnection();

			return redis.GetDatabase();
		}

		private CommandFlags GetReadFlags()
		{
			return _connectionProvider.IsSentinel ? CommandFlags.DemandReplica : CommandFlags.None;
		}

		private string GetGlobalKey()
		{
			return string.IsNullOrEmpty(_redisCacheSettings.GlobalCacheKey)
				? GLOBAL_CACHE_KEY
				: _redisCacheSettings.GlobalCacheKey;
		}

		/// <summary>
		/// Get key that were used to store object
		/// </summary>
		/// <param name="key">Key that is used inside project</param>
		/// <param name="global">This flag indicates if key should be project unspecific</param>        
		/// <returns>Inter project independent cache key</returns>
		private string GetFullyQualifiedKey(string key,
			bool global)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (global)
			{
				return $"{GetGlobalKey()}.{key}";
			}

			return $"{_projectName}.{key}";
		}

		/// <summary>
		/// Get Fully qualified partial key based on global and project name
		/// </summary>
		/// <param name="keyPart">Key part</param>
		/// <param name="global">Create Global Key</param>
		/// <returns></returns>
		private string GetFullyQualifiedPartialKey(string keyPart, bool global)
		{
			return GetFullyQualifiedKey($"*{keyPart}*", global);
		}

		/// <summary>
		/// Get all keys for specific part of key
		/// </summary>
		/// <param name="partOfKey">substring of key between project system name and text resource key, for example "TextResource.en."</param>
		/// <param name="global">This flag indicates if cache entry should be set in Global cache (available for all containers)</param>
		/// <returns>list of keys</returns>
		private RedisKey[] GetKeys(string partOfKey, bool global)
		{
			var keyPattern = GetFullyQualifiedPartialKey(partOfKey, global);
			var connection = _connectionProvider.GetConnection();
			var endPoint = connection.GetEndPoints().First();
			var keyArr = connection.GetServer(endPoint).Keys(pattern: $"{keyPattern}").ToArray();
			return keyArr;
		}

		private void SetItem(string key, string value, long storageDuration)
		{
			IDatabase redDb = GetDatabase();

			byte[] byteArray = SerializeToByteArray(value);
			MemoryStream stream = new MemoryStream(byteArray);

			var redisValue = RedisValue.CreateFrom(stream);

			redDb.StringSet(key, redisValue, TimeSpan.FromSeconds(storageDuration),
				flags: CommandFlags.FireAndForget | CommandFlags.DemandMaster);
		}

		private T GetItem<T>(string key, CommandFlags flags)
		{
			IDatabase redDb = GetDatabase();

			Lease<byte> lease = redDb.StringGetLease(key, flags);
			var byteArray = lease.Memory.ToArray();

			if (byteArray == null)
			{
				return default;
			}

			return DeserializeByteArray<T>(byteArray);
		}

		private static byte[] SerializeToByteArray<T>(T obj) where T : class
		{
			if (obj == null)
			{
				return default;
			}

			using var memoryStream = new MemoryStream();

			var serializer = new DataContractSerializer(typeof(T));
			serializer.WriteObject(memoryStream, obj);
			return memoryStream.ToArray();
		}

		private static T DeserializeByteArray<T>(byte[] byteArray)
		{
			if (byteArray == null)
			{
				return default;
			}

			using var memoryStream = new MemoryStream(byteArray);

			var serializer = new DataContractSerializer(typeof(T));
			var obj = (T)serializer.ReadObject(memoryStream);
			return obj;
		}

		#endregion
	}
}