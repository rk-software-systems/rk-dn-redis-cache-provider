# RKSoftware.Packages.Caching.System.Text.Json.StreamConverter

[![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/)
[![Downloads](https://img.shields.io/nuget/dt/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/)

Binary `System.Text.Json` serialization for
[RKSoftware.Packages.Caching](https://www.nuget.org/packages/RKSoftware.Packages.Caching/). It
contains an `IObjectToStreamConverter` implementation together with the registration extension that
plugs it into the cache service, so cached objects are written to Redis as UTF-8 bytes and read back
from a stream.

The core caching package defines *how* values are cached but not *how they are serialized* — this
package supplies the missing half. Register exactly one converter package in an application.

## Installation

```shell
dotnet add package RKSoftware.Packages.Caching
dotnet add package RKSoftware.Packages.Caching.System.Text.Json.StreamConverter
```

## Getting started

Chain the converter registration onto the cache registration:

```csharp
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.System.Text.Json.StreamConverter;

builder.Services
    .UseRKSoftwareCache(
        builder.Configuration.GetSection(nameof(RedisCacheSettings)),
        scopedKeyPrefix: "MyApp")
    .UseSystemTextJsonStreamConverter();
```

Nothing else is required — the `ICacheService` API is unchanged, only the storage format differs:

```csharp
using RKSoftware.Packages.Caching.Contract;

public sealed class ReportService(ICacheService cache, IReportRepository repository)
{
    public Task<Report?> GetAsync(int id) =>
        cache.GetOrSetCachedObjectAsync(
            $"report.{id}",
            () => repository.RenderAsync(id),
            storageDuration: 600);
}
```

## What it registers

`UseSystemTextJsonStreamConverter()` (namespace
`RKSoftware.Packages.Caching.System.Text.Json.StreamConverter`) adds two scoped services:

| Contract | Implementation |
| --- | --- |
| `IObjectToStreamConverter` | `SystemTextJsonStreamConverter` |
| `ICacheRepository` | `StreamCacheRepository` |

## API overview

`SystemTextJsonStreamConverter` implements `IObjectToStreamConverter`:

| Member | Behaviour |
| --- | --- |
| `byte[] ToBytes<T>(T obj)` | `JsonSerializer.SerializeToUtf8Bytes` of the value. |
| `Task<T?> FromStreamAsync<T>(Stream data)` | `JsonSerializer.DeserializeAsync` over the stream; returns `null` when the payload deserializes to `null`. |

`FromStreamAsync<T>` is constrained to `where T : class`, matching `ICacheService`. Serialization
uses the `System.Text.Json` defaults, so annotate your models with
`System.Text.Json.Serialization` attributes (`[JsonPropertyName]`, `[JsonIgnore]`,
`[JsonConverter]`) to influence the output.

## When to use this instead of a text converter

The two text converters serialize through an intermediate `string`; this one goes straight to and
from UTF-8 bytes. For large cached objects that avoids allocating the whole JSON document as a
string on every read and write, which reduces large-object-heap pressure.

For small values the difference is negligible, and the text converters produce human-readable
entries that are easier to inspect with `redis-cli`.

> **Register exactly one converter package.** All three register `ICacheRepository`, so adding more
> than one leaves the last registration in effect and silently changes the storage format. Values
> written by a text converter cannot be read back by this one, and vice versa — switching converters
> in an existing deployment requires flushing the affected keys.

## Related packages

- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.svg?label=RKSoftware.Packages.Caching)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/) — the core caching abstractions this package extends. **Required.**
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.Converter.svg?label=RKSoftware.Packages.Caching.System.Text.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/) — JSON string serialization with `System.Text.Json`.
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter.svg?label=RKSoftware.Packages.Caching.Newtonsoft.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/) — JSON string serialization with `Newtonsoft.Json`.

Source, issues and full documentation:
[github.com/rk-software-systems/rk-dn-redis-cache-provider](https://github.com/rk-software-systems/rk-dn-redis-cache-provider)

## License

Released into the public domain under [The Unlicense](https://github.com/rk-software-systems/rk-dn-redis-cache-provider/blob/master/LICENSE).
