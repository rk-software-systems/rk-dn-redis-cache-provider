using Microsoft.Extensions.DependencyInjection;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Repositories;

namespace RKSoftware.Packages.Caching.System.Text.Json.StreamConverter
{
    /// <summary>
    /// This class contains registration extension that is used to register System.Json.Text stream converter to be used in Redis Service
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// This method is used to register Object to bytes converter that uses System.Text.Json serialization <see cref="SystemTextJsonStreamConverter"/>.
        /// </summary>
        /// <param name="services">Service collection to register <see cref="IObjectToStreamConverter"/></param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseSystemTextJsonStreamConverter(this IServiceCollection services)
        {
            services.AddScoped<ICacheRepository, StreamCacheRepository>();
            services.AddScoped<IObjectToStreamConverter, SystemTextJsonStreamConverter>();
            return services;
        }
    }
}
