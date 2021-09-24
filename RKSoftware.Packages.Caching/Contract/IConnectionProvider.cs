﻿using StackExchange.Redis;
using System;

namespace RKSoftware.Packages.Caching.Contract
{
    /// <summary>
    /// This interface is used to obtain connection Multiplexer to Redis Database
    /// </summary>
    public interface IConnectionProvider : IDisposable
    {
        /// <summary>
        /// Get redis database connection multiplexer
        /// </summary>
        /// <returns>Redis database connection multiplexer <see cref="IConnectionMultiplexer"/></returns>
        IConnectionMultiplexer GetConnection();

        /// <summary>
        /// This flag indicates if particular connection is Sentinel one
        /// it is used to determine if it is possible to use Redis replica fore read
        /// </summary>
        bool IsSentinel { get; }
    }
}
