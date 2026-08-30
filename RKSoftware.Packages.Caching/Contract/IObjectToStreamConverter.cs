using System.IO;
using System.Threading.Tasks;

namespace RKSoftware.Packages.Caching.Contract
{
    /// <summary>
    /// This service is used to convert objects that are being stored as string to CLR object
    /// </summary>
    public interface IObjectToStreamConverter
    {
        /// <summary>
        /// Create object from stream asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T?> FromStreamAsync<T>(Stream data) where T : class;

        /// <summary>
        /// Serialize object to bytes
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <param name="obj">Source object value</param>
        /// <returns></returns>
        byte[] ToBytes<T>(T obj);
    }
}
