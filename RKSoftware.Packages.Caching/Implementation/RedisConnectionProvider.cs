using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Infrastructure;
using StackExchange.Redis;
using System;

namespace RKSoftware.Packages.Caching.Implementation
{
    /// <summary>
    /// Implementation of <see cref="IConnectionProvider"/>
    /// This class is used to manage Redis connections
    /// </summary>
    public class RedisConnectionProvider : IConnectionProvider, IDisposable
    {
        private bool isDisposed;
        private IConnectionMultiplexer[] _connectionMultiplexers;
        private static object _multiplexerInitLock = new object();
        private readonly RedisCacheSettings _redisCacheSettings;
        private readonly ILogger _logger;

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
                    var options = new ConfigurationOptions()
                    {
                        EndPoints =
                    {
                        _redisCacheSettings.RedisUrl.ToString()
                    },
                        AbortOnConnectFail = false,
                        SyncTimeout = _redisCacheSettings.SyncTimeout ?? 5000
                    };

                    _connectionMultiplexers[i] = ConnectionMultiplexer.Connect(options);
                }
                _logger.LogInformation(LogMessageResource.RedisConnectionOpenening);
            }

            return GetConnectionMultiplexer();
        }

        private IConnectionMultiplexer GetConnectionMultiplexer()
        {
            if (_connectionMultiplexers.Length == 1)
            {
                return _connectionMultiplexers[0];
            }

            var rnd = new Random();
            return _connectionMultiplexers[rnd.Next(0, _connectionMultiplexers.Length - 1)];
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
                foreach(var multiplexer in _connectionMultiplexers)
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
