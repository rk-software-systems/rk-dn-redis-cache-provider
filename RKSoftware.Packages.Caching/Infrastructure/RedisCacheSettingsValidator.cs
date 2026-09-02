using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace RKSoftware.Packages.Caching.Infrastructure
{
    /// <summary>
    /// Validates <see cref="RedisCacheSettings"/> obtained from configuration.
    /// Configuration binding does not honour the <c>required</c> modifier, so settings that are
    /// mandatory for the cache to work are verified here instead. Validation runs the first time
    /// <see cref="IOptions{TOptions}.Value"/> is read, which happens while a cache service is being
    /// constructed, so a misconfigured application fails loudly rather than caching nothing.
    /// Registered by <see cref="RegistrationExtensions.UseAppSettingsSettingsProvider"/>. Register it
    /// yourself when you supply <see cref="RedisCacheSettings"/> through your own options pipeline.
    /// </summary>
    public sealed class RedisCacheSettingsValidator : IValidateOptions<RedisCacheSettings>
    {
        /// <summary>
        /// Validate <see cref="RedisCacheSettings"/>
        /// </summary>
        /// <param name="name">Name of the options instance being validated</param>
        /// <param name="options">Settings to be validated</param>
        /// <returns>Validation result listing every setting that is not usable</returns>
        public ValidateOptionsResult Validate(string? name, RedisCacheSettings options)
        {
            if (options == null)
            {
                return ValidateOptionsResult.Fail($"{nameof(RedisCacheSettings)} section is missing.");
            }

            var failures = new List<string>();

            if (string.IsNullOrWhiteSpace(options.RedisUrl))
            {
                failures.Add($"{nameof(RedisCacheSettings.RedisUrl)} must be set.");
            }

            if (options.DefaultCacheDuration <= 0)
            {
                failures.Add($"{nameof(RedisCacheSettings.DefaultCacheDuration)} must be greater than zero (seconds).");
            }

            return failures.Count > 0
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}
