﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Infrastructure;
using StackExchange.Redis;
using System;
using System.Text;
using System.Linq;

namespace RKSoftware.Packages.Caching.Implementation
{
    /// <summary>
    /// Implementation of <see cref="IConnectionProvider"/>
    /// This class is used to manage Redis connections
    /// </summary>
    public class RedisConnectionProvider : IConnectionProvider, IDisposable
    {
        private bool isDisposed;
        private static IConnectionMultiplexer[] _connectionMultiplexers;
        private static readonly object _multiplexerInitLock = new object();
        private readonly RedisCacheSettings _redisCacheSettings;
        private readonly ILogger _logger;

        /// <summary>
        /// This flag indicates if particular connection is Sentinel one
        /// it is used to determine if it is possible to use Redis replica fore read
        /// </summary>
        public bool IsSentinel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnectionProvider"/> class.
        /// </summary>
        /// <param name="redisCacheSettingsAccessor">General Redis cache settings <see cref="RedisCacheSettings"/></param>
        /// <param name="logger">Logger <see cref="ILogger"/></param>
        public RedisConnectionProvider(IOptions<RedisCacheSettings> redisCacheSettingsAccessor,
            ILogger<RedisConnectionProvider> logger)
        {

            if (redisCacheSettingsAccessor == null)
            {
                throw new ArgumentNullException(nameof(redisCacheSettingsAccessor));
            }

            _redisCacheSettings = redisCacheSettingsAccessor.Value;
            _logger = logger;
            IsSentinel = _redisCacheSettings.RedisUrl
                .Contains("serviceName=", StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Get redis database connection multiplexer
        /// </summary>
        /// <returns>Redis database connection multiplexer <see cref="IConnectionMultiplexer"/></returns>
        public IConnectionMultiplexer GetConnection()
        {
            if (_connectionMultiplexers != null)
            {
                return GetConnectionMultiplexer();
            }

            lock (_multiplexerInitLock)
            {
                if (_connectionMultiplexers != null)
                {
                    return GetConnectionMultiplexer();
                }

                var multiplexerCount = _redisCacheSettings.ConnectionMultiplexerPoolSize ?? 1;
                _connectionMultiplexers = new IConnectionMultiplexer[multiplexerCount];

                for (var i = 0; i < multiplexerCount; i++)
                {
                    _connectionMultiplexers[i] = ConnectionMultiplexer.Connect(GetOptionsString(_redisCacheSettings));
                }
                _logger.LogInformation(LogMessageResource.RedisConnectionOpenening);
            }

            return GetConnectionMultiplexer();
        }

        private static IConnectionMultiplexer GetConnectionMultiplexer()
        {
            if (_connectionMultiplexers.Length == 1)
            {
                return _connectionMultiplexers[0];
            }

            return _connectionMultiplexers.OrderBy(x => x.OperationCount).FirstOrDefault();
        }

        private static string GetOptionsString(RedisCacheSettings settings)
        {
            var optionsStringBuilder = new StringBuilder($"{settings.RedisUrl},abortConnect=False,allowAdmin=true");
            if (settings.SyncTimeout.HasValue)
            {
                optionsStringBuilder.Append($",syncTimeout=" + settings.SyncTimeout);
            }

            return optionsStringBuilder.ToString();
        }

        #region IDisposable
        /// <summary>
        /// <see cref="IDisposable"/> Dispose method implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        /// <param name="disposing">THis flag indicates if managed resource are subject o be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var multiplexer in _connectionMultiplexers)
                {
                    multiplexer.Close(true);
                    multiplexer.Dispose();
                }
            }

            isDisposed = true;
        }

        /// <summary>
        /// Finilizer
        /// </summary>
        ~RedisConnectionProvider()
        {
            Dispose(false);
        }
        #endregion
    }
}
