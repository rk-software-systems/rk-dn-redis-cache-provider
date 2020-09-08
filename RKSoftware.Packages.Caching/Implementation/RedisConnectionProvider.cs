﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Infrastructure;
using StackExchange.Redis;
using System;

namespace RKSoftware.Packages.Caching.Implementation
{
    public class RedisConnectionProvider : IConnectionProvider
    {
        private static IConnectionMultiplexer _connectionMultiplexer;
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
        /// <returns>Redis database connection multiplexer <see cref=IConnectionMultiplexer"/></returns>
        public IConnectionMultiplexer GetConnection()
        {
            if (_connectionMultiplexer != null)
            {
                return _connectionMultiplexer;

            }

            lock (_multiplexerInitLock)
            {
                if (_connectionMultiplexer != null)
                {
                    return _connectionMultiplexer;
                }

                var options = new ConfigurationOptions()
                {
                    EndPoints =
                {
                    _redisCacheSettings.RedisUrl.ToString()
                },
                    SyncTimeout = 3000,
                    AbortOnConnectFail = false
                };

                _logger.LogInformation(LogMessageResource.RedisConnectionOpenening);
                _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
            }

            return _connectionMultiplexer;
        }
    }
}