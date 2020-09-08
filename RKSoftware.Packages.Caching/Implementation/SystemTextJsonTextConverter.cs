using RKSoftware.Packages.Caching.Contract;
using System.Text.Json;

namespace RKSoftware.Packages.Caching.Implementation
{
    /// <summary>
    /// Object converted realization using JSON Serialization with System.Text.Json
    /// </summary>
    public class SystemTextJsonTextConverter : IObjectToTextConverter
    {
        /// <summary>
        /// Convert object from string
        /// </summary>
        /// <typeparam name="T">Type of destination object</typeparam>
        /// <param name="data">Object string representation</param>
        /// <returns>Converted object value</returns>
        public T FromString<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <param name="obj">Target object value</param>
        /// <returns>String representation of the target object</returns>
        public string ToString<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
