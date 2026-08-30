# RKSoftware.Packages.Caching.Newtonsoft.Json.Converter

[![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/)
[![Downloads](https://img.shields.io/nuget/dt/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/)

`Newtonsoft.Json` serialization for
[RKSoftware.Packages.Caching](https://www.nuget.org/packages/RKSoftware.Packages.Caching/). It
contains an `IObjectToTextConverter` implementation together with the registration extension that
plugs it into the cache service, so cached objects are stored in Redis as JSON strings.

The core caching package defines *how* values are cached but not *how they are serialized* — this
package supplies the missing half. Register exactly one converter package in an application.

## Installation

```shell
dotnet add package RKSoftware.Packages.Caching
dotnet add package RKSoftware.Packages.Caching.Newtonsoft.Json.Converter
```

This package brings in `Newtonsoft.Json` (13.0.4 or later) as a transitive dependency.

## Getting started

Chain the converter registration onto the cache registration:

```csharp
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.Newtonsoft.Json.Converter;

builder.Services
    .UseRKSoftwareCache(
        builder.Configuration.GetSection(nameof(RedisCacheSettings)),
        scopedKeyPrefix: "MyApp")
    .UseNewtonsoftJsonTextConverter();
```

Nothing else is required — `ICacheService` now serializes with `Newtonsoft.Json`:

```csharp
using RKSoftware.Packages.Caching.Contract;

public sealed class ProductService(ICacheService cache, IProductRepository repository)
{
    public Task<Product?> GetAsync(int id) =>
        cache.GetOrSetCachedObjectAsync(
            $"product.{id}",
            () => repository.LoadAsync(id),
            storageDuration: 600);
}
```

## What it registers

`UseNewtonsoftJsonTextConverter()` (namespace
`RKSoftware.Packages.Caching.Newtonsoft.Json.Converter`) adds two scoped services:

| Contract | Implementation |
| --- | --- |
| `IObjectToTextConverter` | `NewtonsoftJsonTextConverter` |
| `ICacheRepository` | `StringCacheRepository` |

## API overview

`NewtonsoftJsonTextConverter` implements `IObjectToTextConverter`:

| Member | Behaviour |
| --- | --- |
| `string ToString<T>(T obj)` | `JsonConvert.SerializeObject` of the value. |
| `T FromString<T>(string data)` | `JsonConvert.DeserializeObject`. Throws `InvalidOperationException` when deserialization yields `null`. |

Both members are constrained to `where T : class`, matching `ICacheService`. Serialization uses the
`Newtonsoft.Json` defaults, so `[JsonProperty]`, `[JsonIgnore]` and custom `JsonConverter`
attributes on your models are honoured.

## Choosing a converter

- Use **this package** when your models already depend on `Newtonsoft.Json` attributes, custom
  converters, or `JsonConvert` behaviours that `System.Text.Json` does not reproduce.
- Use
  [RKSoftware.Packages.Caching.System.Text.Json.Converter](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/)
  for new code — it relies on the built-in serializer and adds no extra dependency.
- Use
  [RKSoftware.Packages.Caching.System.Text.Json.StreamConverter](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/)
  for large payloads: it serializes straight to UTF-8 bytes and deserializes from a stream, avoiding
  the intermediate `string` this package produces.

## Related packages

- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.svg?label=RKSoftware.Packages.Caching)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/) — the core caching abstractions this package extends. **Required.**
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.Converter.svg?label=RKSoftware.Packages.Caching.System.Text.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/) — JSON string serialization with `System.Text.Json`.
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter.svg?label=RKSoftware.Packages.Caching.System.Text.Json.StreamConverter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/) — UTF-8 byte and stream serialization with `System.Text.Json`.

Source, issues and full documentation:
[github.com/rk-software-systems/rk-dn-redis-cache-provider](https://github.com/rk-software-systems/rk-dn-redis-cache-provider)

## License

Released into the public domain under [The Unlicense](https://github.com/rk-software-systems/rk-dn-redis-cache-provider/blob/master/LICENSE).
