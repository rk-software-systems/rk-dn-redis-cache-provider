# Introduction 
This packages contains abstractions that could be used to add Redis caching to .NET core applications

### NuGet
**RKSoftware.Packages.Caching**
[![NuGet Badge](https://buildstats.info/nuget/RKSoftware.Packages.Caching)](https://www.nuget.org/packages/RKSoftware.Packages.Caching/)

**RKSoftware.Packages.Caching.Newtonsoft.Json.Converter**
[![NuGet Badge](https://buildstats.info/nuget/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.Newtonsoft.Json.Converter/)

**RKSoftware.Packages.Caching.System.Text.Json.Converter**
[![NuGet Badge](https://buildstats.info/nuget/RKSoftware.Packages.Caching.System.Text.Json.Converter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.Converter/)

**RKSoftware.Packages.Caching.System.Text.Json.StreamConverter**
[![NuGet Badge](https://buildstats.info/nuget/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter)](https://www.nuget.org/packages/RKSoftware.Packages.Caching.System.Text.Json.StreamConverter/)

# Getting Started
To use caching do the following:
- Add RedisCacheSettings section to your application settings this section should contain the following fields:
  - *RedisUrl* - path to the Redis instanse
  - *GlobalCacheKey* -  This key prefix is going to be used as a prefix for Global cache entries
  - *SyncTimeout* - Specifies the time in milliseconds that the system should allow for synchronous operations (defaults to 5 seconds)
  - *ConnectionMultiplexerPoolSize* - Size of connection multiplexer pool
  - *UseLogging* - This flag indicates if logs need to be enabled
  - *Password* - Redis password that can be used to access redis instancess (requirepass redis settings) ACL is not used in this case
  - *DefaultCacheDuration* - Default cache storage duration. (could be overriden when you store value in cache)
- Register cache services using 'RegisterGameOnCache' Extension method on 'IServiceCollection''

# Build and Test
Update this package and push to master it will trigger pipeline build and packages publish

# Contribute
Do not foget to cleanup warnings before push. Otherwise build will fail