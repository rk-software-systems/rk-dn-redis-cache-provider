# RKSoftware.Packages.Caching.System.Text.Json.Converter

[![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.Converter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/)
[![Downloads](https://img.shields.io/nuget/dt/RKSoftware.Packages.Caching.System.Text.Json.Converter.svg)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/)

`System.Text.Json` serialization for
[RKSoftware.Packages.Caching](https://www.nuget.org/packages/RKSoftware.Packages.Caching/). It
contains an `IObjectToTextConverter` implementation together with the registration extension that
plugs it into the cache service, so cached objects are stored in Redis as JSON strings.

The core caching package defines *how* values are cached but not *how they are serialized* — this
package supplies the missing half. Register exactly one converter package in an application.

## Installation

```shell
dotnet add package RKSoftware.Packages.Caching
dotnet add package RKSoftware.Packages.Caching.System.Text.Json.Converter
```

## Getting started

Chain the converter registration onto the cache registration:

```csharp
using RKSoftware.Packages.Caching.Infrastructure;
using RKSoftware.Packages.Caching.System.Text.Json.Converter;

builder.Services
    .UseRKSoftwareCache(
        builder.Configuration.GetSection(nameof(RedisCacheSettings)),
        scopedKeyPrefix: "MyApp")
    .UseSystemTextJsonTextConverter();
```

Nothing else is required — `ICacheService` now serializes with `System.Text.Json`:

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

`UseSystemTextJsonTextConverter()` (namespace
`RKSoftware.Packages.Caching.System.Text.Json.Converter`) adds two scoped services:

| Contract | Implementation |
| --- | --- |
| `IObjectToTextConverter` | `SystemTextJsonTextConverter` |
| `ICacheRepository` | `StringCacheRepository` |

## API overview

`SystemTextJsonTextConverter` implements `IObjectToTextConverter`:

| Member | Behaviour |
| --- | --- |
| `string ToString<T>(T obj)` | `JsonSerializer.Serialize` of the value. |
| `T FromString<T>(string data)` | `JsonSerializer.Deserialize`. Throws `InvalidOperationException` when deserialization yields `null`. |

Both members are constrained to `where T : class`, matching `ICacheService`. Serialization uses the
`System.Text.Json` defaults, so annotate your models with `System.Text.Json.Serialization`
attributes (`[JsonPropertyName]`, `[JsonIgnore]`, `[JsonConverter]`) to influence the output.

## Hash fields

The same converter backs the hash field API of `ICacheService`. `SetCachedHashFieldAsync<T>` stores
the field as a JSON string, and `GetCachedHashFieldAsync<T>` deserializes it, with the same
`InvalidOperationException` when the stored JSON is `null`. The `byte[]` setters bypass the
converter and store bytes as is. `GetCachedHashFieldStreamAsync` returns a typed field as its UTF-8
JSON text, not as the original object. See the
[core package README](https://github.com/rk-software-systems/rk-dn-redis-cache-provider/blob/master/RKSoftware.Packages.Caching/README.md#hash-fields)
for expiration and removal rules.

## Choosing a converter

- Use **this package** for new code: `System.Text.Json` is built into the framework and adds no
  extra dependency.
- Use
  [RKSoftware.Packages.Caching.Newtonsoft.Json.Converter](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/)
  when your models already rely on `Newtonsoft.Json` attributes or custom converters.
- Use
  [RKSoftware.Packages.Caching.System.Text.Json.StreamConverter](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/)
  for large payloads: it serializes straight to UTF-8 bytes and deserializes from a stream, avoiding
  the intermediate `string` this package produces.

## Related packages

- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.svg?label=RKSoftware.Packages.Caching)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/) — the core caching abstractions this package extends. **Required.**
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter.svg?label=RKSoftware.Packages.Caching.Newtonsoft.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/) — JSON string serialization with `Newtonsoft.Json`.
- [![NuGet](https://img.shields.io/nuget/v/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter.svg?label=RKSoftware.Packages.Caching.System.Text.Json.StreamConverter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/) — UTF-8 byte and stream serialization with `System.Text.Json`.

Source, issues and full documentation:
[github.com/rk-software-systems/rk-dn-redis-cache-provider](https://github.com/rk-software-systems/rk-dn-redis-cache-provider)

## License

Released into the public domain under [The Unlicense](https://github.com/rk-software-systems/rk-dn-redis-cache-provider/blob/master/LICENSE).
