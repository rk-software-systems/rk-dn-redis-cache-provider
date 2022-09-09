using Microsoft.Extensions.DependencyInjection;
using RKSoftware.Packages.Caching.Contract;

namespace RKSoftware.Packages.Caching.Newtonsoft.Json.Converter
{
    /// <summary>
    /// This class contains registration extension that is used to register SystemJson.TextConverter to be used in Redis Service
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// This method is used to register Object to Text converter that uses System.Text.Json serialization 
        /// <see cref="NewtonsoftJsonTextConverter"/>.
        /// </summary>
        /// <param name="services">Service collection to register <see cref="IObjectToTextConverter"/></param>
        /// <returns>Services collection with registered service</returns>
        public static IServiceCollection UseNewtonsoftJsonTextConverter(this IServiceCollection services)
        {
            services.AddSingleton<IObjectToTextConverter, NewtonsoftJsonTextConverter>();
            return services;
        }
    }
}
