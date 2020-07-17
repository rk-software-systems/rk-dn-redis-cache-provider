using RKSoftware.Packages.Caching.Contract;
using System.Text.Json;

namespace RKSoftware.Packages.Caching.Implementation
{
    /// <summary>
    /// Object converted realization using JSON Serialization with System.Text.Json
    /// </summary>
    public class SystemTextJsonTextConverter : IObjectToTextConverter
    {
        public T FromString<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        public string ToString<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
