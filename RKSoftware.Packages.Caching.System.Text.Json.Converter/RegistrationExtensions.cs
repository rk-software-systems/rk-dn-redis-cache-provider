﻿using Microsoft.Extensions.DependencyInjection;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Repositories;

namespace RKSoftware.Packages.Caching.System.Text.Json.Converter
{
    /// <summary>
    /// This class contains registration extension that is used to register System.Json.Text text converter to be used in Redis Service
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// This method is used to register Object to Text converter that uses System.Text.Json serialization <see cref="SystemTextJsonTextConverter"/>.
        /// </summary>
        /// <param name="services">Service collection to register <see cref="IObjectToTextConverter"/></param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseSystemTextJsonTextConverter(this IServiceCollection services)
        {
            services.AddScoped<ICacheRepository, StringCacheRepository>();
            services.AddScoped<IObjectToTextConverter, SystemTextJsonTextConverter>();
            return services;
        }
    }
}
