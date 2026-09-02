# RKSoftware.Packages.Caching

[![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/)
[![Downloads](https://img.shields.io/nuget/dt/RKSoftware.Packages.Caching.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/)

Redis caching abstractions for .NET applications. This package provides `ICacheService` — a small,
typed, async facade over [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
that handles key namespacing, expiration, get-or-set population, bulk invalidation and Redis
Sentinel awareness, so your application code never talks to Redis directly.

> **This package on its own is not enough.** It defines *how* values are cached but not *how they
> are serialized*. You must also register a converter — either one of the companion packages listed
> at the bottom of this page, or your own `IObjectToTextConverter` / `IObjectToStreamConverter`
> together with an `ICacheRepository`.

## Installation

```shell
dotnet add package RKSoftware.Packages.Caching
dotnet add package RKSoftware.Packages.Caching.System.Text.Json.Converter
```

## Configuration

Add a `RedisCacheSettings` section to `appsettings.json`:

```json
{
  "RedisCacheSettings": {
    "RedisUrl": "localhost:6379",
    "DefaultCacheDuration": 3600,
    "GlobalCacheKey": "RKSoftware.Global",
    "SyncTimeout": 5000,
    "ConnectionMultiplexerPoolSize": 5,
    "UseLogging": true,
    "Password": null
  }
}
```

| Setting | Type | Description |
| --- | --- | --- |
| `RedisUrl` | `string` (**required**) | Redis endpoint, following the StackExchange.Redis convention. Single instance: `localhost:6379`. Sentinel: `localhost:23679,serviceName=redis_master`. |
| `DefaultCacheDuration` | `long` (**required**) | Lifetime, **in seconds**, applied when a call does not pass an explicit duration. Must be greater than zero; a missing or non positive value throws `OptionsValidationException` the first time the settings are read. |
| `GlobalCacheKey` | `string?` | Prefix used for global cache entries. Defaults to `RKSoftware.Global` when not set. |
| `SyncTimeout` | `int?` | Time in milliseconds allowed for synchronous operations. Defaults to 5 seconds. |
| `ConnectionMultiplexerPoolSize` | `int?` | Size of the connection multiplexer pool. |
| `UseLogging` | `bool` | Enables informational logging of cache reads and writes. |
| `Password` | `string?` | Redis password (`requirepass`). ACL users are not used. |

## Getting started

Register the cache and a converter on your `IServiceCollection`:

```csharp
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.System.Text.Json.Converter;

builder.Services
    .UseRKSoftwareCache(
        builder.Configuration.GetSection(nameof(RedisCacheSettings)),
        scopedKeyPrefix: "MyApp")
    .UseSystemTextJsonTextConverter();
```

`scopedKeyPrefix` is prepended to every non-global key, keeping this application's entries separate
from those of other applications sharing the same Redis instance.

Then inject `ICacheService` wherever you need it:

```csharp
using RKSoftware.Packages.Caching.Contract;

public sealed class ProductService(ICacheService cache, IProductRepository repository)
{
    public Task<Product?> GetAsync(int id) =>
        // Returned from cache when present; otherwise loaded, cached and returned.
        cache.GetOrSetCachedObjectAsync(
            $"product.{id}",
            () => repository.LoadAsync(id),
            storageDuration: 600);

    public Task InvalidateAsync(int id) =>
        cache.ResetAsync($"product.{id}");

    // Removes every key of this application containing "product." — e.g. after a bulk import.
    public Task InvalidateAllAsync() =>
        cache.ResetBulkAsync("product.");
}
```

All cached types must be reference types (`where T : class`), and every `storageDuration` is
expressed **in seconds** and must be greater than zero — a non positive value throws
`ArgumentOutOfRangeException`. Overloads that take no duration fall back to `DefaultCacheDuration`,
which is itself validated when the settings are first read, so a cache entry can never be written
with no lifetime.

`GetOrSetCachedObjectAsync` deliberately tolerates cache storage failures: if Redis cannot be reached
the value from `objectReceiver` is still returned and the failure is logged as an error, regardless of
the `UseLogging` setting. It does not tolerate argument errors or a value that cannot be serialized —
those surface to the caller rather than leaving the cache silently unpopulated.

## Registration options

`UseRKSoftwareCache(IConfigurationSection, string)` is a shorthand that wires up the cache service,
the app-settings based options provider and the default connection provider in one call. Compose
them individually when you need to substitute a piece:

| Method | Registers |
| --- | --- |
| `UseRKSoftwareCache(section, scopedKeyPrefix)` | All three of the below, in one call. |
| `UseRKSoftwareCache(scopedKeyPrefix)` | `ICacheService` → `CacheService` (scoped). |
| `UseAppSettingsSettingsProvider(section)` | `IOptions<RedisCacheSettings>` bound to the given configuration section. |
| `UseDefaultConnectionProvider()` | `IConnectionProvider` → `RedisConnectionProvider` (singleton). |

For example, to supply settings from somewhere other than `IConfiguration`:

```csharp
services
    .UseRKSoftwareCache("MyApp")
    .AddSingleton<IOptions<RedisCacheSettings>>(Options.Create(settingsFromVault))
    .UseDefaultConnectionProvider()
    .UseSystemTextJsonTextConverter();
```

## Scoped and global cache entries

Every method has an overload taking `useGlobalCache`. The flag selects which prefix is applied to
the key:

| Call | Resulting Redis key |
| --- | --- |
| `SetCachedObjectAsync("product.1", value)` | `MyApp.product.1` |
| `SetCachedObjectAsync("product.1", value, useGlobalCache: true)` | `RKSoftware.Global.product.1` |

Use the scoped form for data owned by one application, and the global form for data deliberately
shared between applications or containers that agree on the same `GlobalCacheKey`.

## API overview

`ICacheService` (namespace `RKSoftware.Packages.Caching.Contract`), all members asynchronous and
constrained to `where T : class`:

| Member | Purpose |
| --- | --- |
| `GetCachedObjectAsync<T>(key)`<br/>`GetCachedObjectAsync<T>(key, useGlobalCache)` | Read a value; returns `null` when the key is absent. |
| `GetOrSetCachedObjectAsync<T>(key, objectReceiver)`<br/>`(key, objectReceiver, useGlobalCache)`<br/>`(key, objectReceiver, storageDuration)`<br/>`(key, objectReceiver, storageDuration, useGlobalCache)` | Read a value, populating it via the async delegate on a miss. |
| `SetCachedObjectAsync<T>(key, objectToCache)`<br/>`(key, objectToCache, useGlobalCache)`<br/>`(key, obj, storageDuration, useGlobalCache)` | Write a value. Note that passing a duration also requires the `useGlobalCache` flag. |
| `ResetAsync(key)`<br/>`ResetAsync(key, useGlobalCache)` | Remove a single entry. |
| `ResetBulkAsync(IEnumerable<string> keys)`<br/>`ResetBulkAsync(keys, useGlobalCache)` | Remove several entries by exact key. |
| `ResetBulkAsync(string partOfKey)`<br/>`ResetBulkAsync(partOfKey, globalCache)` | Remove every entry whose key contains the given substring. Scans every connected master — see [Redis Cluster](#redis-cluster). |

## Extension points

Replace any of these by registering your own implementation instead of the defaults:

| Contract | Default implementation | Lifetime |
| --- | --- | --- |
| `ICacheService` | `CacheService` | Scoped |
| `ICacheRepository` | Supplied by the converter package (`StringCacheRepository` or `StreamCacheRepository`) | Scoped |
| `IConnectionProvider` | `RedisConnectionProvider` | Singleton |
| `IObjectToTextConverter` | Supplied by a text converter package | Scoped |
| `IObjectToStreamConverter` | Supplied by the stream converter package | Scoped |

## Redis Sentinel

`RedisConnectionProvider` treats a connection as a Sentinel one when `RedisUrl` contains
`serviceName=`. In that mode read operations are issued with `CommandFlags.DemandReplica`, so they
are served by a replica.

Writes and removals always use `CommandFlags.FireAndForget | CommandFlags.DemandMaster`. Because
they are fire-and-forget, an awaited `SetCachedObjectAsync` or `ResetAsync` means the command has
been dispatched — not that the server has already applied it.

The two halves combine into a consistency caveat worth planning around: **a read issued immediately
after a write or a reset may still return the previous value.** The removal is dispatched
fire-and-forget to the master, while the following read is served by a replica that has not
necessarily received the replication yet. Awaiting the reset does not close that window, so code
that needs to observe its own writes should not rely on a read-back through the cache — re-read from
the system of record, or register an `IConnectionProvider` of your own that returns flags without
`FireAndForget` and without `DemandReplica`.

## Redis Cluster

Cluster mode is detected from the connection, and the reset methods adapt to it.

`ResetBulkAsync(partOfKey)` enumerates keys on **every connected master**, so all shards of a
cluster are covered. Replicas and Sentinel nodes are never scanned, and the call fails with
`RedisConnectionException` when no master is reachable rather than silently reporting that nothing
matched.

Both `ResetBulkAsync` overloads then group the keys they are about to remove **by hash slot** and
issue one `DEL` per slot. A cluster rejects a multi-key operation whose keys do not all hash to the
same slot (`CROSSSLOT`), and grouping per slot is what keeps the removals within that rule — note
that grouping per *node* is not sufficient, since one master owns thousands of slots. No `{hash tag}`
in your keys and no per-key `ResetAsync` is needed.

Off a cluster there is no slot constraint, so the keys are removed with a single `DEL`.

## Related packages

Register exactly one converter package alongside this one:

- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.Converter.svg?label=RKSoftware.Packages.Caching.System.Text.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/) — JSON string serialization with `System.Text.Json`.
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter.svg?label=RKSoftware.Packages.Caching.Newtonsoft.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/) — JSON string serialization with `Newtonsoft.Json`.
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter.svg?label=RKSoftware.Packages.Caching.System.Text.Json.StreamConverter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/) — UTF-8 byte and stream serialization with `System.Text.Json`, for large payloads.

Source, issues and full documentation:
[github.com/rk-software-systems/rk-dn-redis-cache-provider](https://github.com/rk-software-systems/rk-dn-redis-cache-provider)

## License

Released into the public domain under [The Unlicense](https://github.com/rk-software-systems/rk-dn-redis-cache-provider/blob/master/LICENSE).
