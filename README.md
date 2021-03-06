# Introduction 
This packages contains abstractions that could be used to add Redis caching to .NET core applications

### NuGet
**RKSoftware.Packages.Caching**
[![NuGet Badge](https://buildstats.info/nuget/RKSoftware.Packages.Caching)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/)

**RKSoftware.Packages.Caching.Newtonsoft.Json.Converter**
[![NuGet Badge](https://buildstats.info/nuget/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/)

**RKSoftware.Packages.Caching.System.Text.Json.Converter**
[![NuGet Badge](https://buildstats.info/nuget/RKSoftware.Packages.Caching.System.Text.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/)

# Getting Started
To use caching do the following:
- Add RedisCacheSettings section to your application settings this section should contain the following fields:
  - RedisUrl - url to Redis instance
  - DefaultCacheDuration - Default cache storage duration. (could be overriden when you store value in cache)
- Register cache services using 'RegisterGameOnCache' Extension method on 'IServiceCollection''

# Build and Test
Update this package and push to master it will trigger pipeline build and packages publish

# Contribute
Do not foget to cleanup warnings before push. Otherwise build will fail