using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Contract
{
    /// <summary>
    /// This interface is used to obtain connection Multiplexer to Redis Database
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// Get redis database connection multiplexer
        /// </summary>
        /// <returns>Redis database connection multiplexer <see cref=IConnectionMultiplexer"/></returns>
        IConnectionMultiplexer GetConnection();
    }
}
