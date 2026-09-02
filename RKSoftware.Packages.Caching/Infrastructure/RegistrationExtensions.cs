using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Implementation;
using System;

namespace RKSoftware.Packages.Caching.Infrastructure
{
    /// <summary>
    /// This class contains extensions method that is used to register Cache service
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Register RK Software Redis cache provider with App Settings Redis Settings Provider and default connection provider.
        /// </summary>
        /// <param name="services">Service collection to add registrations.</param>
        /// <param name="configuration">Configuration Section that contains Redis settings</param>
        /// <param name="scopedKeyPrefix">This key is used as a prefix to all Redis keys</param>
        public static IServiceCollection UseRKSoftwareCache(this IServiceCollection services,
            IConfigurationSection configuration,
            string scopedKeyPrefix)
        {
            services.UseRKSoftwareCache(scopedKeyPrefix)
                .UseAppSettingsSettingsProvider(configuration)
                .UseDefaultConnectionProvider();

            return services;
        }

        /// <summary>
        /// Register RKSoftware Cache service only
        /// </summary>
        /// <param name="services">Service collection to register cache service</param>
        /// <param name="scopedKeyPrefix">This string is used as a prefix for local cache entries.</param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseRKSoftwareCache(this IServiceCollection services,
            string scopedKeyPrefix)
        {

            ArgumentNullException.ThrowIfNull(scopedKeyPrefix);

            services.AddScoped<ICacheService>(x =>
            new CacheService(x.GetRequiredService<IOptions<RedisCacheSettings>>(),
                             x.GetRequiredService<ILogger<CacheService>>(),
                             x.GetRequiredService<IConnectionProvider>(),
                             x.GetRequiredService<ICacheRepository>(),
                             scopedKeyPrefix));

            return services;
        }

        /// <summary>
        /// Register <see cref="RedisCacheSettings"/> data provider that uses AppSettings <see cref="IConfigurationSection"/> as source.
        /// Settings are validated the first time they are read, so a missing <see cref="RedisCacheSettings.RedisUrl"/>
        /// or a non positive <see cref="RedisCacheSettings.DefaultCacheDuration"/> throws <see cref="OptionsValidationException"/>.
        /// </summary>
        /// <param name="services">Service collection to register settings provider</param>
        /// <param name="redisSettingsConfigurationSection"><see cref="IConfigurationSection"/> with <see cref="RedisCacheSettings"/></param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseAppSettingsSettingsProvider(this IServiceCollection services,
            IConfigurationSection redisSettingsConfigurationSection)
        {
            ArgumentNullException.ThrowIfNull(redisSettingsConfigurationSection);

            services.Configure<RedisCacheSettings>(redisSettingsConfigurationSection);
            services.AddSingleton<IValidateOptions<RedisCacheSettings>, RedisCacheSettingsValidator>();

            return services;
        }

        /// <summary>
        /// Register default implementation of <see cref="IConnectionProvider"/>
        /// </summary>
        /// <param name="services">Service collection to register <see cref="IConnectionProvider"/></param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseDefaultConnectionProvider(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionProvider, RedisConnectionProvider>();
            return services;
        }
    }
}
