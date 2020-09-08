using Newtonsoft.Json;
using RKSoftware.Packages.Caching.Contract;

namespace RKSoftware.Packages.Cachinig.Newtonsoft.Json.Converter
{
    /// <summary>
    /// This class is used to convert objects to string and vise versa using Newtonsoft JSON library
    /// </summary>
    public class NewtonsoftJsonTextConverter : IObjectToTextConverter
    {
        /// <summary>
        /// Convert object from string
        /// </summary>
        /// <typeparam name="T">Type of destination object</typeparam>
        /// <param name="data">Object string representation</param>
        /// <returns>Converted object value</returns>
        public T FromString<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <param name="obj">Target object value</param>
        /// <returns>String representation of the target object</returns>
        public string ToString<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
