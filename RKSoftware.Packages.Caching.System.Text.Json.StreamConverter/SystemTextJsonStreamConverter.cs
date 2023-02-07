using System.Text.Json;
using RKSoftware.Packages.Caching.Contract;

namespace RKSoftware.Packages.Caching.System.Text.Json.StreamConverter
{
    /// <summary>
    /// Object converted realization using byte stream Serialization with System.Text.Json
    /// </summary>
    public class SystemTextJsonStreamConverter : IObjectToStreamConverter
    {
        /// <summary>
        /// Convert object from stream synchronously
        /// </summary>
        /// <typeparam name="T">Type of destination object</typeparam>
        /// <param name="data">Object stream representation</param>
        /// <returns>>Converted object value</returns>
        public T? FromStream<T>(Stream data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// Convert object from stream asynchronously
        /// </summary>
        /// <typeparam name="T">Type of destination object</typeparam>
        /// <param name="data">Object stream representation</param>
        /// <returns>>Converted object value</returns>
        public async Task<T?> FromStreamAsync<T>(Stream data)
        {
            return await JsonSerializer.DeserializeAsync<T>(data);
        }

        /// <summary>
        /// Convert object to bytes
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <param name="obj">Target object value</param>
        /// <returns>Byte representation of the target object</returns>
        public byte[] ToBytes<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }
    }
}