# RKSoftware.Packages.Caching

Redis caching abstractions for .NET applications, targeting `net10.0`. The solution ships a core
package that provides a typed, async cache facade over
[StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis), plus interchangeable
converter packages that decide how cached objects are serialized.

## Packages

| Package | Version | Purpose | Documentation |
| --- | --- | --- | --- |
| `RKSoftware.Packages.Caching` | [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/) | Core abstractions: `ICacheService`, settings, DI registration, Redis connection and Sentinel support. | [README](RKSoftware.Packages.Caching/README.md) |
| `RKSoftware.Packages.Caching.System.Text.Json.Converter` | [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.Converter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/) | JSON string serialization with `System.Text.Json`. | [README](RKSoftware.Packages.Caching.System.Text.Json.Converter/README.md) |
| `RKSoftware.Packages.Caching.Newtonsoft.Json.Converter` | [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/) | JSON string serialization with `Newtonsoft.Json`. | [README](RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/README.md) |
| `RKSoftware.Packages.Caching.System.Text.Json.StreamConverter` | [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/) | UTF-8 byte and stream serialization with `System.Text.Json`, for large payloads. | [README](RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/README.md) |

An application needs the core package **and exactly one converter package** — the core package does
not serialize anything on its own.

[RKSoftware.Packages.Caching.Converter.Mock](RKSoftware.Packages.Caching.Converter.Mock/) is an
internal test double used by the test and integration projects. It is not published to NuGet.

## Quick start

```shell
dotnet add package RKSoftware.Packages.Caching
dotnet add package RKSoftware.Packages.Caching.System.Text.Json.Converter
```

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

| Setting | Description |
| --- | --- |
| `RedisUrl` | Redis endpoint, following the StackExchange.Redis convention. Single instance: `localhost:6379`. Sentinel: `localhost:23679,serviceName=redis_master`. In Sentinel mode reads are served by a replica. |
| `DefaultCacheDuration` | Lifetime, in seconds, used when a call does not pass an explicit duration. |
| `GlobalCacheKey` | Prefix for global cache entries, shared across applications. Defaults to `RKSoftware.Global`. |
| `SyncTimeout` | Time in milliseconds allowed for synchronous operations. Defaults to 5 seconds. |
| `ConnectionMultiplexerPoolSize` | Size of the connection multiplexer pool. |
| `UseLogging` | Enables informational logging of cache reads and writes. |
| `Password` | Redis password (`requirepass`). ACL users are not used. |

Register the cache and a converter with the `UseRKSoftwareCache` extension method on
`IServiceCollection`:

```csharp
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.System.Text.Json.Converter;

builder.Services
    .UseRKSoftwareCache(
        builder.Configuration.GetSection(nameof(RedisCacheSettings)),
        scopedKeyPrefix: "MyApp")
    .UseSystemTextJsonTextConverter();
```

Then inject `ICacheService`:

```csharp
using RKSoftware.Packages.Caching.Contract;

var product = await cache.GetOrSetCachedObjectAsync(
    $"product.{id}",
    () => repository.LoadAsync(id),
    storageDuration: 600);
```

See the [core package README](RKSoftware.Packages.Caching/README.md) for the full API, key
namespacing rules, extension points and Sentinel behaviour.

## Build and Test

Requires the .NET 10 SDK.

```shell
dotnet build RKSoftware.Packages.Caching.sln -c Release
dotnet test Tests/RKSoftware.Packages.Caching.Tests
```

The unit tests are MSTest based and run fully offline — they use the internal
[Converter.Mock](RKSoftware.Packages.Caching.Converter.Mock/) package and a mocked
`IConnectionProvider` backed by an in-memory cache, so no Redis instance is needed.

For manual testing against real Redis, [docker-compose.yml](docker-compose.yml) brings up a
standalone instance plus a master/replica/Sentinel cluster (password from [.env](.env)):

```shell
docker compose up
```

[Integration/TestCluster](Integration/TestCluster/) is the console harness that exercises the cache
against that cluster.

### Publishing

Pushing to `master` triggers
[.github/workflows/dotnet-package-publish.yml](.github/workflows/dotnet-package-publish.yml), which
builds each package project in Release (packages are produced by `GeneratePackageOnBuild`) and
pushes the resulting `.nupkg` files to nuget.org.

Bump `<Version>`, `<AssemblyVersion>` and `<FileVersion>` in the affected `.csproj` before merging.
The push uses `--skip-duplicate`, so an unchanged version is silently skipped rather than failing.

## Contribute

- Branch off `master` and open a pull request.
- **Clean up all warnings before pushing.** Every project sets
  `AnalysisMode=AllEnabledByDefault` and [azure-build-pipelines.yml](azure-build-pipelines.yml)
  builds with `/warnaserror`, so any warning fails the build.
- Agreed analyzer suppressions live in [.editorconfig](.editorconfig) — add exclusions there rather
  than inline.
- Keep XML documentation comments on public members; all four packages emit a documentation file.
- Add or update tests under [Tests/](Tests/) for behaviour changes.
- Update the affected package README when the public API changes — those files ship inside the NuGet
  packages.

## License

Released into the public domain under [The Unlicense](LICENSE).
