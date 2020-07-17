using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        public static IServiceCollection UseRKSoftwareCache(this IServiceCollection services, 
            IConfigurationSection configuration,
            string scopedKeyPrefix)
        {
            services.UseRKSoftwareCache(scopedKeyPrefix)
                .UseAppSettingsSettingsProvider(configuration)
                .UseDefaultConnectionProvider()
                .UseSystemTextJsonTextConverter();

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

            if (scopedKeyPrefix == null)
            {
                throw new ArgumentNullException(nameof(scopedKeyPrefix));
            }

            services.AddScoped<ICacheService, CacheService>();
            return services;
        }

        /// <summary>
        /// Register <see cref="RedisCacheSettings"/> data provider that uses AppSettings <see cref="IConfigurationSection"/> as source.
        /// </summary>
        /// <param name="services">Service collection to register settings provider</param>
        /// <param name="redisSettingsCondfigurationSection"><see cref="IConfigurationSection"/> with <see cref="RedisCacheSettings"/></param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseAppSettingsSettingsProvider(this IServiceCollection services,
            IConfigurationSection redisSettingsCondfigurationSection)
        {
            if (redisSettingsCondfigurationSection == null)
            {
                throw new ArgumentNullException(nameof(redisSettingsCondfigurationSection));
            }

            services.Configure<RedisCacheSettings>(redisSettingsCondfigurationSection);

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

        /// <summary>
        /// This method is used to register Object to Text converter that uses System.Text.Json serialization <see cref="SystemTextJsonTextConverter"/>.
        /// </summary>
        /// <param name="services">Service collection to register <see cref="IObjectToTextConverter"/></param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseSystemTextJsonTextConverter(this IServiceCollection services)
        {
            services.AddSingleton<IObjectToTextConverter, SystemTextJsonTextConverter>();
            return services;
        }
    }
}
