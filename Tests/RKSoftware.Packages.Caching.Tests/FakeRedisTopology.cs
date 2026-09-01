using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Moq;
using StackExchange.Redis;

namespace RKSoftware.Packages.Caching.Tests
{
    /// <summary>
    /// Redis topologies that the connection multiplexer mock is able to report
    /// </summary>
    internal enum RedisTopologyKind
    {
        /// <summary>
        /// Single master, several replicas and Sentinel nodes, the way a Sentinel managed group is discovered
        /// </summary>
        Standalone,

        /// <summary>
        /// Two cluster masters owning disjoint parts of the keyspace, plus a replica
        /// </summary>
        Cluster,

        /// <summary>
        /// Replicas, Sentinel nodes and a disconnected master, so there is no master to scan
        /// </summary>
        NoMaster
    }

    /// <summary>
    /// <see cref="IServer"/> mock that serves the keys of a shared key source
    /// and counts the key scans that were issued against it
    /// </summary>
    internal sealed class FakeRedisServer
    {
        #region fields

        private readonly Func<IEnumerable<string>> _keySource;
        private readonly Func<string, bool> _ownsKey;
        #endregion

        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeRedisServer"/> class
        /// </summary>
        /// <param name="name">Server name used in test assertion messages</param>
        /// <param name="serverType">Server type reported by <see cref="IServer.ServerType"/></param>
        /// <param name="isReplica">Value reported by <see cref="IServer.IsReplica"/></param>
        /// <param name="isConnected">Value reported by <see cref="IServer.IsConnected"/></param>
        /// <param name="keySource">Whole keyspace known to the test</param>
        /// <param name="ownsKey">Part of the keyspace this server holds, everything by default</param>
        internal FakeRedisServer(string name,
            ServerType serverType,
            bool isReplica,
            bool isConnected,
            Func<IEnumerable<string>> keySource,
            Func<string, bool>? ownsKey = null)
        {
            Name = name;
            EndPoint = new DnsEndPoint(name, 6379);
            _keySource = keySource;
            _ownsKey = ownsKey ?? (_ => true);

            var serverMoq = new Mock<IServer>();
            serverMoq.SetupGet(x => x.ServerType).Returns(serverType);
            serverMoq.SetupGet(x => x.IsReplica).Returns(isReplica);
            serverMoq.SetupGet(x => x.IsConnected).Returns(isConnected);
            serverMoq.SetupGet(x => x.EndPoint).Returns(EndPoint);

            // the code under test enumerates keys asynchronously, IServer.KeysAsync has a single overload
            serverMoq
                .Setup(x => x.KeysAsync(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
                .Returns((int database, RedisValue pattern, int pageSize, long cursor, int pageOffset, CommandFlags flags) =>
                    Scan(pattern).ToAsyncEnumerable());

            Server = serverMoq.Object;
        }
        #endregion

        #region props

        /// <summary>
        /// Server name used in test assertion messages
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Endpoint this server is reachable at, used to resolve the server behind an endpoint
        /// </summary>
        internal EndPoint EndPoint { get; }

        /// <summary>
        /// Mocked <see cref="IServer"/>
        /// </summary>
        internal IServer Server { get; }

        /// <summary>
        /// Number of key scans that were issued against this server
        /// </summary>
        internal int ScanCount { get; private set; }
        #endregion

        #region helpers

        private RedisKey[] Scan(RedisValue pattern)
        {
            ScanCount++;

            var keyRegex = new Regex(
                "^" + string.Join(".*", pattern.ToString().Split('*').Select(Regex.Escape)) + "$",
                RegexOptions.None,
                TimeSpan.FromSeconds(1));

            return _keySource()
                .Where(key => _ownsKey(key) && keyRegex.IsMatch(key))
                .Select(key => (RedisKey)key)
                .ToArray();
        }
        #endregion
    }

    /// <summary>
    /// Factory of the <see cref="FakeRedisServer"/> sets behind every <see cref="RedisTopologyKind"/>
    /// </summary>
    internal static class FakeRedisTopology
    {
        /// <summary>
        /// Keys ending with this suffix live on the second shard of <see cref="RedisTopologyKind.Cluster"/>
        /// </summary>
        internal const string ClusterShardBSuffix = "_2";

        /// <summary>
        /// Hash slot a key is assigned to, a deterministic stand in for the CRC16 based slot
        /// assignment of a real cluster. Backs the GetHashSlot mock so that keys held by one shard
        /// still land in different slots, the way they do on a real cluster.
        /// </summary>
        /// <param name="key">Key to get a slot for</param>
        /// <returns>Slot id between 0 and 16383</returns>
        internal static int SlotOf(RedisKey key)
        {
            var slot = 0;
            foreach (var character in key.ToString())
            {
                // masked on every step so the value can neither overflow nor turn negative
                slot = ((slot * 31) + character) & 0x3FFF;
            }

            return slot;
        }

        /// <summary>
        /// Create the servers reported by a topology
        /// </summary>
        /// <param name="kind">Topology to create servers for</param>
        /// <param name="keySource">Whole keyspace known to the test</param>
        /// <returns>Servers of the requested topology</returns>
        internal static IReadOnlyList<FakeRedisServer> Create(RedisTopologyKind kind,
            Func<IEnumerable<string>> keySource)
        {
            return kind switch
            {
                // the master is deliberately not the first entry, a Sentinel managed connection
                // discovers its endpoints in no particular order
                RedisTopologyKind.Standalone =>
                [
                    new FakeRedisServer("sentinel-1", ServerType.Sentinel, false, true, keySource),
                    new FakeRedisServer("replica-1", ServerType.Standalone, true, true, keySource),
                    new FakeRedisServer("master", ServerType.Standalone, false, true, keySource),
                    new FakeRedisServer("replica-2", ServerType.Standalone, true, true, keySource),
                    new FakeRedisServer("sentinel-2", ServerType.Sentinel, false, true, keySource)
                ],
                RedisTopologyKind.Cluster =>
                [
                    new FakeRedisServer("shard-a", ServerType.Cluster, false, true, keySource,
                        key => !key.EndsWith(ClusterShardBSuffix, StringComparison.Ordinal)),
                    new FakeRedisServer("shard-b", ServerType.Cluster, false, true, keySource,
                        key => key.EndsWith(ClusterShardBSuffix, StringComparison.Ordinal)),
                    new FakeRedisServer("shard-a-replica", ServerType.Cluster, true, true, keySource)
                ],
                RedisTopologyKind.NoMaster =>
                [
                    new FakeRedisServer("replica-1", ServerType.Standalone, true, true, keySource),
                    new FakeRedisServer("disconnected-master", ServerType.Standalone, false, false, keySource),
                    new FakeRedisServer("sentinel-1", ServerType.Sentinel, false, true, keySource)
                ],
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }
    }
}
